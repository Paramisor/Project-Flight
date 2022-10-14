using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FileExportHandler : MonoBehaviour
{
    public bool dataFilePresent = false;
    public IEnumerator SaveThread;

    /*-------------------------------------------------------------------*/

    public void CreateData(SongData songData)
    {
        SaveThread = StoreData(songData);
        StartCoroutine(SaveThread);
        Debug.Log("CreatedData");
    }

    public bool BeginSaveTread(SongData songData, int stage)
    {
        SaveThread = StoreData(MasterConfig.songData, stage);
        StartCoroutine(SaveThread);
        return true;
    }

    IEnumerator StoreData(SongData data)
    {
        string filePath = Application.dataPath + "\\StreamingAssets";
        FileManagementUtility.CreateGameFolder(filePath + "\\" + data.songName);

        string SubBass = JsonUtility.ToJson(data.SubBass, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "SubBass.json", SubBass);

        string Bass = JsonUtility.ToJson(data.Bass, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "Bass.json", Bass);

        string LowMidrange = JsonUtility.ToJson(data.LowMidrange, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "LowMidrange.json", LowMidrange);

        string Midrange = JsonUtility.ToJson(data.Midrange, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "Midrange.json", Midrange);

        string UpperMidrange = JsonUtility.ToJson(data.UpperMidrange, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "UpperMidrange.json", UpperMidrange);

        string Presence = JsonUtility.ToJson(data.Presence, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "Presence.json", Presence);

        string Brilliance = JsonUtility.ToJson(data.Brilliance, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "Brilliance.json", Brilliance);

        string BeyondBrilliance = JsonUtility.ToJson(data.BeyondBrilliance, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + "BeyondBrilliance.json", Brilliance);



        Debug.Log("Data Saved");

        dataFilePresent = true;
        yield return data;

        StopCoroutine(SaveThread);
    }

    IEnumerator StoreData(SongData data, bool useWebRequest)
    {
        string filePath = Application.dataPath + "\\StreamingAssets";

        string SubBass = JsonUtility.ToJson(data.SubBass, true);
        UnityWebRequest wwwSubBass = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "SubBass.json", SubBass);

        string Bass = JsonUtility.ToJson(data.Bass, true);
        UnityWebRequest wwwBass = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "Bass.json", Bass);

        string LowMidrange = JsonUtility.ToJson(data.LowMidrange, true);
        UnityWebRequest wwwLowMidrange = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "LowMidrange.json", LowMidrange);

        string Midrange = JsonUtility.ToJson(data.Midrange, true);
        UnityWebRequest wwwMidrange = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "Midrange.json", Midrange);

        string UpperMidrange = JsonUtility.ToJson(data.UpperMidrange, true);
        UnityWebRequest wwwUpperMidrange = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "UpperMidrange.json", UpperMidrange);

        string Presence = JsonUtility.ToJson(data.Presence, true);
        UnityWebRequest wwwPresence = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "Presence.json", Presence);

        string Brilliance = JsonUtility.ToJson(data.Brilliance, true);
        UnityWebRequest wwwBrilliance = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "Brilliance.json", Brilliance);

        string BeyondBrilliance = JsonUtility.ToJson(data.BeyondBrilliance, true);
        UnityWebRequest wwwBeyondBrilliance = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + "BeyondBrilliance.json", BeyondBrilliance);

        while (!wwwBrilliance.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Data Saved");

        dataFilePresent = true;
        yield return data;

        StopCoroutine(SaveThread);
    }


    IEnumerator StoreData(SongData data, int segmentId)
    {
        string filePath = Application.dataPath + "\\StreamingAssets";
        FileManagementUtility.CreateGameFolder(filePath + "\\" + data.songName + "\\" + segmentId);

        string SubBass = JsonUtility.ToJson(data.SubBass, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "SubBass.json", SubBass);

        string Bass = JsonUtility.ToJson(data.Bass, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "Bass.json", Bass);

        string LowMidrange = JsonUtility.ToJson(data.LowMidrange, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "LowMidrange.json", LowMidrange);

        string Midrange = JsonUtility.ToJson(data.Midrange, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "Midrange.json", Midrange);

        string UpperMidrange = JsonUtility.ToJson(data.UpperMidrange, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "UpperMidrange.json", UpperMidrange);

        string Presence = JsonUtility.ToJson(data.Presence, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "Presence.json", Presence);

        string Brilliance = JsonUtility.ToJson(data.Brilliance, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "Brilliance.json", Brilliance);

        string BeyondBrilliance = JsonUtility.ToJson(data.BeyondBrilliance, true);
        File.WriteAllText(filePath + "\\" + data.songName + "\\" + segmentId + "\\" + "Brilliance.json", BeyondBrilliance);

        Debug.Log("Data Saved");

        dataFilePresent = true;
        yield return data;

        StopCoroutine(SaveThread);
    }

    IEnumerator StoreData(SongData data, int segmentId, bool useWebRequest)
    {
        if (useWebRequest)
        {
            string filePath = Application.dataPath + "\\StreamingAssets";
            FileManagementUtility.CreateGameFolder(filePath + "\\" + data.songName + "\\" + segmentId);

            string SubBass = JsonUtility.ToJson(data.SubBass, true);
            UnityWebRequest wwwSubBass = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + segmentId + "\\" + "SubBass.json", SubBass);

            string Bass = JsonUtility.ToJson(data.Bass, true);
            UnityWebRequest wwwBass = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + segmentId + "\\" + "Bass.json", Bass);

            string LowMidrange = JsonUtility.ToJson(data.LowMidrange, true);
            UnityWebRequest wwwLowMidrange = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + segmentId + "\\" + "LowMidrange.json", LowMidrange);

            string Midrange = JsonUtility.ToJson(data.Midrange, true);
            UnityWebRequest wwwMidrange = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + segmentId + "\\" + "Midrange.json", Midrange);

            string UpperMidrange = JsonUtility.ToJson(data.UpperMidrange, true);
            UnityWebRequest wwwUpperMidrange = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + segmentId + "\\" + "\\" + "UpperMidrange.json", UpperMidrange);

            string Presence = JsonUtility.ToJson(data.Presence, true);
            UnityWebRequest wwwPresence = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + segmentId + "\\" + "Presence.json", Presence);

            string Brilliance = JsonUtility.ToJson(data.Brilliance, true);
            UnityWebRequest wwwBrilliance = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + segmentId + "\\" + "Brilliance.json", Brilliance);

            string BeyondBrilliance = JsonUtility.ToJson(data.BeyondBrilliance, true);
            UnityWebRequest wwwBeyondBrilliance = UnityWebRequest.Put(filePath + "\\" + MasterConfig.audioSource.clip.name + "\\" + segmentId + "\\" + "BeyondBrilliance.json", BeyondBrilliance);

            while (!wwwBrilliance.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            StoreData(data, segmentId);
        }

        Debug.Log("Data Saved");

        dataFilePresent = true;
        yield return data;

        StopCoroutine(SaveThread);
    }
}
