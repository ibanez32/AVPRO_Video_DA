using System;
using UnityEngine;
using System.Collections;

public class UMP_Controller : MonoBehaviour
{
    public UniversalMediaPlayer _mediaPlayer;
    private float Offset;
	// Use this for initialization
    void Start()
    {
        if (_mediaPlayer != null)
        {
           // _mediaPlayer.AddPlayingEvent(OnPlayerPlaying);
          //  _mediaPlayer.AddTimeChangedEvent(OnPlayerTimeChanged);
          //  _mediaPlayer.AddPositionChangedEvent(OnPlayerPositionChanged);
          // 
          //  _mediaPlayer.AddBufferingEvent(OnPlayerBuffering);
        }
    }

    private void OnPlayerBuffering(float arg0)
    {
        Debug.Log("OnPlayerBuffering="+arg0);
        if (arg0==100f)
        {
            OnPlayerPlaying();
        }
    }

    // Update is called once per frame
	void Update () {
	
	}
    public void ReplacePlayVideo(string path, int offset, bool url)
    {
        Debug.Log("Rep");

        LoadVideo(path, url);
      

    }

    
    public void LoadVideo(string filePath, bool url = false)
    {
        if (url)
        {
            _mediaPlayer.Path = filePath;
           _mediaPlayer.Play();
            
           OnPlayerPlaying();
        }
        else
        {
            _mediaPlayer.Path = "file:///" + filePath;
           _mediaPlayer.Play();
           OnPlayerPlaying();
        }
    }
    public void Play()
    {
        _mediaPlayer.Play();
    }

    public void OnPlayerOpening()
    {
        Debug.Log("OnPlayerOpening");
    }

  // public void OnPlayerBuffering()
  // {
  //     Debug.Log("OnPlayerBuffering");
  // }
  //
    public void OnPlayerPlaying()
    {
        DateTime localDate = DateTime.Now;
        int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
        var Item = DataSchedule.Instance.GetDataschedules()
        .Find(
            elm =>
                Int32.Parse(elm.TimeStart) <= mSec &&
                (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);
        Debug.Log("mSec=" + mSec);
        Debug.Log("TimeStart=" + Int32.Parse(Item.TimeStart));
        Debug.Log("Duration=" + Int32.Parse(Item.duration) * 1000);
        float delta = (float)(mSec - Int32.Parse(Item.TimeStart));
        float duration = (float)(Int32.Parse(Item.duration) * 1000);
        Debug.Log("delta=" + delta);
        Debug.Log("Duration=" + duration);
        float Offs = delta / duration;
        Debug.Log("OnPlayerPlaying=" + Offs);
        _mediaPlayer.Position = Offs;
     
        if (StateController.Instance.GetIsFirstDowloadClip())
        {
            if (!StateController.Instance.GetIsDowloadMovie())
            {
                StateController.Instance.SetstopDowloadMovie(false);
                StateController.Instance.StartDeleteClip();

            }

            StateController.Instance.SetIsFirstDowload(false);

        }
    }

    IEnumerator StartLoad()
    {
        while (!_mediaPlayer.IsPlaying)
        {
            yield return null;
        }
        DateTime localDate = DateTime.Now;
        int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
        var Item = DataSchedule.Instance.GetDataschedules()
        .Find(
            elm =>
                Int32.Parse(elm.TimeStart) <= mSec &&
                (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);
        Debug.Log("mSec=" + mSec);
        Debug.Log("TimeStart=" + Int32.Parse(Item.TimeStart));
        Debug.Log("Duration=" + Int32.Parse(Item.duration) * 1000);
        float delta = (float)(mSec - Int32.Parse(Item.TimeStart));
        float duration = (float)(Int32.Parse(Item.duration) * 1000);
        Debug.Log("delta=" + delta);
        Debug.Log("Duration=" + duration);
        float Offs = delta / duration;
        Debug.Log("OnPlayerPlaying=" + Offs);
        _mediaPlayer.Position = Offs;
        yield return new WaitForSeconds(30f);
        if (StateController.Instance.GetIsFirstDowloadClip())
        {
            if (!StateController.Instance.GetIsDowloadMovie())
            {
                StateController.Instance.SetstopDowloadMovie(false);
                StateController.Instance.StartDeleteClip();

            }

            StateController.Instance.SetIsFirstDowload(false);

        }
    }
    public void OnPlayerPaused()
    {
        Debug.Log("OnPlayerPaused");
    }

    public void OnPlayerStopped()
    {
        Debug.Log("OnPlayerStopped");
    }

    public void OnPlayerEndReached()
    {
        Debug.Log("OnPlayerEndReached");
    }

    public void OnPlayerEncounteredError()
    {
        Debug.Log("OnPlayerEncounteredError");
    }

    public void OnPlayerTimeChanged(long time)
    {
        Debug.Log("OnPlayerTimeChanged: " + time);
    }

    public void OnPlayerPositionChanged(float position)
    {
        Debug.Log("OnPlayerPositionChanged: " + position);
    }

    public void OnPlayerSnapshotTaken(string path)
    {
        Debug.Log("OnPlayerSnapshotTaken: " + path);
    }
}
