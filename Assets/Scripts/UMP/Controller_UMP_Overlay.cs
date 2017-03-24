using System;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;


public class Controller_UMP_Overlay : MonoBehaviour
{
    public UniversalMediaPlayer _mediaPlayer;
    private float Offset;
    private bool isEnd;
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
            _mediaPlayer.AddEndReachedEvent(EndReached);
        }
    }

    private void EndReached()
    {
        Debug.Log("EndReached");
        isEnd = true;
        // State_Controller_UMP_Overlay.Instance.NextMovie();
    }
    private void OnPlayerBuffering(float arg0)
    {
       
    }


    // Update is called once per frame
    void Update()
    {

    }
    public void ReplacePlayVideo(string path, float offset, bool url)
    {
        Debug.Log("Rep");
        Offset = offset;
        LoadVideo(path, url);


    }


    public void LoadVideo(string filePath, bool url = false)
    {
        if (url)
        {
            _mediaPlayer.Path = filePath;
            try
            {
                isEnd = false;
                _mediaPlayer.Play();
               
            }
            catch (Exception ex)
            {

                Debug.Log("ERROR Play");
            }
            if (Offset!=0)
            {
                Debug.Log("Offset");
                _mediaPlayer.Position = Offset;
                try
                {

                    _mediaPlayer.Play();

                }
                catch (Exception ex)
                {

                    Debug.Log("ERROR Play");
                }
            }
            
            State_Controller_UMP_Overlay.Instance.SetOverlay(false);
           
            
        }
        else
        {
            _mediaPlayer.Path = "file:///" + filePath;
            try
            {
                isEnd = false;
                _mediaPlayer.Play();

            }
            catch (Exception ex)
            {

                Debug.Log("ERROR Play");
            }
            if (Offset!=0)
            {
                Debug.Log("Offset");
                _mediaPlayer.Position = Offset;
                try
                {

                    _mediaPlayer.Play();

                }
                catch (Exception ex)
                {

                    Debug.Log("ERROR Play");
                }
            }
           
            State_Controller_UMP_Overlay.Instance.SetOverlay(false);
        }
    }
    

    
    public void OnStoped()
    {
        Debug.Log("STOPED=" + _mediaPlayer.Position);
        if (isEnd)
        {
            State_Controller_UMP_Overlay.Instance.NextMovie();
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
