using RuntimeAudioClipLoader;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;

    public AudioLoader lastAudioLoader;
    //public string currentSongName;

    //public AudioLoaderConfig audioLoadConfig;

    public FileExportHandler FileExportHandler;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance is null)
        {
            Instance = this as GameManager;
        }


        FileExportHandler = this.GetComponentInChildren<FileExportHandler>();
    }

    public string GetPath(string songName)
    {
        return Path.Combine(MasterConfig.SourceFolder + '\\' + songName);
    }

    
}
