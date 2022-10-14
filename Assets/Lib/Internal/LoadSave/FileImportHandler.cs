using RuntimeAudioClipLoader;
using RuntimeAudioClipLoaderDemo;
using SimpleFileBrowser;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class FileImportHandler : MonoBehaviour
{
    static public FileImportHandler instance;

    public ProducerConsumerQueue PCQ;
    Coroutine lastCoroutine;
    IEnumerator BrowserThread;
    public IEnumerator LoadThread;

    public string filePath;
    public AudioLoader lastAudioLoader;
    protected AudioClip clip;
    //protected AudioSource audioSource;

    public System.Diagnostics.Stopwatch loadTimer;
    public EnumSelectionGrid<LoadMethod> loadMethod = new EnumSelectionGrid<LoadMethod>();
    public EnumSelectionGrid<PreferredDecoder> preferredDecoder = new EnumSelectionGrid<PreferredDecoder>();

    public string statusMessage;
    static public float LoadStatus;

    void Awake()
    {
        instance = this;
        AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE");
        //if (result == AndroidRuntimePermissions.Permission.Granted)
        //	UnityEngine.Debug.Log("We have permission to access external storage!");
        //else
        //	UnityEngine.Debug.Log("Permission state: " + result);

        filePath = Application.dataPath + "\\StreamingAssets";
    }
    public string GetPath(string songName)
    {
        return Path.Combine(MasterConfig.SourceFolder + '\\' + songName);
    }

    public void SetupElements(AudioSource source)
    {
        //audioSource = source;
        PCQ = new ProducerConsumerQueue(MasterConfig.ThreadCount);
    }

    public void BeginBrowse()
    {
        if (BrowserThread == null)
        {
            BrowserThread = ShowLoadDialogCoroutine();
            StartCoroutine(BrowserThread);
        }
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        UnityEngine.Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        if (FileBrowser.Success)
        {
            // If a file was chosen, read its bytes via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
        }
        else
        {
            UnityEngine.Debug.Log("No Result Found..");
        }
    }

    public void LoadSong(string songName)
    {
        string path = GetPath(songName);
        if (!path.StartsWith("jar:") && !path.StartsWith("http:")) path = "file://" + path;
        //Debug.Log(path);
        StartLoadCoroutine(path);
    }

    public void LoadPresetSong(string path)
    {
        path = GetPath(path);
        if (!path.StartsWith("jar:") && !path.StartsWith("http:")) path = "file://" + path;
        //Debug.Log(path);
        StartLoadCoroutine(path);
    }

    public virtual void StartLoadCoroutine(string url)
    {
        if (lastCoroutine != null)
        {
            StopCoroutine("PrepDownload");
            lastCoroutine = null;
        }

        lastCoroutine = StartCoroutine(PrepDownload(url));
    }


    // Unity unifies file system access in the WWW class, we can use the WWW class to get data both from internet and from local file system on any platform.
    public virtual IEnumerator PrepDownload(string url)
    {
        if (lastAudioLoader != null) lastAudioLoader.Destroy();
        lastAudioLoader = null;
        clip = null;
        //PathTxt.text = SystemInfo.processorCount + " Cores"; //getNumCores().ToString();
        Debug.Log("url : " + url);

        var isFromInternet = url.IndexOf("http", StringComparison.InvariantCultureIgnoreCase) != -1;

        var www = new WWW(url);
        while (!www.isDone)
        {
            if (isFromInternet)
                statusMessage = "Downloading " + url + "\nDownload progress: " + (int)(www.progress * 100) + "%";
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Getting data");

        if (www.bytes.Length == 0)
        {
            statusMessage = "Error: downloaded zero bytes from " + url;
        }
        else
        {
            statusMessage = "Loading " + url;

            if (loadMethod.value == LoadMethod.StreamInUnityThread) statusMessage = "Streaming";
            else loadTimer = System.Diagnostics.Stopwatch.StartNew();

            AudioLoaderConfig sourceConfig = new AudioLoaderConfig();

            sourceConfig.DataStream = new MemoryStream(www.bytes);
            sourceConfig.PreferredDecoder = PreferredDecoder.PreferNative; // preferredDecoder.value;
            int position = url.LastIndexOf('\\');
            sourceConfig.UnityAudioClipName = url.Substring(position + 1);
            sourceConfig.LoadMethod = LoadMethod.AllPartsInBackgroundThread; // loadMethod.value;
            sourceConfig.pcQ = PCQ;

            lastAudioLoader = new AudioLoader(sourceConfig);

            sourceConfig.frequency = lastAudioLoader.AudioClip.frequency;
            sourceConfig.channels = lastAudioLoader.AudioClip.channels;
            sourceConfig.samples = lastAudioLoader.AudioClip.samples;

            if (sourceConfig.UnityAudioClipName.EndsWith(".mp3"))
            {
                sourceConfig.AudioFormat = SelectDecoder.MP3;
            }

            for (int i = 0; i < MasterConfig.ThreadCount; i++)
            {
                AudioLoaderConfig thisConfig = sourceConfig.Clone();
                thisConfig.DataStream = new MemoryStream(www.bytes);
                lastAudioLoader.MTConfigs.Add(thisConfig);
            }

            AudioProcessingManager.instance.Setup(sourceConfig);

            lastAudioLoader.SplitMultiplier = MasterConfig.ThreadCount;
            lastAudioLoader.OnLoadingAborted += () => statusMessage = "Loading aborted.";
            lastAudioLoader.OnLoadingDone += () => statusMessage = "Loaded in " + loadTimer.Elapsed.TotalSeconds + " seconds";
            lastAudioLoader.OnLoadingDone += () => sourceConfig.UnityMainThreadRunner.Enqueue(() => MasterConfig.audioSource.clip = lastAudioLoader.AudioClip);
            //Remove
            //lastAudioLoader.OnLoadingDone += () => PCQ.EnqueueTask(() => AudioProcessingManager.instance.BeginProcessing(lastAudioLoader.AudioClip), PCQ.cancelSource.Token);
            lastAudioLoader.OnLoadingFailed += (exception) => statusMessage = "Loading has failed: " + exception.Message;
            lastAudioLoader.StartLoading();
        }
    }

    public virtual bool IsFilePresent(string pathToCheck)
    {
        string path = MasterConfig.RootFolder + '\\' + pathToCheck;
        Debug.Log(path);
        if (FileManagementUtility.CheckExists(path))
        {
            Debug.Log("Exists");
            MasterConfig.dataFilePresent = true;
            MasterConfig.songData = LoadSongData(pathToCheck);
            return true;
        }
        else
        {
            Debug.Log("Doesn't Exist");
            AudioProcessingManager.instance.BeginProcessing(lastAudioLoader.AudioClip);
            return false;
        }
    }

    public virtual SongData LoadSongData(string songPath)
    {
        LoadStatus = 2;
        if (FileManagementUtility.CheckExists(filePath + "\\" + songPath))
        {
            Debug.Log("Loading");
            DataFile sb = LoadData(songPath, "SubBass.json");
            LoadStatus = 15;
            DataFile ba = LoadData(songPath, "Bass.json");
            LoadStatus = 25;
            DataFile lm = LoadData(songPath, "LowMidrange.json");
            LoadStatus = 35;
            DataFile m = LoadData(songPath, "Midrange.json");
            LoadStatus = 45;
            DataFile um = LoadData(songPath, "UpperMidrange.json");
            LoadStatus = 55;
            DataFile p = LoadData(songPath, "Presence.json");
            LoadStatus = 65;
            DataFile br = LoadData(songPath, "Brilliance.json");
            LoadStatus = 75;
            DataFile bb = LoadData(songPath, "Brilliance.json");
            LoadStatus = 85;

            SongData data = new SongData
            {
                SubBass = sb,

                Bass = ba,

                LowMidrange = lm,

                Midrange = m,

                UpperMidrange = um,

                Presence = p,

                Brilliance = br,

                BeyondBrilliance = bb

            };
            LoadStatus = 100;

            return data;
        }
        return new SongData();
    }

    public virtual DataFile LoadData(string songPath, string dataPath)
    {
        string fullPath = Application.dataPath + "\\StreamingAssets" + "\\" + songPath + "\\" + dataPath;
        string jsonString;
        DataFile data = new DataFile();
        if (File.Exists(fullPath))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                //Debug.Log("Android");
                jsonString = File.ReadAllText(fullPath);
                data = JsonUtility.FromJson<DataFile>(jsonString);
            }
            else
            {
                //Debug.Log("Other Platform");
                jsonString = File.ReadAllText(fullPath);
                data = JsonUtility.FromJson<DataFile>(jsonString);
            }
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
        return data;
    }
}
