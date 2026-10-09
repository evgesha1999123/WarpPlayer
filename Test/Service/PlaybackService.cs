using NAudio.Wave;
using System;
using System.Diagnostics;
using Test.Utils;


namespace Test.Service
{
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

        public int ForwardPlaybackRewindDelta { get; private init; } = 9;
        public int BackwardPlaybackRewindDelta { get; private init; } = -9;

        public float PlaybackSpeed
        {
            get => _speedSampleProvider?.Speed ?? 1f;
            set
            {
                if (_speedSampleProvider != null)
                    _speedSampleProvider.Speed = value;
            }
        }

        public PlaybackState PlaybackState => _outputDevice?.PlaybackState ?? PlaybackState.Stopped;

        public bool CanStartPlaypack => _audioFileReader != null && PlaybackState != PlaybackState.Playing;
        public bool CanStopPlaypack => _audioFileReader != null && PlaybackState != PlaybackState.Stopped;
        public bool CanPausePlayback => _audioFileReader != null && PlaybackState == PlaybackState.Playing;

        public double CurrentPlaybackTime => _audioFileReader != null ? _audioFileReader.CurrentTime.TotalSeconds / PlaybackSpeed : 0;
        public double TotalPlaybackDuration => _audioFileReader != null ? _audioFileReader.TotalTime.TotalSeconds / PlaybackSpeed : 0;
        private bool _wasPlaying => PlaybackState == PlaybackState.Playing;

        private float _previousVolume { get; set; }
        public VolumeState VolumeState = VolumeState.Unmuted;

        public void Init(string filePath, float prevSpeed)
        {
            _outputDevice = new WaveOutEvent();
            _outputDevice.PlaybackStopped += (_, e) =>
            {
                if (e.Exception != null)
                {
                    Debug.WriteLine(
                        $"PlaybackStopped: " +
                        $"type={e.Exception.GetType().FullName}, " +
                        $"message={e.Exception.Message}");

                    Debug.WriteLine(e.Exception);
                }
                else
                {
                    Debug.WriteLine(
                        $"PlaybackStopped normally, " +
                        $"state={_outputDevice?.PlaybackState}");
                }
            };

            _audioFileReader = new AudioFileReader(filePath);
            _speedSampleProvider = new SpeedSampleProvider(_audioFileReader.ToSampleProvider(), prevSpeed);
            _outputDevice.Init(_speedSampleProvider);
        }

        public void ReloadAndPlay(string filePath, float prevSpeed)
        {
            Reload(filePath, prevSpeed);
            Play();
        }

        public void ReloadAndMatchState(string filePath, float prevSpeed)
        {
            var wasPlayingBefore = _wasPlaying;
            Reload(filePath, prevSpeed);

            if (wasPlayingBefore)
            {
                Play();
            }
        }

        public void Reload(string filePath, float prevSpeed)
        {
            Unload();
            Init(filePath, prevSpeed);
        }

        public void Unload()
        {
            _outputDevice?.Stop();

            _outputDevice?.Dispose();
            _outputDevice = null;

            _audioFileReader?.Dispose();
            _audioFileReader = null;

            _speedSampleProvider = null;
        }

        public void Play()
        {
            if (CanStartPlaypack)
            {
                _outputDevice?.Play();
            }
        }

        public void Pause()
        {
            if (CanPausePlayback)
            {
                _outputDevice?.Pause();
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
            }
        }

        public void Rewind(double delta)
        {
            SetPlaybackPosition(CurrentPlaybackTime + delta);
        }

        public void Seek(double seconds)
        {
            SetPlaybackPosition(seconds);
        }

        private void SetPlaybackPosition(double seconds)
        {
            if (_audioFileReader == null)
                return;

            var wasPlaying = _outputDevice?.PlaybackState == PlaybackState.Playing;
            if (wasPlaying)
            {
                _outputDevice?.Pause();
            }

            seconds = Math.Clamp(seconds, 0, TotalPlaybackDuration);

            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(seconds * PlaybackSpeed);

            if (wasPlaying)
                _outputDevice?.Play();
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
