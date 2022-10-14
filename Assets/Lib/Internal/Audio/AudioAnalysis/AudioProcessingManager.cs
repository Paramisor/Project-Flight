using DSPLib;
using RuntimeAudioClipLoader;
using System;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

public class AudioProcessingManager : MonoBehaviour
{
    static public AudioProcessingManager instance;

    ProducerConsumerQueue pcQueue;

    //public AudioLoader lastAudioLoader;
    //public AudioLoaderConfig audioLoadConfig;

    public SpectralFluxAnalyzer SubBassSpectralFluxAnalyzer;
    public SpectralFluxAnalyzer BassSpectralFluxAnalyzer;
    public SpectralFluxAnalyzer LowMidrangeSpectralFluxAnalyzer;
    public SpectralFluxAnalyzer MidrangeSpectralFluxAnalyzer;
    public SpectralFluxAnalyzer UpperMidrangeSpectralFluxAnalyzer;
    public SpectralFluxAnalyzer PresenceSpectralFluxAnalyzer;
    public SpectralFluxAnalyzer BrillianceSpectralFluxAnalyzer;
    public SpectralFluxAnalyzer BeyondBrillianceSpectralFluxAnalyzer;

    int offset;
    int sampleRate;
    int segmentSize;
    int numChannels;
    int numTotalSamples;
    public int iterations;
    int spectrumSampleSize = 2048;

    static float analysisPercent = 0;

    #region FrequencyBasedVariables

    AnalysisQueueNode SubBassNode;
    AnalysisQueueNode BassNode;
    AnalysisQueueNode LowMidrangeNode;
    AnalysisQueueNode MidrangeNode;
    AnalysisQueueNode UpperMidrangeNode;
    AnalysisQueueNode PresenceNode;
    AnalysisQueueNode BrillianceNode;
    AnalysisQueueNode BeyondBrillianceNode;

    #endregion

    void Awake()
    {
        instance = this;

        SubBassSpectralFluxAnalyzer = new SpectralFluxAnalyzer("SubBass");
        BassSpectralFluxAnalyzer = new SpectralFluxAnalyzer("Bass");
        LowMidrangeSpectralFluxAnalyzer = new SpectralFluxAnalyzer("LowMidrange");
        MidrangeSpectralFluxAnalyzer = new SpectralFluxAnalyzer("Midrange");
        UpperMidrangeSpectralFluxAnalyzer = new SpectralFluxAnalyzer("UpperMidrange");
        PresenceSpectralFluxAnalyzer = new SpectralFluxAnalyzer("Presence");
        BrillianceSpectralFluxAnalyzer = new SpectralFluxAnalyzer("Brilliance");
        BeyondBrillianceSpectralFluxAnalyzer = new SpectralFluxAnalyzer("BeyondBrilliance");
    }

    public void Setup(AudioLoaderConfig alc)
    {
        this.sampleRate = alc.frequency;
        this.numChannels = alc.channels;
        this.pcQueue = alc.pcQ;
        this.segmentSize = Mathf.CeilToInt(alc.samples / alc.pcQ.currentWorkerCount);
    }

    public void Setup(float[] segData, int offset, AudioLoaderConfig alc)
    {
        this.sampleRate = alc.frequency;
        this.numChannels = alc.channels;
        this.pcQueue = alc.pcQ;
        this.segmentSize = Mathf.CeilToInt(alc.samples / alc.pcQ.currentWorkerCount);
        this.offset = offset;

        Task task = pcQueue.EnqueueTask(() => ProcessSegment(segData), pcQueue.cancelSource.Token);
    }

    public void BeginProcessing(AudioClip clip)
    {
        try
        {
            RuntimeAudioClipLoader.Internal.UnityMainThreadRunner.Shared.Enqueue(() => { GetAudioData(clip); });
        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    public virtual void GetAudioData(AudioClip audioClip)
    {
        try
        {
            // Need all audio samples.
            // If in stereo, samples will return with left and right channels interweaved
            // [L,R,L,R,L,R]
            Debug.Log(audioClip);
            float[] multiChannelSamples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(multiChannelSamples, 0);
            sampleRate = audioClip.frequency;
            numTotalSamples = audioClip.samples;
            numChannels = audioClip.channels;
            Task task = pcQueue.EnqueueTask(() => ProcessAudioData(multiChannelSamples), pcQueue.cancelSource.Token);
        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    public virtual void ProcessAudioData(float[] multiChannelSamples)
    {
        try
        {
            int numProcessed = 0;
            float combinedChannelAverage = 0f;
            float[] preProcessedSamples = new float[numTotalSamples];

            for (int q = 0; q < multiChannelSamples.Length; q++)
            {
                combinedChannelAverage += multiChannelSamples[q];

                // Each time we have processed all channels samples for a point in time,
                // we will store the average of the channels combined
                if ((q + 1) % numChannels == 0)
                {
                    preProcessedSamples[numProcessed] = combinedChannelAverage / numChannels;
                    numProcessed++;
                    combinedChannelAverage = 0f;
                }
            }

            RuntimeAudioClipLoader.Internal.UnityMainThreadRunner.Shared.Enqueue(() => PrepQueueNodes(preProcessedSamples));
        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    //static readonly object _locker = new object();
    //bool isInitialized = false;
    public virtual void PrepQueueNodes(float[] preProcessedSamples)
    {
        GameObject Holder = new GameObject();

        SubBassNode = Holder.AddComponent<AnalysisQueueNode>();
        SubBassNode.Setup(SubBassSpectralFluxAnalyzer, sampleRate);

        BassNode = Holder.AddComponent<AnalysisQueueNode>();
        BassNode.Setup(BassSpectralFluxAnalyzer, sampleRate);

        LowMidrangeNode = Holder.AddComponent<AnalysisQueueNode>();
        LowMidrangeNode.Setup(LowMidrangeSpectralFluxAnalyzer, sampleRate);

        MidrangeNode = Holder.AddComponent<AnalysisQueueNode>();
        MidrangeNode.Setup(MidrangeSpectralFluxAnalyzer, sampleRate);

        UpperMidrangeNode = Holder.AddComponent<AnalysisQueueNode>();
        UpperMidrangeNode.Setup(UpperMidrangeSpectralFluxAnalyzer, sampleRate);

        PresenceNode = Holder.AddComponent<AnalysisQueueNode>();
        PresenceNode.Setup(PresenceSpectralFluxAnalyzer, sampleRate);

        BrillianceNode = Holder.AddComponent<AnalysisQueueNode>();
        BrillianceNode.Setup(BrillianceSpectralFluxAnalyzer, sampleRate);

        BeyondBrillianceNode = Holder.AddComponent<AnalysisQueueNode>();
        BeyondBrillianceNode.Setup(BeyondBrillianceSpectralFluxAnalyzer, sampleRate);
        int iterations = preProcessedSamples.Length / spectrumSampleSize;

        Task task = pcQueue.EnqueueTask(() => FastFourierTransform(preProcessedSamples), pcQueue.cancelSource.Token);
    }

    public virtual void FastFourierTransform(float[] preProcessedSamples)
    {
        try
        {
            FFT fft = new FFT();
            fft.Initialize((UInt32)spectrumSampleSize);

            iterations = preProcessedSamples.Length / spectrumSampleSize;

            double[] sampleChunk = new double[spectrumSampleSize];


            for (int c = 0; c < iterations; c++)
            {
                analysisPercent = c / iterations * 100f;

                // Grab the current 1024 chunk of audio sample data
                Array.Copy(preProcessedSamples, c * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

                // Apply our chosen FFT Window
                double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
                double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
                double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

                // Perform the FFT and convert output (complex numbers) to Magnitude
                ComplexNum[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
                double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                //Separate notes
                //48000 / 2 / 512 = 46.875
                float curSongTime = getTimeFromIndex(c) * spectrumSampleSize;
                //(getTimeFromIndex(c + ((count * segmentSize) / spectrumSampleSize)) * spectrumSampleSize);

                float binSize = (sampleRate) / (scaledFFTSpectrum.Length);
                int range;
                int lowEnd;

                //Sub-bass	20 to 60 Hz
                lowEnd = Mathf.RoundToInt(20 / binSize);
                range = Mathf.RoundToInt(60 / binSize) - lowEnd;
                double[] SubBass = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, SubBass, 0, range);
                if (SubBassSpectralFluxAnalyzer.NumSamples == 0)
                { SubBassSpectralFluxAnalyzer.NumSamples = range; }
                SubBassNode.ProcessData(SubBass, curSongTime, c);

                //Bass	60 to 250 Hz
                lowEnd = Mathf.RoundToInt(60 / binSize);
                range = Mathf.RoundToInt(250 / binSize) - lowEnd;
                double[] Bass = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, Bass, 0, range);
                if (BassSpectralFluxAnalyzer.NumSamples == 0)
                { BassSpectralFluxAnalyzer.NumSamples = range; }
                BassNode.ProcessData(Bass, curSongTime, c);

                //Low midrange	250 to 500 Hz
                lowEnd = Mathf.RoundToInt(250 / binSize);
                range = Mathf.RoundToInt(500 / binSize) - lowEnd;
                double[] LowMidrange = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, LowMidrange, 0, range);
                if (LowMidrangeSpectralFluxAnalyzer.NumSamples == 0)
                { LowMidrangeSpectralFluxAnalyzer.NumSamples = range; }
                LowMidrangeNode.ProcessData(LowMidrange, curSongTime, c);

                //Midrange	500 Hz to 2 kHz
                lowEnd = Mathf.RoundToInt(500 / binSize);
                range = Mathf.RoundToInt(2000 / binSize) - lowEnd;
                double[] Midrange = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, Midrange, 0, range);
                if (MidrangeSpectralFluxAnalyzer.NumSamples == 0)
                { MidrangeSpectralFluxAnalyzer.NumSamples = range; }
                MidrangeNode.ProcessData(Midrange, curSongTime, c);

                //Upper midrange	2 to 4 kHz
                lowEnd = Mathf.RoundToInt(2000 / binSize);
                range = Mathf.RoundToInt(4000 / binSize) - lowEnd;
                double[] UpperMidrange = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, UpperMidrange, 0, range);
                if (UpperMidrangeSpectralFluxAnalyzer.NumSamples == 0)
                { UpperMidrangeSpectralFluxAnalyzer.NumSamples = range; }
                UpperMidrangeNode.ProcessData(UpperMidrange, curSongTime, c);

                //Presence	4 to 6 kHz
                lowEnd = Mathf.RoundToInt(4000 / binSize);
                range = Mathf.RoundToInt(6000 / binSize) - lowEnd;
                double[] Presence = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, Presence, 0, range);
                if (PresenceSpectralFluxAnalyzer.NumSamples == 0)
                { PresenceSpectralFluxAnalyzer.NumSamples = range; }
                PresenceNode.ProcessData(Presence, curSongTime, c);

                //Brilliance	6 to 20 kHz
                lowEnd = Mathf.RoundToInt(6000 / binSize);
                range = Mathf.RoundToInt(20000 / binSize) - lowEnd;
                double[] Brilliance = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, Brilliance, 0, range);
                if (BrillianceSpectralFluxAnalyzer.NumSamples == 0)
                { BrillianceSpectralFluxAnalyzer.NumSamples = range; }
                BrillianceNode.ProcessData(Brilliance, curSongTime, c);

                //BeyondBrilliance	20 to 44 kHz
                lowEnd = Mathf.RoundToInt(20000f / binSize);
                range = Mathf.RoundToInt(44000f / binSize) - lowEnd;
                double[] BeyondBrilliance = new double[range];
                Array.Copy(scaledFFTSpectrum, lowEnd, BeyondBrilliance, 0, range);
                if (BeyondBrillianceSpectralFluxAnalyzer.NumSamples == 0)
                {
                    BeyondBrillianceSpectralFluxAnalyzer.NumSamples = range;
                    BeyondBrillianceSpectralFluxAnalyzer.iterations = iterations;
                }
                BeyondBrillianceNode.ProcessData(BeyondBrilliance, curSongTime, c);
            }

            //config.UnityMainThreadRunner.Enqueue(() => AnalysisComplete(iterations));
            //PCQ.Dispose();
        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    public virtual void AnalysisComplete(int iterations)
    {
        Debug.Log("Play");
        //SongController.instance.audioSource.Play();
    }

    /// <summary>
    /// Use point in array to determine time
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public float getTimeFromIndex(int index)
    {
        return ((1f / (float)this.sampleRate) * index);
    }


    void ProcessSegment(float[] segData)
    {
        float combinedChannelAverage = 0f;
        int numProcessed = 0;
        float[] preProcessedSamples = new float[segData.Length];

        for (int q = 0; q < segData.Length; q++)
        {
            combinedChannelAverage += segData[q];

            // Each time we have processed all channels samples for a point in time,
            // we will store the average of the channels combined
            if ((q + 1) % numChannels == 0)
            {
                preProcessedSamples[numProcessed] = combinedChannelAverage / numChannels;
                numProcessed++;
                combinedChannelAverage = 0f;
            }
        }
        Task task = pcQueue.EnqueueTask(() => AnalyzeSegment(preProcessedSamples), pcQueue.cancelSource.Token);
    }

    void AnalyzeSegment(float[] preProcessedSamples)
    {
        int iterations = preProcessedSamples.Length / spectrumSampleSize;

        FFT fft = new FFT();
        fft.Initialize((UInt32)spectrumSampleSize);

        double[] sampleChunk = new double[spectrumSampleSize];
        for (int c = 0; c < iterations; c++)
        {
            // Grab the current 1024 chunk of audio sample data
            Array.Copy(preProcessedSamples, c * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

            // Apply our chosen FFT Window
            double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
            double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
            double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

            // Perform the FFT and convert output (complex numbers) to Magnitude
            ComplexNum[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
            double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
            scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

            //Separate notes
            //48000 / 2 / 512 = 46.875
            float curSongTime = getTimeFromIndex(c + offset) * spectrumSampleSize;
            //(getTimeFromIndex(c + ((count * segmentSize) / spectrumSampleSize)) * spectrumSampleSize);
            //float curSongTime = (timeRefInArray * );
            float binSize = (sampleRate) / (scaledFFTSpectrum.Length);
            int range;
            int lowEnd;

            //Sub-bass	20 to 60 Hz
            lowEnd = Mathf.RoundToInt(20 / binSize);
            range = Mathf.RoundToInt(60 / binSize) - lowEnd;
            double[] SubBass = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, SubBass, 0, range);
            if (SubBassSpectralFluxAnalyzer.NumSamples == 0)
            { SubBassSpectralFluxAnalyzer.NumSamples = range; }
            //SubBassThread = 
            //AnalyzeSegment(SubBass, curSongTime, "SubBass");
            SubBassSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(SubBass, x => (float)x), curSongTime);
            //StartCoroutine(SubBassThread);

            //Bass	60 to 250 Hz
            lowEnd = Mathf.RoundToInt(60 / binSize);
            range = Mathf.RoundToInt(250 / binSize) - lowEnd;
            double[] Bass = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, Bass, 0, range);
            if (BassSpectralFluxAnalyzer.NumSamples == 0)
            { BassSpectralFluxAnalyzer.NumSamples = range; }
            //BassThread = 
            //AnalyzeSegment(Bass, curSongTime, "Bass");
            BassSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(Bass, x => (float)x), curSongTime);
            //StartCoroutine(BassThread);

            //Low midrange	250 to 500 Hz
            lowEnd = Mathf.RoundToInt(250 / binSize);
            range = Mathf.RoundToInt(500 / binSize) - lowEnd;
            double[] LowMidrange = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, LowMidrange, 0, range);
            if (LowMidrangeSpectralFluxAnalyzer.NumSamples == 0)
            { LowMidrangeSpectralFluxAnalyzer.NumSamples = range; }
            //LowMidrangeThread = 
            //AnalyzeSegment(LowMidrange, curSongTime, "LowMidrange");
            //StartCoroutine(LowMidrangeThread);
            LowMidrangeSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(LowMidrange, x => (float)x), curSongTime);

            //Midrange	500 Hz to 2 kHz
            lowEnd = Mathf.RoundToInt(500 / binSize);
            range = Mathf.RoundToInt(2000 / binSize) - lowEnd;
            double[] Midrange = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, Midrange, 0, range);
            if (MidrangeSpectralFluxAnalyzer.NumSamples == 0)
            { MidrangeSpectralFluxAnalyzer.NumSamples = range; }
            //MidrangeThread = 
            //AnalyzeSegment(Midrange, curSongTime, "Midrange");
            //StartCoroutine(MidrangeThread);
            MidrangeSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(Midrange, x => (float)x), curSongTime);

            //Upper midrange	2 to 4 kHz
            lowEnd = Mathf.RoundToInt(2000 / binSize);
            range = Mathf.RoundToInt(4000 / binSize) - lowEnd;
            double[] UpperMidrange = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, UpperMidrange, 0, range);
            if (UpperMidrangeSpectralFluxAnalyzer.NumSamples == 0)
            { UpperMidrangeSpectralFluxAnalyzer.NumSamples = range; }
            //UpperMidrangeThread = 
            //AnalyzeSegment(UpperMidrange, curSongTime, "UpperMidrange");
            //StartCoroutine(UpperMidrangeThread);
            lock (UpperMidrangeSpectralFluxAnalyzer)
            {
                UpperMidrangeSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(UpperMidrange, x => (float)x), curSongTime);
            }
            //Presence	4 to 6 kHz
            lowEnd = Mathf.RoundToInt(4000 / binSize);
            range = Mathf.RoundToInt(6000 / binSize) - lowEnd;
            double[] Presence = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, Presence, 0, range);
            if (PresenceSpectralFluxAnalyzer.NumSamples == 0)
            { PresenceSpectralFluxAnalyzer.NumSamples = range; }
            //PresenceThread = 
            //AnalyzeSegment(Presence, curSongTime, "Presence");
            //StartCoroutine(PresenceThread);
            PresenceSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(Presence, x => (float)x), curSongTime);

            //Brilliance	6 to 20 kHz
            lowEnd = Mathf.RoundToInt(6000 / binSize);
            range = Mathf.RoundToInt(20000 / binSize) - lowEnd;
            double[] Brilliance = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, Brilliance, 0, range);
            if (BrillianceSpectralFluxAnalyzer.NumSamples == 0)
            { BrillianceSpectralFluxAnalyzer.NumSamples = range; }
            //BrillianceThread = 
            //AnalyzeSegment(Brilliance, curSongTime, "Brilliance");
            //StartCoroutine(BrillianceThread);
            BrillianceSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(Brilliance, x => (float)x), curSongTime);

            //BeyondBrilliance	20 to 44 kHz
            lowEnd = Mathf.RoundToInt(20000f / binSize);
            range = Mathf.RoundToInt(44000f / binSize) - lowEnd;
            double[] BeyondBrilliance = new double[range];
            Array.Copy(scaledFFTSpectrum, lowEnd, BeyondBrilliance, 0, range);
            if (BeyondBrillianceSpectralFluxAnalyzer.NumSamples == 0)
            { BeyondBrillianceSpectralFluxAnalyzer.NumSamples = range; }
            //BeyondBrillianceThread = 
            //AnalyzeSegment(BeyondBrilliance, curSongTime, "BeyondBrilliance");
            //StartCoroutine(BeyondBrillianceThread);
            BeyondBrillianceSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(BeyondBrilliance, x => (float)x), curSongTime);
        }
    }

}
