using System;
using System.Collections.Generic;

[Serializable]

public class DataFile
{
    public int peaks;

    public float min;
    public float max;
    public float avg;

    public List<SpectralFluxInfo> samples;
}
