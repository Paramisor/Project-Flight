using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpectralFluxAnalyzer
{

    private int numSamples = 0;
    public int NumSamples
    {
        get
        {
            return numSamples;
        }
        set
        {
            numSamples = value;
            curSpectrum = new float[numSamples];
            prevSpectrum = new float[numSamples];
        }
    }

    public string SpectrumRange;

    // Sensitivity multiplier to scale the average threshold.
    // In this case, if a rectified spectral flux sample is > 1.5 times the average, it is a peak
    public float thresholdMultiplier = 1.25f;

    // Number of samples to average in our window
    public int thresholdWindowSize = 15;

    int indexToProcess;

    public DataFile AnalyzedData;

    float[] curSpectrum;
    float[] prevSpectrum;

    public SpectralFluxAnalyzer(string range)
    {
        AnalyzedData = new DataFile();
        AnalyzedData.samples = new List<SpectralFluxInfo>();

        //Set which range we are analyzing
        SpectrumRange = range;

        // Start processing from middle of first window and increment by 1 from there
        indexToProcess = thresholdWindowSize / 2;

        curSpectrum = new float[NumSamples];
        prevSpectrum = new float[NumSamples];
    }

    /// <summary>
    ///  Updating and keeping the most recent spectrum data for comparison
    /// </summary>
    /// <param name="spectrum"></param>
    public void setCurSpectrum(float[] spectrum)
    {
        if (spectrum.Length > curSpectrum.Length)
            curSpectrum.CopyTo(prevSpectrum, 0);
        spectrum.CopyTo(curSpectrum, 0);
    }

    /// <summary>
    /// Analyze the entire frequency spectrum at once
    /// </summary>
    /// <returns></returns>
    float calculateRectifiedSpectralFlux()
    {
        float sum = 0f;

        // Aggregate positive changes in spectrum data
        for (int i = 0; i < NumSamples; i++)
        {
            sum += Mathf.Max(0f, curSpectrum[i] - prevSpectrum[i]);
            //Debug.Log(sum);
        }
        //Debug.Log(NumSamples + " : " + raiseProgressChangedStopwatch.ElapsedTicks);
        return sum;
    }

    float getFluxThreshold(int spectralFluxIndex)
    {
        // How many samples in the past and future we include in our average
        int windowStartIndex = Mathf.Max(0, spectralFluxIndex - thresholdWindowSize / 2);
        int windowEndIndex = Mathf.Min(AnalyzedData.samples.Count - 1, spectralFluxIndex + thresholdWindowSize / 2);

        // Add up our spectral flux over the window
        float sum = 0f;
        for (int i = windowStartIndex; i < windowEndIndex; i++)
        {
            sum += AnalyzedData.samples[i].spectralFlux;
        }

        // Return the average multiplied by our sensitivity multiplier
        float avg = sum / (windowEndIndex - windowStartIndex);
        return avg * thresholdMultiplier;
    }


    //public void InitializeSpectrumList(int capacity)
    //{
    //	for (int i = 0; i < capacity; i++)
    //	{
    //		spectrumSamples.Add
    //	}
    //}

    public void analyzeSpectrum(float[] spectrum, float time)
    {
        // Set spectrum
        //if (spectrum.Length > numSamples)
        //{ NumSamples = spectrum.Length; }
        //Debug.Log(spectrum.Length + " : " + numSamples);
        setCurSpectrum(spectrum);

        // Get current spectral flux from spectrum
        SpectralFluxInfo curInfo = new SpectralFluxInfo
        {
            time = time,
            spectralFlux = calculateRectifiedSpectralFlux()
        };
        AnalyzedData.samples.Add(curInfo);

        AnalyzedData.avg += curInfo.spectralFlux;

        if (curInfo.spectralFlux > AnalyzedData.max)
        {
            AnalyzedData.max = curInfo.spectralFlux;
        }

        // We have enough samples to detect a peak
        if (AnalyzedData.samples.Count >= thresholdWindowSize)
        {
            // Get Flux threshold of time window surrounding index to process
            AnalyzedData.samples[indexToProcess].threshold = getFluxThreshold(indexToProcess);

            // Only keep amp amount above threshold to allow peak filtering
            AnalyzedData.samples[indexToProcess].prunedSpectralFlux = getPrunedSpectralFlux(indexToProcess);

            // Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
            int indexToDetectPeak = indexToProcess - 1;

            bool curPeak = isPeak(indexToDetectPeak);

            if (curPeak)
            {
                AnalyzedData.samples[indexToDetectPeak].isPeak = true;
                AnalyzedData.peaks++;
            }
            indexToProcess++;

        }
        else
        {
            //Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", spectralFluxSamples.Count, thresholdWindowSize));
        }
    }

    int currentStage = 1;
    public int iterations = 1;

    public void analyzeSpectrum(float[] spectrum, float time, int index)
    {
        // Set spectrum
        //if (spectrum.Length > numSamples)
        //{ NumSamples = spectrum.Length; }
        //Debug.Log(spectrum.Length + " : " + numSamples);
        setCurSpectrum(spectrum);

        // Get current spectral flux from spectrum
        SpectralFluxInfo curInfo = new SpectralFluxInfo
        {
            time = time,
            spectralFlux = calculateRectifiedSpectralFlux()
        };
        AnalyzedData.samples.Add(curInfo);

        AnalyzedData.avg = (AnalyzedData.avg + curInfo.spectralFlux) / 2;

        if (curInfo.spectralFlux > AnalyzedData.max)
        {
            AnalyzedData.max = curInfo.spectralFlux;
        }
        if (curInfo.spectralFlux < AnalyzedData.min)
        {
            AnalyzedData.min = curInfo.spectralFlux;
        }

        // We have enough samples to detect a peak
        if (AnalyzedData.samples.Count >= thresholdWindowSize)
        {
            // Get Flux threshold of time window surrounding index to process
            AnalyzedData.samples[indexToProcess].threshold = getFluxThreshold(indexToProcess);

            // Only keep amp amount above threshold to allow peak filtering
            AnalyzedData.samples[indexToProcess].prunedSpectralFlux = getPrunedSpectralFlux(indexToProcess);

            // Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
            int indexToDetectPeak = indexToProcess - 1;

            bool curPeak = isPeak(indexToDetectPeak);

            if (curPeak)
            {
                AnalyzedData.samples[indexToDetectPeak].isPeak = true;
                AnalyzedData.peaks++;
            }
            indexToProcess++;


            if (SpectrumRange == "BeyondBrilliance")
            {

                if (index >= Mathf.RoundToInt(((iterations) / 10f) * currentStage))
                {
                    //Debug.Log(index + " > " + ((iterations) / 10f) * currentStage + " : " + iterations + " : " + currentStage);
                    RuntimeAudioClipLoader.Internal.UnityMainThreadRunner.Shared.Enqueue(() => UpdateSongData());
                    //LevelController.instance.GenerateSegment(currentStage - 1));
                    //Debug.Log("Advance  : " + currentStage);

                    currentStage++;
                }
                else if (currentStage > 9)
                {
                    //Debug.Log(index + " > " + ((iterations) / 10f) * currentStage + " : " + iterations + " : " + currentStage);
                    if (index + 1 >= Mathf.RoundToInt(((iterations) / 10f) * currentStage))
                    {
                        Debug.Log("Advance  : " + currentStage);

                        RuntimeAudioClipLoader.Internal.UnityMainThreadRunner.Shared.Enqueue(() => UpdateSongData());
                        //MasterConfig.Instance.audioLoadConfig.UnityMainThreadRunner.Enqueue(() => LevelController.instance.GenerateSegment(currentStage));
                    }
                }
                //FileManager.instance.PCQ.EnqueueTask(() => SongController.instance.GenerateSegment(index), FileManager.instance.PCQ.cancelSource.Token);
            }
        }
        else
        {
            //Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", AnalyzedData.samples.Count, thresholdWindowSize));
        }
    }

    //public void analyzeSpectrum(float[] spectrum, float time, int index)
    //{
    //	// Set spectrum
    //	//if (spectrum.Length > numSamples)
    //	//{ NumSamples = spectrum.Length; }
    //	//Debug.Log(spectrum.Length + " : " + numSamples);
    //	addSpectrum(spectrum);
    //
    //	// Get current spectral flux from spectrum
    //	SpectralFluxInfo curInfo = new SpectralFluxInfo
    //	{
    //		time = time,
    //		spectralFlux = calculateRectifiedSpectralFlux(index)
    //	};
    //	spectralFluxSamples.Add(curInfo);
    //
    //	avgValue += curInfo.spectralFlux;
    //
    //	if (curInfo.spectralFlux > maxValue)
    //	{
    //		maxValue = curInfo.spectralFlux;
    //	}
    //	if (curInfo.spectralFlux < minValue)
    //	{
    //		minValue = curInfo.spectralFlux;
    //	}
    //
    //	// We have enough samples to detect a peak
    //	if (spectralFluxSamples.Count >= thresholdWindowSize)
    //	{
    //		// Get Flux threshold of time window surrounding index to process
    //		//spectralFluxSamples[indexToProcess] = new SpectralFluxInfo(spectralFluxSamples[indexToProcess], getFluxThreshold(indexToProcess));
    //
    //		// Only keep amp amount above threshold to allow peak filtering
    //		//spectralFluxSamples[indexToProcess] = new SpectralFluxInfo(getPrunedSpectralFlux(indexToProcess), spectralFluxSamples[indexToProcess]);
    //
    //		// Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
    //		int indexToDetectPeak = indexToProcess - 1;
    //
    //		bool curPeak = isPeak(indexToDetectPeak);
    //
    //		if (curPeak)
    //		{
    //			//spectralFluxSamples[indexToDetectPeak] = new SpectralFluxInfo(spectralFluxSamples[indexToProcess], true);
    //		}
    //		indexToProcess++;
    //	}
    //	else
    //	{
    //		//Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", spectralFluxSamples.Count, thresholdWindowSize));
    //	}
    //}

    float getPrunedSpectralFlux(int spectralFluxIndex)
    {
        return Mathf.Max(0f, AnalyzedData.samples[spectralFluxIndex].spectralFlux - AnalyzedData.samples[spectralFluxIndex].threshold);
    }

    bool isPeak(int spectralFluxIndex)
    {
        if (AnalyzedData.samples[spectralFluxIndex].prunedSpectralFlux > AnalyzedData.samples[spectralFluxIndex + 1].prunedSpectralFlux &&
            AnalyzedData.samples[spectralFluxIndex].prunedSpectralFlux > AnalyzedData.samples[spectralFluxIndex - 1].prunedSpectralFlux)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateSongData()
    {
        //string path = MasterConfig.RootFolder + '\\' + audioSource.clip.name + "\\Bass.json";

        //if (System.IO.File.Exists(path))
        //{
        Debug.Log("Loading");
        //	FBT.statusMessage = "Loading";
        //	songData = LoadData();
        //}
        //else
        //{
        //	Debug.Log("Create");
        //	FBT.statusMessage = "Creating";
        //	StartCoroutine(CreateData());
        //}

        //Debug.Log("Generate : " + currentStage);
        MasterConfig.songData = new SongData();

        MasterConfig.songData.SubBass = AudioProcessingManager.instance.SubBassSpectralFluxAnalyzer.AnalyzedData;
        MasterConfig.songData.Bass = AudioProcessingManager.instance.BassSpectralFluxAnalyzer.AnalyzedData;
        MasterConfig.songData.LowMidrange = AudioProcessingManager.instance.LowMidrangeSpectralFluxAnalyzer.AnalyzedData;
        MasterConfig.songData.Midrange = AudioProcessingManager.instance.MidrangeSpectralFluxAnalyzer.AnalyzedData;
        MasterConfig.songData.UpperMidrange = AudioProcessingManager.instance.UpperMidrangeSpectralFluxAnalyzer.AnalyzedData;
        MasterConfig.songData.Presence = AudioProcessingManager.instance.PresenceSpectralFluxAnalyzer.AnalyzedData;
        MasterConfig.songData.Brilliance = AudioProcessingManager.instance.BrillianceSpectralFluxAnalyzer.AnalyzedData;
        MasterConfig.songData.BeyondBrilliance = AudioProcessingManager.instance.BeyondBrillianceSpectralFluxAnalyzer.AnalyzedData;

        //FileExportHandler.CreateData();
    }
}


[Serializable]
public class SpectralFluxInfo
{
    public float time;
    public float spectralFlux;
    public float threshold;
    public float prunedSpectralFlux;
    public bool isPeak;
}
