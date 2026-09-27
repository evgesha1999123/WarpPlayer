using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Test.Schema;
using Test.Service;
using Test.Utils;

namespace Test.ViewModel
{
    public partial class PlaybackViewModel : ObservableObject
    {
        public int ForwardPlaybackRewindDelta { get; }
        public int BackwardPlaybackRewindDelta { get; }

        public bool IsUpdatingSlider = false;
        public PlaybackState PlaybackState => _playbackService.PlaybackState;
        public PlaybackMode PlaybackMode => _playbackQueue.PlaybackMode;
        public Symbol PlayPauseIcon => PlaybackState == PlaybackState.Playing ? Symbol.Pause : Symbol.Play;
        public Symbol PlaylistButtonIcon => PlaylistVisibility == Visibility.Visible ? Symbol.ClosePane : Symbol.OpenPane;
        public Symbol VolumeStateIcon => _playbackService.VolumeState == VolumeState.Unmuted ? Symbol.Volume : Symbol.Mute;
        public Symbol PlaybackButtonIcon => PlaybackMode switch
        {
            PlaybackMode.Normal => Symbol.Forward,
            PlaybackMode.Shuffle => Symbol.Shuffle,
            PlaybackMode.RepeatAll => Symbol.RepeatAll,
            _ => Symbol.Forward
        };

        string startPart = "Текущий режим плеера: ";
        public string PlaybackButtonToolTip
        {
            get => PlaybackMode switch
            {
                PlaybackMode.Normal => startPart + "обычный",
                PlaybackMode.Shuffle => startPart + "перемешать",
                PlaybackMode.RepeatAll => startPart + "повтор",
                _ => startPart + "обычный"
            };
        }
        public Visibility PlaylistVisibility
        {
            get => PlaylistVM.Visibility;
            set => PlaylistVM.Visibility = value;
        }

        public BitmapImage? CoverArt
        {
            get => _coverArt;
            private set => SetProperty(ref _coverArt, value);
        }

        private BitmapImage? _coverArt;

        public double CurrentPlaybackTime => _playbackService.CurrentPlaybackTime;
        public double TotalTrackLenSeconds => _playbackService.TotalPlaybackDuration;
        public int DisplayTotalTrackLenSeconds => (int)_playbackService.TotalPlaybackDuration;
        public string CurrentTrackNameUI => GetCurrentTrackName();

        public string CurrentTrackPositionUI => TimeSpan.FromSeconds(_playbackService.CurrentPlaybackTime).ToString(@"mm\:ss");
        public string TrackDurationUI => TimeSpan.FromSeconds(_playbackService.TotalPlaybackDuration).ToString(@"mm\:ss");
        public float PlaybackSpeed
        {
            get => _playbackService.PlaybackSpeed;
            set => _playbackService.PlaybackSpeed = value;
        }

        public float Volume
        {
            get => _playbackService.GetVolume();
            set => SetVolume(value);
        }

        private readonly PlaybackService _playbackService;
        private readonly MediaLibraryService _mediaLibraryService;
        private readonly PlaybackQueue _playbackQueue;
        private readonly FolderPicker _folderPicker;
        private readonly DispatcherQueueTimer _timer;
        private bool CanStartPlayback { get => PlaybackState == PlaybackState.Init || PlaybackState == PlaybackState.Stopped || PlaybackState == PlaybackState.Paused; }
        private bool CanPausePlayback { get => PlaybackState == PlaybackState.Playing; }

        public PlaylistViewModel PlaylistVM;

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

            var existingTracks = _mediaLibraryService.LookupLibrary();
            if (existingTracks.Count > 0)
            {
                var track = existingTracks[0];
                _playbackService.Init(track.FilePath, PlaybackSpeed);
                _ = GetCoverArtAsync(track.Cover);
            }

            PlaylistVM = new PlaylistViewModel(_playbackQueue.Size > 0 ? Visibility.Visible : Visibility.Collapsed, _mediaLibraryService, _playbackQueue, this);

            _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.3f);
            // Then, subscribe timer tick event
            _timer.Tick += PlaybackTimer_Tick;
        }

        [RelayCommand]
        public async Task GetCoverArtAsync(byte[] imgBytes)
        {
            CoverArt = await _mediaLibraryService.GetCoverArt(imgBytes);
        }

        // TODO: Вызвано исключение: "System.Runtime.InteropServices.COMException" в WinRT.Runtime.dll
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
                if (_playbackQueue.IsLast)
                {
                    Stop();
                }
                else
                {
                    _ = NextTrack();
                }
            }

            OnPropertyChanged(nameof(CurrentPlaybackTime), nameof(CurrentTrackPositionUI));

            IsUpdatingSlider = false;
        }

        [RelayCommand]
        public async Task TogglePlayback()
        {
            if (CanStartPlayback)
            {
                await Play();
            }

            else if (CanPausePlayback)
            {
                Pause();
            }
        }

        [RelayCommand]
        public async Task Play()
        {
            _playbackService.Play();
            EnsureTimerStarted();

            var currentTrack = _playbackQueue.Current;
            if (_playbackService.IsReady && currentTrack != null)
            {
                await NotifyTrackWasChanged(currentTrack);
            }
            
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

        public void StopEmpty() 
        {
            _timer.Stop(); 
            _playbackService.Unload();
            CoverArt = null;
            OnPropertyChanged(
                nameof(PlaybackState),
                nameof(PlayPauseIcon),
                nameof(CurrentPlaybackTime), 
                nameof(CurrentTrackPositionUI), 
                nameof(TotalTrackLenSeconds),
                nameof(DisplayTotalTrackLenSeconds),
                nameof(TrackDurationUI),
                nameof(CurrentTrackNameUI), 
                nameof(CoverArt)
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
        public async Task NextTrack()
        {
            var track = _playbackQueue.MoveNext();
            if (track != null)
            {
                await PlayTrack(track);
            }
        }

        [RelayCommand]
        public async Task PreviousTrack()
        {
            var track = _playbackQueue.MovePrevious();
            if (track != null)
            {
                await PlayTrack(track);
            }
        }

        public async Task PlayTrack(TrackRecord track)
        {
            await Play(
                track, 
                () => _playbackService.ReloadAndPlay(track.FilePath, PlaybackSpeed)
            );
        }

        public async Task PlayTrackMatchState(TrackRecord track)
        {
            await Play(
                track,
                () =>
                {
                    SetCurrentIndexOfQ(track);
                    _playbackService.ReloadAndMatchState(track.FilePath, PlaybackSpeed);
                }
            );
        }

        public async Task PlayTrackFromQueue(TrackRecord track)
        {
            await Play(
                track,
                () =>
                {
                    SetCurrentIndexOfQ(track);
                    _playbackService.ReloadAndPlay(track.FilePath, PlaybackSpeed);
                }
            );
        }

        private async Task Play(TrackRecord track, Action playbackAction)
        {
            IsUpdatingSlider = true;

            try
            {
                playbackAction();
                EnsureTimerStarted();
                await NotifyTrackWasChanged(track);
            }
            finally
            {
                IsUpdatingSlider = false;
            }
        }

        private void SetCurrentIndexOfQ(TrackRecord track)
        {
            _playbackQueue.SetCurrent(track.FilePath);
        }

        private void EnsureTimerStarted()
        {
            if (!_timer.IsRunning)
            {
                _timer.Start();
            }
        }

        private async Task NotifyTrackWasChanged(TrackRecord track)
        {
            OnPropertyChanged(
                nameof(PlaybackState),
                nameof(PlayPauseIcon),
                nameof(CurrentPlaybackTime),
                nameof(CurrentTrackPositionUI),
                nameof(TrackDurationUI),
                nameof(DisplayTotalTrackLenSeconds),
                nameof(CurrentTrackNameUI)
            );
            await GetCoverArtAsync(track.Cover);
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
            if (!_playbackService.IsReady)
            {
                return;
            }

            PlaybackSpeed = speed;
            OnPropertyChanged(nameof(DisplayTotalTrackLenSeconds), nameof(TrackDurationUI));
        }

        [RelayCommand]
        public void SetDefaultSpeed()
        {
            SetSpeed(1f);
            OnPropertyChanged(nameof(PlaybackSpeed));
        }

        [RelayCommand]
        public void SetVolume(float volume)
        {
            _playbackService.SetVolume(volume);
        }

        [RelayCommand]
        public void ToggleMute()
        {
            if (_playbackService.VolumeState == VolumeState.Unmuted) {
                _playbackService.Mute();
            } else {
                _playbackService.Unmute();
            }

            OnPropertyChanged(nameof(VolumeStateIcon));
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
            bool isPlaybackServiceInitedNow = false;

            if (!_playbackService.IsReady && tracks.Count > 0)
            {
                var track = tracks[0];
                _playbackService.Init(track.FilePath, PlaybackSpeed);
                await GetCoverArtAsync(track.Cover);
                isPlaybackServiceInitedNow = true;
            }

            foreach (var track in tracks)
            {
                PlaylistVM.AddTrack(track);
            }

            if (isPlaybackServiceInitedNow)
            {
                OnPropertyChanged(nameof(CurrentTrackNameUI));
            }
        }

        [RelayCommand]
        public void ShowHidePlaylist()
        {
            PlaylistVisibility = PlaylistVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            OnPropertyChanged(nameof(PlaylistButtonIcon), nameof(PlaylistVisibility));
        }

        [RelayCommand]
        public void TogglePlaybackMode()
        {
            _playbackQueue.TogglePlaybackMode();
            OnPropertyChanged(nameof(PlaybackMode), nameof(PlaybackButtonIcon), nameof(PlaybackButtonToolTip));
        }

        public string GetCurrentTrackName()
        {
            var track = _playbackQueue.Current;
            return track != null ? $"{track?.Artist} - {track?.Title}" : " ";
        }

    }
}
