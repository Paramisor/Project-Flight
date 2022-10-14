using System.IO;
using System.Linq;
using UnityEngine;

public static class MasterConfig
{
    static public bool directoryGetFilesThrewException = false;
    public static bool dataFilePresent = false;

    static public string RootFolder { get { return Application.streamingAssetsPath; } }
    static public string SourceFolder { get { return Path.Combine(RootFolder, "Songs"); } }

    private static string[] trackList;

    public static string[] TrackList
    {
        get 
        {
            if (trackList == null)
                trackList = GetStreamingAssetsFiles();
            return trackList;
        }  
    }

    public static AudioSource audioSource;

    public static SongData songData;

    public static int ThreadCount = 10;

    public static string[] GetStreamingAssetsFiles()
    {
        // On some platforms we are unable to list all files with Directory.GetFiles
        // due to security or architecture restrictions.

        // For those platforms we need to build the files list beforehand in editor.
        if (!directoryGetFilesThrewException)
        {
            try
            {
                return
                    Directory.GetFiles(SourceFolder, "*", SearchOption.AllDirectories)
                    .Where(f => RuntimeAudioClipLoader.AudioLoaderConfig.IsSupportedFormat(f))
                    .Select(f => f.Substring(RootFolder.Length + 7))
                    .ToArray();
            }
            catch
            {
                directoryGetFilesThrewException = true;
            }
        }
        return trackList;
    }
}

