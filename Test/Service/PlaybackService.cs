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
        public String filePath;

        public int ForwardPlaybackRewindDelta { get; private set; } = 9;
        public int BackwardPlaybackRewindDelta { get; private set; } = -9;

        public float playbackSpeed
        {
            get => speedSampleProvider.Speed;
            set => speedSampleProvider.Speed = value;
        }

        public PlaybackState playbackState
        {
            get => _playbackState;
            private set => _playbackState = value;
        }

        public double currentPlaybackTime => audioFileReader.CurrentTime.TotalSeconds / playbackSpeed;
        public double totalPlaybackDuration => audioFileReader.TotalTime.TotalSeconds / playbackSpeed;

        private SpeedSampleProvider speedSampleProvider;

        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;

        private PlaybackState _playbackState = PlaybackState.Init;

        private bool canStartPlaypack { 
            get
            {
                return audioFileReader != null && playbackState != PlaybackState.Playing;
            }
        }

        private bool canStopPlaypack { 
            get
            {
                return audioFileReader != null && playbackState is not (PlaybackState.Stopped or PlaybackState.Init);
            } 
        }

        private bool canPausePlayback { 
            get
            {
                return audioFileReader != null && playbackState == PlaybackState.Playing;
            } 
        }

        public PlaybackService(String filePath = @"C:\Users\evges\source\repos\Test\Test\три дня дождя - демоны slowed + reverb - gloomy tears.mp3")
        {
            this.filePath = filePath;

            audioFileReader = new AudioFileReader(filePath);
            outputDevice = new WaveOutEvent();

            speedSampleProvider = new SpeedSampleProvider(audioFileReader.ToSampleProvider());

            Debug.WriteLine($"Hello from Playback. Now playing: '{filePath}'");
            Debug.WriteLine($"Track Length: '{audioFileReader.TotalTime}'");

            speedSampleProvider.Speed = playbackSpeed;

            outputDevice.Init(speedSampleProvider);
        }
        public void Play()
        {
            if (canStartPlaypack)
            {
                outputDevice.Play();
                playbackState = PlaybackState.Playing;
            }
        }

        public void Pause()
        {
            if (canPausePlayback)
            {
                outputDevice.Pause();
                playbackState = PlaybackState.Paused;
            }
        }

        public void Rewind(double delta)
        {
            bool wasPlaying = playbackState == PlaybackState.Playing;

            if (wasPlaying) 
            {
                outputDevice.Stop(); 
            }

            double newPlaybackSeconds = currentPlaybackTime + delta;

            newPlaybackSeconds = Math.Clamp(newPlaybackSeconds, 0, totalPlaybackDuration);

            audioFileReader.CurrentTime = TimeSpan.FromSeconds(newPlaybackSeconds * playbackSpeed);

            if (wasPlaying)
            { 
                outputDevice.Play(); 
            }
        }

        public void Stop()
        {
            if (canStopPlaypack)
            {
                outputDevice.Stop();
                audioFileReader.CurrentTime = TimeSpan.Zero;
                playbackState = PlaybackState.Stopped;
            }
        }

        public void Seek(double seconds)
        {
            double sourceSeconds = seconds * playbackSpeed;
            sourceSeconds = Math.Clamp(sourceSeconds, 0, audioFileReader.TotalTime.TotalSeconds);
            audioFileReader.CurrentTime = TimeSpan.FromSeconds(sourceSeconds);
        }
    }
}
