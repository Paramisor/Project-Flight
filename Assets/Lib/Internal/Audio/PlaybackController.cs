using UnityEngine;


/// <summary>
/// Controls playback of audio
/// Handles loading and saving of data
/// </summary>
public class PlaybackController : MonoBehaviour
{
    public bool createDataFile = false;
    public bool preProcessSamples = false;
    public bool backgroundThreadCompleted = false;

    void Awake()
    {
        MasterConfig.audioSource = this.transform.GetComponent<AudioSource>();
    }

    public bool SetPlayingState(bool newState)
    {
        if (newState)
        {
            MasterConfig.audioSource.Play();
            return true;
        }
        else
        {
            MasterConfig.audioSource.Stop();
            return false;
        }
    }
}