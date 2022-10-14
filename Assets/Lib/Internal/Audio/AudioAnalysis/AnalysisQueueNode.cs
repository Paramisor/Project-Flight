using System;
using System.Threading.Tasks;
using UnityEngine;

public class AnalysisQueueNode : MonoBehaviour
{
    ProducerConsumerQueue pcQueue;

    public Task myTask;

    public SpectralFluxAnalyzer SFA;

    public void Setup(SpectralFluxAnalyzer myFluxAnalyzer, int sampleRate)
    {
        this.SFA = myFluxAnalyzer;
        pcQueue = new ProducerConsumerQueue(1);
    }

    public void ProcessData(double[] segData, float currentTime, int count)
    {
        myTask = pcQueue.EnqueueTask(() => AnalyzeSegment(segData, currentTime, count), pcQueue.cancelSource.Token);
    }

    void AnalyzeSegment(double[] segData, float curSongTime, int count)
    {
        SFA.analyzeSpectrum(Array.ConvertAll(segData, x => (float)x), curSongTime, count);
    }
}
