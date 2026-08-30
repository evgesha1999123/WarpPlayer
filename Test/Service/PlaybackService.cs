using System;
using NAudio.Wave;
using System.Diagnostics;
using NAudio.Wave.SampleProviders;


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
        public PlaybackState playbackState = PlaybackState.Init;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;
        public PlaybackService(String filePath = @"C:\Users\evges\source\repos\Test\Test\три дня дождя - демоны slowed + reverb - gloomy tears.mp3")
        {
            this.filePath = filePath;

            audioFileReader = new AudioFileReader(filePath);
            outputDevice = new WaveOutEvent();

            var Test = new SmbPitchShiftingSampleProvider(audioFileReader.ToSampleProvider());

            Test.PitchFactor = 0.8f;

            Debug.WriteLine($"Hello from Playback. Now playing: '{filePath}'");

            outputDevice.Init(Test);
        }
        public void Play()
        {
            if (CanStartPlayback())
            {
                outputDevice.Play();
                playbackState = PlaybackState.Playing;
            }
        }

        private bool CanStartPlayback()
        {
            return audioFileReader != null && playbackState != PlaybackState.Playing;
        }

        public void Pause()
        {
            if (CanPausePlayback())
            {
                outputDevice.Pause();
                playbackState = PlaybackState.Paused;
            }
        }

        private bool CanPausePlayback()
        {
            return audioFileReader != null && playbackState == PlaybackState.Playing;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if (CanStopPlayback())
            {
                outputDevice.Stop();
                audioFileReader.CurrentTime = TimeSpan.Zero;
                playbackState = PlaybackState.Stopped;
            }
        }

        private bool CanStopPlayback()
        {
            return audioFileReader != null && playbackState is not (PlaybackState.Stopped or PlaybackState.Init);
        }
    }
}
