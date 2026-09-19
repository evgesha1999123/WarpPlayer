using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Test.Repo;
using Test.Schema;
using Test.Service;
using Test.Utils;

namespace Test.ViewModel
{
    public partial class PlaybackViewModel : ObservableObject
    {
        public string FilePath => _playbackService.FilePath;

        public int ForwardPlaybackRewindDelta { get; }
        public int BackwardPlaybackRewindDelta { get; }

        public bool IsUpdatingSlider = false;

        public PlaybackState PlaybackState
        {
            get => _playbackService.PlaybackState;
        }

        public Symbol PlayPauseIcon
        {
            get
            {
                return PlaybackState == PlaybackState.Playing ? Symbol.Pause : Symbol.Play;
            }
        }

        public double CurrentPlaybackTime => _playbackService.CurrentPlaybackTime;
        public double TotalTrackLenSeconds => _playbackService.TotalPlaybackDuration;
        public int DisplayTotalTrackLenSeconds => (int)_playbackService.TotalPlaybackDuration;

        public string CurrentTrackPositionUI => TimeSpan.FromSeconds(_playbackService.CurrentPlaybackTime).ToString(@"mm\:ss");
        public string TrackDurationUI => TimeSpan.FromSeconds(_playbackService.TotalPlaybackDuration).ToString(@"mm\:ss");

        public float PlaybackSpeed
        {
            get => _playbackService.PlaybackSpeed;
            set => _playbackService.PlaybackSpeed = value;
        }

        private readonly PlaybackService _playbackService;
        private readonly ITrackRepository<Track, TrackRecord> trackRepository;

        private bool CanStartPlayback { get => PlaybackState == PlaybackState.Init || PlaybackState == PlaybackState.Stopped || PlaybackState == PlaybackState.Paused; }
        private bool CanPausePlayback { get => PlaybackState == PlaybackState.Playing; }
        private readonly DispatcherQueueTimer _timer;


        public PlaybackViewModel(PlaybackService playbackService, ITrackRepository<Track, TrackRecord> trackRepository)
        {
            _playbackService = playbackService;
            this.trackRepository = trackRepository;

            ForwardPlaybackRewindDelta = playbackService.ForwardPlaybackRewindDelta;
            BackwardPlaybackRewindDelta = playbackService.BackwardPlaybackRewindDelta;

            _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.3f);

            // Then, subscribe timer tick event
            _timer.Tick += PlaybackTimer_Tick;
        }

        private void PlaybackTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            IsUpdatingSlider = true;

            if ((int)CurrentPlaybackTime >= (int)TotalTrackLenSeconds)
            {
                Stop();
            }

            OnPropertyChanged(nameof(CurrentPlaybackTime));
            OnPropertyChanged(nameof(CurrentTrackPositionUI));

            IsUpdatingSlider = false;
        }

        [RelayCommand]
        public void TogglePlayback()
        {
            if (CanStartPlayback)
            {
                Play();
            }

            else if (CanPausePlayback)
            {
                Pause();
            }
        }

        [RelayCommand]
        public void Play()
        {
            _playbackService.Play();

            _timer.Start();

            OnPropertyChanged(nameof(PlaybackState));
            OnPropertyChanged(nameof(PlayPauseIcon));
        }

        [RelayCommand]
        public void Pause()
        {
            _playbackService.Pause();

            _timer.Stop();

            OnPropertyChanged(nameof(PlaybackState));
            OnPropertyChanged(nameof(PlayPauseIcon));
        }

        [RelayCommand]
        public void Stop()
        {
            _playbackService.Stop();

            _timer.Stop();

            OnPropertyChanged(nameof(PlaybackState));
            OnPropertyChanged(nameof(PlayPauseIcon));

            OnPropertyChanged(nameof(CurrentPlaybackTime));
        }

        [RelayCommand]
        public void Rewind(int delta)
        {
            _playbackService.Rewind(delta);

            OnPropertyChanged(nameof(CurrentPlaybackTime));
        }

        [RelayCommand]
        public void NextTrack()
        {
           //...
        }

        [RelayCommand]
        public void PreviousTrack()
        {
            //..
        }

        [RelayCommand]
        public void Seek(double seconds)
        {
            if (IsUpdatingSlider)
            {
                return;
            }

            _playbackService.Seek(seconds);
        }

        [RelayCommand]
        public void SetSpeed(float speed)
        {
            PlaybackSpeed = speed;
            OnPropertyChanged(nameof(DisplayTotalTrackLenSeconds));
            OnPropertyChanged(nameof(TrackDurationUI));
        }
    }
}
