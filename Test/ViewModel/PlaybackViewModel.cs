using Test.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;
using System;
using System.Diagnostics;

namespace Test.ViewModel
{
    public partial class PlaybackViewModel : ObservableObject
    {
        public string FilePath => playbackService.filePath;

        public int ForwardPlaybackRewindDelta { get; }
        public int BackwardPlaybackRewindDelta { get; }

        public bool isUpdatingSlider = false;

        public PlaybackState playbackState
        {
            get => playbackService.playbackState;
        }

        public Symbol playPauseIcon
        {
            get
            {
                return playbackState == PlaybackState.Playing ? Symbol.Pause : Symbol.Play;
            }
        }

        public double currentPlaybackTime => playbackService.currentPlaybackTime;
        public double totalTrackLenSeconds => playbackService.totalPlaybackDuration;
        public int displayTotalTrackLenSeconds => (int)playbackService.totalPlaybackDuration;

        public float playbackSpeed
        {
            get => playbackService.playbackSpeed;
            set => playbackService.playbackSpeed = value;
        }

        private readonly PlaybackService playbackService;

        private bool canStartPlayback { get => playbackState == PlaybackState.Init || playbackState == PlaybackState.Stopped || playbackState == PlaybackState.Paused; }
        private bool canPausePlayback { get => playbackState == PlaybackState.Playing; }
        private readonly DispatcherQueueTimer Timer;


        public PlaybackViewModel(PlaybackService playbackService)
        {
            this.playbackService = playbackService;

            ForwardPlaybackRewindDelta = playbackService.ForwardPlaybackRewindDelta;
            BackwardPlaybackRewindDelta = playbackService.BackwardPlaybackRewindDelta;

            Timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            Timer.Interval = TimeSpan.FromSeconds(0.3f);

            // Then, subscribe timer tick event
            Timer.Tick += PlaybackTimer_Tick;
        }

        private void PlaybackTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            isUpdatingSlider = true;

            if ((int)currentPlaybackTime >= (int)totalTrackLenSeconds)
            {
                Stop();
            }

            OnPropertyChanged(nameof(currentPlaybackTime));

            isUpdatingSlider = false;
        }

        [RelayCommand]
        public void TogglePlayback()
        {
            if (canStartPlayback)
            {
                Play();
            }

            else if (canPausePlayback)
            {
                Pause();
            }
        }

        [RelayCommand]
        public void Play()
        {
            playbackService.Play();

            Timer.Start();

            OnPropertyChanged(nameof(playbackState));
            OnPropertyChanged(nameof(playPauseIcon));
        }

        [RelayCommand]
        public void Pause()
        {
            playbackService.Pause();

            Timer.Stop();

            OnPropertyChanged(nameof(playbackState));
            OnPropertyChanged(nameof(playPauseIcon));
        }

        [RelayCommand]
        public void Stop()
        {
            playbackService.Stop();

            Timer.Stop();

            OnPropertyChanged(nameof(playbackState));
            OnPropertyChanged(nameof(playPauseIcon));

            OnPropertyChanged(nameof(currentPlaybackTime));
        }

        [RelayCommand]
        public void Rewind(int delta)
        {
            playbackService.Rewind(delta);

            //if (currentPlaybackTime >= totalTrackLenSeconds)
            //{
            //    Stop();
            //    Play();
            //}

            OnPropertyChanged(nameof(currentPlaybackTime));
        }

        [RelayCommand]
        public void Seek(double seconds)
        {
            if (isUpdatingSlider)
            {
                return;
            }

            playbackService.Seek(seconds);
        }

        [RelayCommand]
        public void SetSpeed(float speed)
        {
            playbackSpeed = speed;
        }
    }
}
