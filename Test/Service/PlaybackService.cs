using NAudio.Wave;
using System;
using System.Diagnostics;
using Test.Schema;
using Test.Utils;


namespace Test.Service
{
    public enum PlaybackState
    {
        Init,
        Prepared,
        Playing,
        Stopped,
        Paused,
    }

    public enum VolumeState
    {
        Muted,
        Unmuted,
    }

    public class PlaybackService : IPlayable
    {
        private WaveOutEvent? _outputDevice;
        private SpeedSampleProvider? _speedSampleProvider;
        private AudioFileReader? _audioFileReader;

        public bool IsReady => (_speedSampleProvider != null && _audioFileReader != null);

        public int ForwardPlaybackRewindDelta { get; private set; } = 9;
        public int BackwardPlaybackRewindDelta { get; private set; } = -9;

        public float PlaybackSpeed
        {
            get => _speedSampleProvider?.Speed ?? 1f;
            set
            {
                if (_speedSampleProvider != null)
                    _speedSampleProvider.Speed = value;
            }
        }

        public PlaybackState PlaybackState
        {
            get => _playbackState;
            private set => _playbackState = value;
        }

        public double CurrentPlaybackTime => _audioFileReader != null ? _audioFileReader.CurrentTime.TotalSeconds / PlaybackSpeed : 0;
        public double TotalPlaybackDuration => _audioFileReader != null ? _audioFileReader.TotalTime.TotalSeconds / PlaybackSpeed : 0;

        private PlaybackState _playbackState = PlaybackState.Init;
        private bool CanStartPlaypack => _audioFileReader != null && PlaybackState != PlaybackState.Playing;
        private bool CanStopPlaypack => _audioFileReader != null && PlaybackState is not (PlaybackState.Stopped or PlaybackState.Init);
        private bool CanPausePlayback => _audioFileReader != null && PlaybackState == PlaybackState.Playing;

        private float _previousVolume { get; set; }
        public VolumeState VolumeState = VolumeState.Unmuted;

        public void Init(string filePath)
        {
            _outputDevice = new WaveOutEvent();
            _audioFileReader = new AudioFileReader(filePath);
            _speedSampleProvider = new SpeedSampleProvider(_audioFileReader.ToSampleProvider());
            _speedSampleProvider.Speed = PlaybackSpeed;
            _outputDevice.Init(_speedSampleProvider);
        }

        public void ResetAndPlay(string filePath)
        {   
            DisposePlayback();

            Init(filePath);
            Play();
        }

        private void DisposePlayback()
        {
            _outputDevice?.Stop();

            _outputDevice?.Dispose();
            _outputDevice = null;

            _audioFileReader?.Dispose();
            _audioFileReader = null;

            _speedSampleProvider = null;
            PlaybackState = PlaybackState.Stopped;
        }

        public void Play()
        {
            if (CanStartPlaypack)
            {
                _outputDevice?.Play();
                PlaybackState = PlaybackState.Playing;
            }
        }

        public void Pause()
        {
            if (CanPausePlayback)
            {
                _outputDevice?.Pause();
                PlaybackState = PlaybackState.Paused;
            }
        }

        public void Rewind(double delta)
        {
            if (_audioFileReader == null)
            {
                return;
            }

            bool wasPlaying = PlaybackState == PlaybackState.Playing;

            if (wasPlaying) 
            {
                _outputDevice?.Stop(); 
            }

            double newPlaybackSeconds = CurrentPlaybackTime + delta;

            newPlaybackSeconds = Math.Clamp(newPlaybackSeconds, 0, TotalPlaybackDuration);

            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(newPlaybackSeconds * PlaybackSpeed);

            if (wasPlaying)
            { 
                _outputDevice?.Play(); 
            }
        }

        public void Stop()
        {
            if (_audioFileReader == null)
            {
                return;
            }

            if (CanStopPlaypack)
            {
                _outputDevice?.Stop();
                _audioFileReader.CurrentTime = TimeSpan.Zero;
                PlaybackState = PlaybackState.Stopped;
            }
        }

        public void Seek(double seconds)
        {
            if (_audioFileReader == null)
            {
                return;
            }

            double sourceSeconds = seconds * PlaybackSpeed;
            sourceSeconds = Math.Clamp(sourceSeconds, 0, _audioFileReader.TotalTime.TotalSeconds);
            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(sourceSeconds);
        }

        public void Mute()
        {
            SetVolume(0);
            VolumeState = VolumeState.Muted;
        }

        public void Unmute()
        {
            if (_outputDevice == null)
            {
                return;
            }

            VolumeState = VolumeState.Unmuted;
            _outputDevice.Volume = _previousVolume;
        }

        public void SetVolume(float value)
        {
            if (_outputDevice == null || VolumeState == VolumeState.Muted)
            {
                _previousVolume = value;
                return;
            }

            _previousVolume = _outputDevice.Volume;
            _outputDevice.Volume = value;
        }

        public float GetVolume()
        {
            return _outputDevice == null ? 1f : _outputDevice.Volume;
        }
    }
}
