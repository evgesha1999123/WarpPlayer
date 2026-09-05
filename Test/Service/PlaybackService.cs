using System;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System;


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

        public int totalTrackLenSeconds { get; set; } = 0;

        public PlaybackState playbackState = PlaybackState.Init;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;

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

            totalTrackLenSeconds = (int)audioFileReader.TotalTime.TotalSeconds;

            var speedSampleProvider = new SpeedSampleProvider(audioFileReader.ToSampleProvider());

            Debug.WriteLine($"Hello from Playback. Now playing: '{filePath}'");
            Debug.WriteLine($"Track Length: '{audioFileReader.TotalTime}'");

            speedSampleProvider.Speed = 1f;

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

        public void Rewind(int delta)
        {

            int newCurrentTime = (int)audioFileReader.CurrentTime.TotalSeconds + delta;

            if (newCurrentTime >= totalTrackLenSeconds)
            {
                Debug.WriteLine("END");
                audioFileReader.CurrentTime = audioFileReader.TotalTime;
            }

            else if (newCurrentTime > 0)
            {
                Debug.WriteLine("Rewind");
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(newCurrentTime);
            }

            else
            {
                Debug.WriteLine("START");
                audioFileReader.CurrentTime = TimeSpan.Zero;
            }

            Debug.WriteLine($"current position: {audioFileReader.CurrentTime} | Length: {audioFileReader.TotalTime}");

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
    }
}
