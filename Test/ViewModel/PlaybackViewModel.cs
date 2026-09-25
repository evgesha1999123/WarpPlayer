using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private readonly MediaLibraryService _mediaLibraryService;
        private readonly PlaybackQueue _playbackQueue;
        private readonly FolderPicker _folderPicker;

        private bool CanStartPlayback { get => PlaybackState == PlaybackState.Init || PlaybackState == PlaybackState.Stopped || PlaybackState == PlaybackState.Paused; }
        private bool CanPausePlayback { get => PlaybackState == PlaybackState.Playing; }
        private readonly DispatcherQueueTimer _timer;

        public PlaylistViewModel PlaylistVM;

        public Symbol PlaylistButtonIcon
        {
            get
            {
                return PlaylistVisibility == Visibility.Visible ? Symbol.ClosePane : Symbol.OpenPane;
            }
        }
        public Visibility PlaylistVisibility
        {
            get
            {
                return PlaylistVM.Visibility;
            }

            set
            {
                PlaylistVM.Visibility = value;
            }
        }


        public PlaybackViewModel(PlaybackService playbackService, MediaLibraryService mediaLibraryService, PlaybackQueue playbackQueue, FolderPicker folderPicker)
        {
            _playbackService = playbackService;
            _mediaLibraryService = mediaLibraryService;
            _playbackQueue = playbackQueue;

            ForwardPlaybackRewindDelta = playbackService.ForwardPlaybackRewindDelta;
            BackwardPlaybackRewindDelta = playbackService.BackwardPlaybackRewindDelta;

            _mediaLibraryService = mediaLibraryService;
            _playbackQueue = playbackQueue;
            _folderPicker = folderPicker;

            PlaylistVM = new PlaylistViewModel(_playbackQueue.Size > 0 ? Visibility.Visible : Visibility.Collapsed, _mediaLibraryService, _playbackQueue, _playbackService);

            // Then, subscribe timer tick event
            _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.3f);
            _timer.Tick += PlaybackTimer_Tick;
        }

        new protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(string propertyName, params string[] propertyNames)
        {
            OnPropertyChanged(propertyName);
            foreach (var name in propertyNames)
            {
                OnPropertyChanged(name);
            }
        }

        private void PlaybackTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            IsUpdatingSlider = true;

            if ((int)CurrentPlaybackTime >= (int)TotalTrackLenSeconds)
            {
                Stop();
            }

            OnPropertyChanged(nameof(CurrentPlaybackTime), nameof(CurrentTrackPositionUI));

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

            OnPropertyChanged(nameof(PlaybackState), nameof(PlayPauseIcon));
        }

        [RelayCommand]
        public void Pause()
        {
            _playbackService.Pause();
            _timer.Stop();

            OnPropertyChanged(nameof(PlaybackState), nameof(PlayPauseIcon));
        }

        [RelayCommand]
        public void Stop()
        {
            _playbackService.Stop();
            _timer.Stop();

            OnPropertyChanged(
                nameof(PlaybackState),
                nameof(PlayPauseIcon), 
                nameof(CurrentPlaybackTime),
                nameof(CurrentTrackPositionUI)
            );
        }

        [RelayCommand]
        public void Rewind(int delta)
        {
            IsUpdatingSlider = true;
            _playbackService.Rewind(delta);
            OnPropertyChanged(nameof(CurrentPlaybackTime));
            IsUpdatingSlider = false;
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
            OnPropertyChanged(nameof(DisplayTotalTrackLenSeconds), nameof(TrackDurationUI));
        }

        [RelayCommand]
        public async Task SelectFolder()
        {
            var folder = await _folderPicker.PickFolderAsync();

            if (folder == null)
            {
                return;
            }

            var tracks = _mediaLibraryService.ImportTracks(folder);

            foreach (var track in tracks)
            {
                PlaylistVM.AddTrack(track);
            }
        }

        [RelayCommand]
        public void ShowHidePlaylist()
        {
            PlaylistVisibility = PlaylistVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            OnPropertyChanged(nameof(PlaylistButtonIcon), nameof(PlaylistVisibility));
        }
    }
}
