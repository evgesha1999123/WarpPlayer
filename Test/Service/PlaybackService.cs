using System;
using NAudio.Wave;
using System.Diagnostics;


namespace Test.Service
{
    public enum PlaybackState
    {
        Init,
        Playing,
        Stopped,
        Paused,
    }

    public class PlaybackService : IPlayable
    {
        public string FilePath;

        public int ForwardPlaybackRewindDelta { get; private set; } = 9;
        public int BackwardPlaybackRewindDelta { get; private set; } = -9;

        public float PlaybackSpeed
        {
            get => _speedSampleProvider.Speed;
            set => _speedSampleProvider.Speed = value;
        }

        public PlaybackState PlaybackState
        {
            get => _playbackState;
            private set => _playbackState = value;
        }

        public double CurrentPlaybackTime => _audioFileReader.CurrentTime.TotalSeconds / PlaybackSpeed;
        public double TotalPlaybackDuration => _audioFileReader.TotalTime.TotalSeconds / PlaybackSpeed;

        private readonly SpeedSampleProvider _speedSampleProvider;

        private readonly WaveOutEvent _outputDevice;
        private readonly AudioFileReader _audioFileReader;

        private PlaybackState _playbackState = PlaybackState.Init;

        private bool CanStartPlaypack { 
            get
            {
                return _audioFileReader != null && PlaybackState != PlaybackState.Playing;
            }
        }

        private bool CanStopPlaypack { 
            get
            {
                return _audioFileReader != null && PlaybackState is not (PlaybackState.Stopped or PlaybackState.Init);
            } 
        }

        private bool CanPausePlayback { 
            get
            {
                return _audioFileReader != null && PlaybackState == PlaybackState.Playing;
            } 
        }

        public PlaybackService(String filePath = @"C:\Users\evges\source\repos\Test\Test\три дня дождя - демоны slowed + reverb - gloomy tears.mp3")
        {
            FilePath = filePath;

            _audioFileReader = new AudioFileReader(filePath);
            _outputDevice = new WaveOutEvent();

            _speedSampleProvider = new SpeedSampleProvider(_audioFileReader.ToSampleProvider());

            Debug.WriteLine($"Hello from Playback. Now playing: '{filePath}'");
            Debug.WriteLine($"Track Length: '{_audioFileReader.TotalTime}'");

            _speedSampleProvider.Speed = PlaybackSpeed;

            _outputDevice.Init(_speedSampleProvider);
        }
        public void Play()
        {
            if (CanStartPlaypack)
            {
                _outputDevice.Play();
                PlaybackState = PlaybackState.Playing;
            }
        }

        public void Pause()
        {
            if (CanPausePlayback)
            {
                _outputDevice.Pause();
                PlaybackState = PlaybackState.Paused;
            }
        }

        public void Rewind(double delta)
        {
            bool wasPlaying = PlaybackState == PlaybackState.Playing;

            if (wasPlaying) 
            {
                _outputDevice.Stop(); 
            }

            double newPlaybackSeconds = CurrentPlaybackTime + delta;

            newPlaybackSeconds = Math.Clamp(newPlaybackSeconds, 0, TotalPlaybackDuration);

            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(newPlaybackSeconds * PlaybackSpeed);

            if (wasPlaying)
            { 
                _outputDevice.Play(); 
            }
        }

        public void Stop()
        {
            if (CanStopPlaypack)
            {
                _outputDevice.Stop();
                _audioFileReader.CurrentTime = TimeSpan.Zero;
                PlaybackState = PlaybackState.Stopped;
            }
        }

        public void Seek(double seconds)
        {
            double sourceSeconds = seconds * PlaybackSpeed;
            sourceSeconds = Math.Clamp(sourceSeconds, 0, _audioFileReader.TotalTime.TotalSeconds);
            _audioFileReader.CurrentTime = TimeSpan.FromSeconds(sourceSeconds);
        }
    }
}
