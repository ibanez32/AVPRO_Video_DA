using System;

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using LitJsonSrc;
using UGS;

using RenderHeads.Media.AVProVideo;
using Debug = UnityEngine.Debug;

namespace RenderHeads.Media.AVProVideo.Demos
{
    /// <summary>
    /// Simple GUI built using IMGUI to show scripting examples
    /// </summary>
    public class ControllerAVPRO_Overlay : MonoBehaviour
    {

        public MediaPlayer _mediaPlayer;

        private float _durationSeconds;

      
        private MediaPlayer.FileLocation _nextVideoLocation;
        private string _nextVideoPath;
        private int Offset_controller;
        void Start()
        {
            _mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
        }

        public void ReplacePlayVideo(string path, int offset, bool url)
        {
            LoadVideo(path, url);
            Offset_controller = offset;
            Debug.Log("SEEK_2" + offset);
            Debug.Log("SEEK_2" + Offset_controller);
        }

        // Callback function to handle events
        public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
        {
            switch (et)
            {
                case MediaPlayerEvent.EventType.Error:

                    break;
                case MediaPlayerEvent.EventType.ReadyToPlay:

                    //_mediaPlayer.Control.Play();
                   // _mediaPlayer.Control.SeekFast(Offset_controller);
                    break;
                case MediaPlayerEvent.EventType.Started:

                    break;
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    Debug.Log("SEEK=" + Offset_controller);
                    if (Offset_controller!=0)
                    {
                        _mediaPlayer.Control.SeekFast(Offset_controller);
                    }
                   
                    StateControllerAVPro.Instance.SetOverlay(false);
                    break;
                case MediaPlayerEvent.EventType.MetaDataReady:
                    break;
                case MediaPlayerEvent.EventType.Closing:
                    //StateControllerAVPro.Instance.SetOverlay(true);
                    break;
                case MediaPlayerEvent.EventType.FinishedPlaying:
                    StateControllerAVPro.Instance.NextMovie();
                    break;
            }
            AddEvent(et);
            
        }




        private void AddEvent(MediaPlayerEvent.EventType et)
        {
            Debug.Log("[SimpleController] Event: " + et.ToString());
            
        }

        void Update()
        {
           
        }

        private void LoadVideo(string filePath, bool url = false)
        {
            // Set the video file name and to load. 
            if (!url)
                _nextVideoLocation = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
            // _nextVideoLocation = MediaPlayer.FileLocation.RelativeToProjectFolder;
            else
                _nextVideoLocation = MediaPlayer.FileLocation.AbsolutePathOrURL;
            _nextVideoPath = filePath;


            // Load the video
            if (!_mediaPlayer.OpenVideoFromFile(_nextVideoLocation, _nextVideoPath, _mediaPlayer.m_AutoStart))
            {
                Debug.LogError("Failed to open video!");
            }
        }

       

        


        
    }

}
