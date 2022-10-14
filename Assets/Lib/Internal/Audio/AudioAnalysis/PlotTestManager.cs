using System.Collections;
using UnityEngine;

public class PlotTestManager : MonoBehaviour
{
    [Range(0, 3)]
    public float GlobalScalar = 1;

    public int selectedSong;

    public PlotController SubBassPlot;
    public PlotController BassPlot;
    public PlotController LowMidPlot;
    public PlotController MidPlot;
    public PlotController UpperMidPlot;
    public PlotController PresensePlot;
    public PlotController BrilliancePlot;
    public PlotController BeyondBrilliancePlot;


    public PlotController SubBassPlot2;
    public PlotController BassPlot2;
    public PlotController LowMidPlot2;
    public PlotController MidPlot2;
    public PlotController UpperMidPlot2;
    public PlotController PresensePlot2;
    public PlotController BrilliancePlot2;
    public PlotController BeyondBrilliancePlot2;

    // Start is called before the first frame update
    void Start()
    {
        GetPlotControllers();

        StartCoroutine(Begin());
    }

    IEnumerator Begin()
    {
        yield return new WaitForSeconds(0.01f);
        FileImportHandler.instance.SetupElements(MasterConfig.audioSource);
        yield return new WaitForSeconds(0.1f);
        FileImportHandler.instance.LoadPresetSong(MasterConfig.TrackList[selectedSong]);
        yield return new WaitForSeconds(0.3f);
        FileImportHandler.instance.IsFilePresent(MasterConfig.TrackList[selectedSong]);
        yield return new WaitForSeconds(0.3f);
        MasterConfig.audioSource.Play();
    }

    private void GetPlotControllers()
    {
        //FullSpectrumPlot = GameObject.Find("FullSpectrumPlot").GetComponent<PlotController>();
        SubBassPlot = GameObject.Find("SubBassPlot").GetComponent<PlotController>();
        BassPlot = GameObject.Find("BassPlot").GetComponent<PlotController>();
        LowMidPlot = GameObject.Find("LowMidPlot").GetComponent<PlotController>();
        MidPlot = GameObject.Find("MidPlot").GetComponent<PlotController>();
        UpperMidPlot = GameObject.Find("UpperMidPlot").GetComponent<PlotController>();
        PresensePlot = GameObject.Find("PresensePlot").GetComponent<PlotController>();
        BrilliancePlot = GameObject.Find("BrilliancePlot").GetComponent<PlotController>();
        BeyondBrilliancePlot = GameObject.Find("BeyondBrilliancePlot").GetComponent<PlotController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MasterConfig.songData != null && MasterConfig.songData.SubBass.samples.Count > 0)
        {
            int indexToPlot = getIndexFromTime(MasterConfig.audioSource.time) / 2048;
            Debug.Log("indexToPlot : " + indexToPlot);

            UpdatePlots(indexToPlot);

            UpdateGlobalScalar();


#if UNITY_EDITOR
            //if (indexToPlot > 1500)
            //    UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void UpdatePlots(int indexToPlot)
    {
        SubBassPlot.updatePlot(MasterConfig.songData.SubBass.samples, indexToPlot);
        BassPlot.updatePlot(MasterConfig.songData.Bass.samples, indexToPlot);
        LowMidPlot.updatePlot(MasterConfig.songData.LowMidrange.samples, indexToPlot);
        MidPlot.updatePlot(MasterConfig.songData.Midrange.samples, indexToPlot);
        UpperMidPlot.updatePlot(MasterConfig.songData.UpperMidrange.samples, indexToPlot);
        PresensePlot.updatePlot(MasterConfig.songData.Presence.samples, indexToPlot);
        BrilliancePlot.updatePlot(MasterConfig.songData.Brilliance.samples, indexToPlot);
        BeyondBrilliancePlot.updatePlot(MasterConfig.songData.BeyondBrilliance.samples, indexToPlot);

        SubBassPlot2.updatePlot(MasterConfig.songData.SubBass.samples, indexToPlot);
        BassPlot2.updatePlot(MasterConfig.songData.Bass.samples, indexToPlot);
        LowMidPlot2.updatePlot(MasterConfig.songData.LowMidrange.samples, indexToPlot);
        MidPlot2.updatePlot(MasterConfig.songData.Midrange.samples, indexToPlot);
        UpperMidPlot2.updatePlot(MasterConfig.songData.UpperMidrange.samples, indexToPlot);
        PresensePlot2.updatePlot(MasterConfig.songData.Presence.samples, indexToPlot);
        BrilliancePlot2.updatePlot(MasterConfig.songData.Brilliance.samples, indexToPlot);
        BeyondBrilliancePlot2.updatePlot(MasterConfig.songData.BeyondBrilliance.samples, indexToPlot);
    }

    private void UpdateGlobalScalar()
    {
        SubBassPlot.globalScaleMultiplier = GlobalScalar;
        BassPlot.globalScaleMultiplier = GlobalScalar;
        LowMidPlot.globalScaleMultiplier = GlobalScalar;
        MidPlot.globalScaleMultiplier = GlobalScalar;
        UpperMidPlot.globalScaleMultiplier = GlobalScalar;
        PresensePlot.globalScaleMultiplier = GlobalScalar;
        BrilliancePlot.globalScaleMultiplier = GlobalScalar;
        BeyondBrilliancePlot.globalScaleMultiplier = GlobalScalar;
    }

    public int getIndexFromTime(float curTime)
    {
        float lengthPerSample = MasterConfig.audioSource.clip.length / MasterConfig.audioSource.clip.samples;
        return Mathf.FloorToInt(curTime / lengthPerSample);
    }
}
