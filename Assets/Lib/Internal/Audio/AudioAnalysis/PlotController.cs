using System.Collections.Generic;
using UnityEngine;

public class PlotController : MonoBehaviour
{

    public List<Transform> plotPoints;
    public int displayWindowLength = 1500;
    [Range(0, 15)]
    public float pointHeightMultiplier = 0.06f;
    public float plotScaleMultiplier = 0.06f;
    public float globalScaleMultiplier = 1f;


    void Start()
    {
        plotPoints = new List<Transform>();

        float localWidth = transform.Find("Point/BasePoint").localScale.x;
        // -n/2...0...n/2
        for (int i = 0; i < displayWindowLength; i++)
        {
            //Instantiate point
            Transform t = (Instantiate(Resources.Load("Point"), transform) as GameObject).transform;

            // Set position
            float pointX = (displayWindowLength / 2) * -1 * localWidth + i * localWidth;
            t.localPosition = new Vector3(pointX, t.localPosition.y, t.localPosition.z);

            plotPoints.Add(t);
        }
    }

    public void updatePlot(List<SpectralFluxInfo> pointInfo, int curIndex = -1)
    {
        if (plotPoints.Count < displayWindowLength - 1)
            return;

        int numPlotted = 0;
        int windowStart;
        int windowEnd;

        if (curIndex > 0)
        {
            windowStart = Mathf.Max(0, curIndex - displayWindowLength / 2);
            windowEnd = Mathf.Min(curIndex + displayWindowLength / 2, pointInfo.Count - 1);
        }
        else
        {
            windowStart = Mathf.Max(0, pointInfo.Count - displayWindowLength - 1);
            windowEnd = Mathf.Min(windowStart + displayWindowLength, pointInfo.Count);
        }

        for (int i = windowStart; i < windowEnd; i++)
        {
            int plotIndex = numPlotted;
            numPlotted++;

            Transform fluxPoint = plotPoints[plotIndex].Find("FluxPoint");
            Transform threshPoint = plotPoints[plotIndex].Find("ThreshPoint");
            Transform peakPoint = plotPoints[plotIndex].Find("PeakPoint");

            if (pointInfo[i].isPeak)
            {
                setPointHeight(peakPoint, pointInfo[i].spectralFlux * 2);
                setPointHeight(fluxPoint, 0f);
            }
            else
            {
                setPointHeight(fluxPoint, pointInfo[i].spectralFlux);
                setPointHeight(peakPoint, 0f);
            }
            setPointHeight(threshPoint, pointInfo[i].threshold);
        }
    }

    public void setPointHeight(Transform point, float height)
    {

        //point.localScale = new Vector3(point.localScale.x * plotScaleMultiplier, point.localScale.y * plotScaleMultiplier, point.localPosition.z * plotScaleMultiplier);
        point.localPosition = new Vector3(point.localPosition.x, height * pointHeightMultiplier * globalScaleMultiplier, point.localPosition.z);
    }
}
