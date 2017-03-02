using System;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class VLC_Controller : MonoBehaviour
{
    public MediaPlayer _mediaPlayer;

    public Slider _videoSeekSlider;
    private float _setVideoSeekSliderValue;
    private bool _wasPlayingOnScrub;

    public Slider _audioVolumeSlider;
    private float _setAudioVolumeSliderValue;

    public Toggle _AutoStartToggle;
    public Toggle _MuteToggle;

    public string[] _videoFiles;
    private bool seekIs = false;

    private int _VideoIndex = 0;

    public void OnOpenVideoFile(string path)
    {
        _mediaPlayer.m_VideoPath = path;

        if (string.IsNullOrEmpty(_mediaPlayer.m_VideoPath))
        {
            _mediaPlayer.CloseVideo();

        }
        else
        {
            _videoSeekSlider.value = 0;
            _mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, _mediaPlayer.m_VideoPath, _AutoStartToggle.isOn);

        }
    }


    public void OnAutoStartChange()
    {
        if (_mediaPlayer &&
            _AutoStartToggle && _AutoStartToggle.enabled &&
            _mediaPlayer.m_AutoStart != _AutoStartToggle.isOn)
        {
            _mediaPlayer.m_AutoStart = _AutoStartToggle.isOn;
        }
    }

    public void OnMuteChange()
    {
        if (_mediaPlayer)
        {
            _mediaPlayer.Control.MuteAudio(_MuteToggle.isOn);
        }
    }

    public void OnPlayButton()
    {
        if (_mediaPlayer)
        {
            _mediaPlayer.Control.Play();

        }
    }
    public void OnPauseButton()
    {
        if (_mediaPlayer)
        {
            _mediaPlayer.Control.Pause();

        }
    }

    public void OnVideoSeekSlider()
    {
        if (_mediaPlayer && _videoSeekSlider && _videoSeekSlider.value != _setVideoSeekSliderValue)
        {
            _mediaPlayer.Control.SeekFast(_videoSeekSlider.value * _mediaPlayer.Info.GetDurationMs());
        }
    }
    public void OnVideoSliderDown()
    {
        if (_mediaPlayer)
        {
            _wasPlayingOnScrub = _mediaPlayer.Control.IsPlaying();
            if (_wasPlayingOnScrub)
            {
                _mediaPlayer.Control.Pause();

            }
            OnVideoSeekSlider();
        }
    }
    public void OnVideoSliderUp()
    {
        if (_mediaPlayer && _wasPlayingOnScrub)
        {
            _mediaPlayer.Control.Play();
            _wasPlayingOnScrub = false;


        }
    }

    public void OnAudioVolumeSlider()
    {
        if (_mediaPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
        {
            _mediaPlayer.Control.SetVolume(_audioVolumeSlider.value);
        }
    }


    public void OnRewindButton()
    {
        if (_mediaPlayer)
        {
            _mediaPlayer.Control.Rewind();
        }
    }

    void Start()
    {
        if (_mediaPlayer)
        {
            _mediaPlayer.Events.AddListener(OnVideoEvent);
        }
    }

    void Update()
    {
        if (_mediaPlayer && _mediaPlayer.Info != null && _mediaPlayer.Info.GetDurationMs() > 0f)
        {
            if (seekIs)
            {

                DateTime localDate = DateTime.Now;
                int mSec = (Int32.Parse(localDate.ToString("HH")) * 3600 + Int32.Parse(localDate.ToString("mm")) * 60 + Int32.Parse(localDate.ToString("ss"))) * 1000;
                var Item = DataSchedule.Instance.GetDataschedules()
                .Find(
                    elm =>
                        Int32.Parse(elm.TimeStart) <= mSec &&
                        (Int32.Parse(elm.TimeStart) + Int32.Parse(elm.duration) * 1000) > mSec);

                int seek_of = mSec - Int32.Parse(Item.TimeStart);
                _videoSeekSlider.value = (float)seek_of / _mediaPlayer.Info.GetDurationMs();
                seekIs = false;
            }
            float time = _mediaPlayer.Control.GetCurrentTimeMs();
            float d = time / _mediaPlayer.Info.GetDurationMs();
            _setVideoSeekSliderValue = d;
            _videoSeekSlider.value = d;
        }
    }

    // Callback function to handle events
    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                break;
            case MediaPlayerEvent.EventType.Started:
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                seekIs = true;
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                break;
        }

        Debug.Log("Event: " + et.ToString());
    }


}