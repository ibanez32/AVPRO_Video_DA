using System;
using System.Text.RegularExpressions;
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
            _mediaPlayer.AddStoppedEvent(OnStoped);
            _mediaPlayer.AddEndReachedEvent(PlayerEndReached);
        }
    }

  private  void PlayerEndReached()
    {
        Debug.Log("PlayerEndReached");

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
            try
            {
                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                
                Debug.Log("ERROR Play");
            }
          
            
           OnPlayerPlaying();
        }
        else
        {
            _mediaPlayer.Path = "file:///" + filePath;
            try
            {
                _mediaPlayer.Play();
            }
            catch (Exception)
            {

                Debug.Log("ERROR Play");
            }
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
    public void OnStoped()
    {
        Debug.Log("STOPED=" + _mediaPlayer.Position);
        
        
    }
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
        if (Offs>0)
        {
            _mediaPlayer.Position = Offs;
        }
       

        StartCoroutine(StartLoad());
    }

    IEnumerator StartLoad()
    {
        while (!_mediaPlayer.IsPlaying)
        {
            yield return null;
        }
        for (int j = 0; j < DataSchedule.Instance.GetDataschedules().Count; j++)
        {
            if (string.IsNullOrEmpty(DataSchedule.Instance.GetDataschedules()[j].PathLoad))
            {
                string id = DataSchedule.Instance.GetDataschedules()[j].id;
                string path = null;
                // Debug.Log("PathLoad=" + ItemClip["path"].ToString());
                WWW www_PlayList = new WWW(DataSchedule.Instance.GetDataschedules()[j].PathLoadGlobal);

                while (false == www_PlayList.isDone)
                {


                    yield return null;

                }
                if (www_PlayList.error == null)
                {
                    // Debug.Log(Screen.width + " x " + Screen.height);
                    Regex regex = new Regex(@"\bRESOLUTION=\b([0-9]+)[x]([0-9]+).*?\n([https://].*?)\/(\w+.m3u8)");
                    Match match = regex.Match(www_PlayList.text);

                    int count = 0;
                    while (match.Success)
                    {
                        //   Debug.Log("List=" + match.Groups[1].Value + " X " + match.Groups[2].Value);
                        if (count == 0)
                        {
                            path = match.Groups[3].Value + "/" + match.Groups[4].Value;
                        }

                        if (Screen.width >= Int32.Parse(match.Groups[1].Value) && Screen.height >= Int32.Parse(match.Groups[2].Value))
                        {
                            path = match.Groups[3].Value + "/" + match.Groups[4].Value;
                            //       Debug.Log("Yes");
                        }
                        else
                        {
                            //       Debug.Log("NO");
                        }
                        count++;
                        // Переходим к следующему совпадению
                        match = match.NextMatch();
                    }
                    // Debug.Log(path);
                    foreach (var VARIABLE in DataSchedule.Instance.GetDataschedules())
                    {
                        if (VARIABLE.id == id)
                        {
                            VARIABLE.PathLoad = path;
                        }
                    }
                }

                else
                {

                }
            }

        }
      //  DataSchedule.Instance.PrintDataSchedule();
        yield return null;
       // if (StateController.Instance.GetIsFirstDowloadClip())
       // {
       //     if (!StateController.Instance.GetIsDowloadMovie())
       //     {
       //         StateController.Instance.SetstopDowloadMovie(false);
       //         StateController.Instance.StartDeleteClip();
       //
       //     }
       //
       //     StateController.Instance.SetIsFirstDowload(false);
       //
       // }
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
