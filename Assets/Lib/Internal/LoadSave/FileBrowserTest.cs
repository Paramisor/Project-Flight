using RuntimeAudioClipLoader;
using RuntimeAudioClipLoaderDemo;
using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowserTest : MonoBehaviour
{
    public PermissionReq req;

    public int flag = 0;

    float loadTime;
    float analysisTime;

    public Button BrowseBtn;
    public TMP_Dropdown LibraryDdl;
    public Button PlayBtn;
    public TMP_Text StatusTxt;
    public TMP_Text PathTxt;
    public RectTransform LoadBar;
    public float width;
    public string[] presetFiles;

    public IEnumerable<string> libraryFiles;
    bool directoryGetFilesThrewException = false;

    static public string rootFolder { get { return Application.streamingAssetsPath; } }
    static public string sourceFolder { get { return Path.Combine(rootFolder, "Songs"); } }

    public string statusMessage;
    public EnumSelectionGrid<LoadMethod> loadMethod = new EnumSelectionGrid<LoadMethod>();
    public EnumSelectionGrid<PreferredDecoder> preferredDecoder = new EnumSelectionGrid<PreferredDecoder>();
    public AudioLoader lastAudioLoader;
    Coroutine lastCoroutine;
    System.Diagnostics.Stopwatch loadTimer;
    AudioClip song;
    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    void Start()
    {
        AndroidRuntimePermissions.Permission result = AndroidRuntimePermissions.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE");
        if (result == AndroidRuntimePermissions.Permission.Granted)
            UnityEngine.Debug.Log("We have permission to access external storage!");
        else
            UnityEngine.Debug.Log("Permission state: " + result);

        if (BrowseBtn == null)
            BrowseBtn = this.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        if (LibraryDdl == null)
            LibraryDdl = this.transform.GetChild(1).GetComponent<TMP_Dropdown>();
        if (PlayBtn == null)
            PlayBtn = this.transform.GetChild(2).GetComponent<Button>();
        if (StatusTxt == null)
            StatusTxt = this.transform.GetChild(3).GetComponent<TMP_Text>();
        if (PathTxt == null)
            PathTxt = this.transform.GetChild(4).GetComponent<TMP_Text>();

        LoadBar = this.transform.GetChild(5).Find("Sliding Area/Handle").GetComponent<RectTransform>();
        width = LoadBar.transform.parent.GetComponent<RectTransform>().sizeDelta.x;

        BrowseBtn.onClick.AddListener(BeginBrowse);
        LibraryDdl.onValueChanged.AddListener(LoadSong);
        PlayBtn.onClick.AddListener(PlaySong);

        UpdateDropdown();
    }

    private void Update()
    {
        if (lastAudioLoader != null)
        {
            if (!string.IsNullOrEmpty(statusMessage))
            {
                if (flag != 0)
                {
                    StatusTxt.text = "Processing of Song Completed in " + (analysisTime - loadTime);
                    LoadBar.parent.parent.gameObject.SetActive(false);
                }
                else if (lastAudioLoader.IsLoadingDone)
                {
                    StatusTxt.text = statusMessage + "\n" + "Analysis " + ((AudioProcessingManager.instance.MidrangeSpectralFluxAnalyzer.AnalyzedData.samples.Count / (lastAudioLoader.AudioClip.samples / 2048f)) * 100).ToString("F1") + "% Complete";
                    LoadBar.SetSizeWithCurrentAnchors(0, (AudioProcessingManager.instance.MidrangeSpectralFluxAnalyzer.AnalyzedData.samples.Count / (lastAudioLoader.AudioClip.samples / 2048f)) * width);
                    analysisTime = Convert.ToSingle(loadTimer.Elapsed.TotalSeconds);
                }
                else
                {
                    LoadBar.SetSizeWithCurrentAnchors(0, lastAudioLoader.LoadProgress * width);
                    StatusTxt.text = statusMessage;
                }
            }
        }

        if (AudioProcessingManager.instance.MidrangeSpectralFluxAnalyzer.AnalyzedData.samples.Count > 0 && AudioProcessingManager.instance.MidrangeSpectralFluxAnalyzer.AnalyzedData.samples.Count > (lastAudioLoader.AudioClip.samples / 2048) - 10 && flag == 0)
        {
            flag++;
            StartCoroutine(EnablePlay());
        }

    }

    IEnumerator EnablePlay()
    {
        yield return new WaitForEndOfFrame();
        PlayBtn.interactable = true;
        StopCoroutine(EnablePlay());
    }

    void UpdateLoadBar()
    {
        statusMessage = ("Load progress " + lastAudioLoader.LoadProgress * 100);
    }

    void UpdateDropdown()
    {
        //string path = "jar:file://" + Application.dataPath + "!/assets/";

        var files = GetStreamingAssetsFiles();
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            //LibraryDdl.options.Add(new TMP_Dropdown.OptionData(file));
            int position = file.LastIndexOf('\\');
            LibraryDdl.options.Add(new TMP_Dropdown.OptionData(file.Substring(position + 1)));
        }
    }

    void BeginBrowse()
    {
        BrowseBtn.interactable = false;
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    void LoadSong(int i)
    {
        LibraryDdl.captionText.text = LibraryDdl.options[i].text;
        if (i != 0)
        {
            var path = Path.Combine(sourceFolder + '\\' + LibraryDdl.options[i].text);
            if (!path.StartsWith("jar:") && !path.StartsWith("http:")) path = "file://" + path;

            //PathTxt.text = path;
            StartLoading(path);
        }
        else
        {
            PlayBtn.interactable = false;
        }
    }

    void PlaySong()
    {
        //Use PlaybackController instead
        //MasterConfig.audioSource.Play();
        //this.gameObject.SetActive(false);
        statusMessage = "";
        //this.transform.parent.GetChild(0).gameObject.SetActive(true);
    }

    public string[] GetStreamingAssetsFiles()
    {
        // On some platforms we are unable to list all files with Directory.GetFiles due to security or architecture restrictions.
        // For those platforms we need to build the files list beforehand in editor.
        // Iam not sure where it's possible so we are trying it everywhere, if exception occurs, we stop trying.
        if (!directoryGetFilesThrewException)
        {
            try
            {
                return
                    Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories)
                    .Where(f => AudioLoaderConfig.IsSupportedFormat(f))
                    .Select(f => f.Substring(rootFolder.Length + 1))
                    .ToArray();
            }
            catch
            {
                directoryGetFilesThrewException = true;
            }
        }
        return presetFiles;
    }

    void StartLoading(string url)
    {
        if (lastCoroutine != null)
        {
            StopCoroutine("DownloadClipAndPlayFromUrl");
            lastCoroutine = null;
        }

        lastCoroutine = StartCoroutine(
            DownloadClipAndPlayFromUrl(url)

        );
    }

    //private void getNumCores()
    //{
    //	//Private Class to display only CPU devices in the directory listing
    //	class CpuFilter implements FileFilter
    //	{
    //	@Override
    //		public boolean accept(File pathname)
    //		{
    //			//Check if filename is "cpu", followed by one or more digits
    //			if (Pattern.matches("cpu[0-9]+", pathname.getName()))
    //			{
    //				return true;
    //			}
    //			return false;
    //		}
    //	}

    //	try 
    //	{
    //	    //Get directory containing CPU info
    //	    File dir = new File("/sys/devices/system/cpu/");
    //		//Filter to only list the devices we care about
    //		File[] files = dir.listFiles(new CpuFilter());
    //	    //Return the number of cores (virtual CPU devices)
    //	    return files.length;
    //	} 
    //	catch(Exception e) 
    //	{
    //	    //Default to return 1 core
    //	    return 1;
    //	}
    //}

    public int SplitMultiplier = 28;

    // Unity unifies file system access in the WWW class, we can use the WWW class to get data both from internet and from local file system on any platform.
    IEnumerator DownloadClipAndPlayFromUrl(string url)
    {
        if (lastAudioLoader != null) lastAudioLoader.Destroy();
        lastAudioLoader = null;
        MasterConfig.audioSource.clip = null;

        var isFromInternet = url.IndexOf("http", StringComparison.InvariantCultureIgnoreCase) != -1;

        statusMessage = "Getting data " + url;
        var www = new WWW(url);
        while (!www.isDone)
        {
            if (isFromInternet)
                statusMessage = "Downloading " + url + "\nDownload progress: " + (int)(www.progress * 100) + "%";
            yield return new WaitForEndOfFrame();
        }

        if (www.bytes.Length == 0)
        {
            statusMessage = "Error: downloaded zero bytes from " + url;
        }
        else
        {
            statusMessage = "Loading " + url;

            if (loadMethod.value == LoadMethod.StreamInUnityThread) statusMessage = "Streaming";
            else loadTimer = System.Diagnostics.Stopwatch.StartNew();


            AudioLoaderConfig config = new AudioLoaderConfig();
            config.DataStream = new MemoryStream(www.bytes);
            config.PreferredDecoder = PreferredDecoder.PreferNative; // preferredDecoder.value;
            int position = url.LastIndexOf('\\');
            config.UnityAudioClipName = url.Substring(position + 1);
            config.LoadMethod = LoadMethod.AllPartsInBackgroundThread; // loadMethod.value;

            lastAudioLoader = new AudioLoader(config);

            for (int i = 0; i < SplitMultiplier; i++)
            {
                var thisConfig = new AudioLoaderConfig();
                thisConfig.DataStream = new MemoryStream(www.bytes);
                thisConfig.PreferredDecoder = PreferredDecoder.PreferNative; // preferredDecoder.value;
                int positions = url.LastIndexOf('\\');
                thisConfig.UnityAudioClipName = url.Substring(position + 1);
                thisConfig.LoadMethod = LoadMethod.AllPartsInBackgroundThread; // loadMethod.value;
                lastAudioLoader.MTConfigs.Add(thisConfig);
            }

            lastAudioLoader.SplitMultiplier = SplitMultiplier;
            lastAudioLoader.OnLoadingAborted += () => statusMessage = "Loading aborted.";
            lastAudioLoader.OnLoadingDone += () => statusMessage = "Loaded in " + loadTimer.Elapsed.TotalSeconds + " seconds";
            lastAudioLoader.OnLoadingDone += () => loadTime = Convert.ToSingle(loadTimer.Elapsed.TotalSeconds);
            lastAudioLoader.OnLoadingFailed += (exception) => statusMessage = "Loading has failed: " + exception.Message;
            lastAudioLoader.OnLoadProgressChanged += UpdateLoadBar;
            MasterConfig.audioSource.clip = lastAudioLoader.AudioClip;
            lastAudioLoader.StartLoading();
            //SongController.instance.audioSource.Play();

            //Create new processing manager.
            //StartCoroutine(ProcessAudioData());
        }
    }

    IEnumerator ProcessAudioData()
    {
        int Count = 0;
        int sampleSize = Mathf.CeilToInt(lastAudioLoader.AudioClip.samples / 10);
        while ((lastAudioLoader.LoadProgress * 100) <= 20)
        {
            yield return null;
        }
        while (Count < 10)
        {
            if ((lastAudioLoader.LoadProgress * 100) >= 10 && Count == 0)
            {
                //0 - 930931
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, 0);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                //StartCoroutine(ProcessSegment(sampData, 0));
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 20 && Count == 1)
            {
                //930931 - 1861862
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 30 && Count == 2)
            {
                //1861862 - 2,792,793
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 40 && Count == 3)
            {
                //2,792,793 - 3,723,724
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 50 && Count == 4)
            {
                //3,723,724 - 4,654,655
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 60 && Count == 5)
            {
                //4,654,655 - 5,585,586
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 70 && Count == 6)
            {
                //5,585,586 - 6,516,517
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 80 && Count == 7)
            {
                //6,516,517 - 7,447,448
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 90 && Count == 8)
            {
                //7,447,448 - 8,378,379
                float[] sampData = new float[sampleSize];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
            else if ((lastAudioLoader.LoadProgress * 100) >= 100 && Count == 9)
            {
                //8,378,379 - 9309312
                float[] sampData = new float[lastAudioLoader.AudioClip.samples - (sampleSize * Count)];
                lastAudioLoader.AudioClip.GetData(sampData, sampleSize * Count);
                GameObject holder = new GameObject();
                //AudioProcessingManager APM = holder.AddComponent<AudioProcessingManager>();
                //APM.Setup(lastAudioLoader.AudioClip, sampData, Count, lastAudioLoader.Config);
                Count++;
                yield return null;
            }
        }
        //PlayBtn.interactable = true;
        StopCoroutine(ProcessAudioData());
        yield return new WaitForEndOfFrame();
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
            //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result);
            //FileBrowserHelpers.WriteCopyToFile(Application.dataPath + "\\StreamingAssets\\Test.mp3", FileBrowser.Result);

            //ReadAllTheWayThroughMp3File(FileBrowser.Result);
            LibraryDdl.captionText.text = FileBrowser.Result;
            LibraryDdl.options.Add(new TMP_Dropdown.OptionData(FileBrowser.Result));

            //float[] f;
            //
            //float[] floatArr = new float[bytes.Length / 4];
            //for (int i = 0; i < floatArr.Length; i++) 
            //{
            //    if (BitConverter.IsLittleEndian) 
            //        Array.Reverse(bytes, i * 4, 4);
            //    floatArr[i] = BitConverter.ToSingle(bytes, i * 4);
            //	Debug.Log(bytes[i] + " : " + floatArr[i]);
            //}
            //
            //f = floatArr;
            //
            //
            //song = AudioClip.Create(FileBrowser.Result, f.Length, 2, 44100, false);
            //song.SetData(f, 0);
            //
            //Debug.Log(song.samples); 
            //Debug.Log(song.frequency);

            //SongController.instance.audioSource.clip = song;
            //SongController.instance.Play();
        }
        else
        {
            UnityEngine.Debug.LogError("No Result Found..");
        }
    }
}