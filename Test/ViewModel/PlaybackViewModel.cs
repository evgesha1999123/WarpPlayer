using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Test.Schema;
using Test.Service;
using Test.Utils;
using Test.Utils.File;
using static Test.Utils.File.FilePicker;


namespace Test.ViewModel
{
    public partial class PlaybackViewModel : ObservableObject
    {
        public int ForwardPlaybackRewindDelta { get; }
        public int BackwardPlaybackRewindDelta { get; }

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

        private DispatcherQueueTimer _notificationTimer;
        private string _notificationMessage = "";
        public string NotificationMessage
        {
            get => _notificationMessage;
            set => SetProperty(ref _notificationMessage, value);
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
        private readonly FilePicker _filePicker;
        private readonly DispatcherQueueTimer _playbackTimer;
        private bool CanStartPlayback => _playbackService.CanStartPlaypack;
        private bool CanPausePlayback => _playbackService.CanPausePlayback;

        public bool IsSeeking = false;

        public PlaylistViewModel PlaylistVM;

        public PlaybackViewModel(PlaybackService playbackService, MediaLibraryService mediaLibraryService, PlaybackQueue playbackQueue, FilePicker folderPicker)
        {
            _playbackService = playbackService;
            _mediaLibraryService = mediaLibraryService;
            _playbackQueue = playbackQueue;

            ForwardPlaybackRewindDelta = playbackService.ForwardPlaybackRewindDelta;
            BackwardPlaybackRewindDelta = playbackService.BackwardPlaybackRewindDelta;

            _mediaLibraryService = mediaLibraryService;
            _playbackQueue = playbackQueue;
            _filePicker = folderPicker;

            _notificationTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _notificationTimer.Interval = TimeSpan.FromSeconds(10f);
            _notificationTimer.Tick += NotificationTimer_TickOneTime;

            var existingTracks = _mediaLibraryService.LookupLibrary();

            if (existingTracks.Count > 0) {
                var track = existingTracks[0];
                _playbackService.Init(track.FilePath, PlaybackSpeed);
                _ = GetCoverArtAsync(track.Cover);
                SetNotificationMessage($"Треков в плейлисте: {existingTracks.Count}");
            } else {
                SetNotificationMessage("Импортируйте файлы для добавления в плейлист");
            }

            PlaylistVM = new PlaylistViewModel(_playbackQueue.Size > 0 ? Visibility.Visible : Visibility.Collapsed, _mediaLibraryService, _playbackQueue, this);

            _playbackTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _playbackTimer.Interval = TimeSpan.FromSeconds(0.3f);
            _playbackTimer.Tick += PlaybackTimer_Tick;
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

        public void SetNotificationMessage(string text)
        {
            EnsureTimerStopped(_notificationTimer);
            EnsureTimerStarted(_notificationTimer);
            _notificationMessage = NormalizeMessage(text);
            OnPropertyChanged(nameof(NotificationMessage));
        }

        private static string NormalizeMessage(string text)
        {
            if (text.Length >= 62)
            {
                return text[0..60] + "...";
            }
            return text;
        }

        private void PlaybackTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            if (IsSeeking)
            {
                return;
            }

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
        }

        private void NotificationTimer_TickOneTime(DispatcherQueueTimer sender, object args)
        {
            SetNotificationMessage("");
            EnsureTimerStopped(_notificationTimer);
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

            var view = PlaylistVM.GetCurrentActiveView();
            view?.SetState(true, PlaybackState);
        }

        public PlaybackState TogglePlaybackWrapper()
        {
            TogglePlayback();
            return PlaybackState;
        }

        public void Play()
        {
            _playbackService.Play();
            EnsureTimerStarted(_playbackTimer);

            var currentTrack = _playbackQueue.Current;
            if (_playbackService.IsReady && currentTrack != null)
            {
                NotifyTrackWasChanged();
            }
        }

        [RelayCommand]
        public async Task PlayAsync()
        {
            _playbackService.Play();
            EnsureTimerStarted(_playbackTimer);

            var currentTrack = _playbackQueue.Current;
            if (_playbackService.IsReady && currentTrack != null)
            {
                NotifyTrackWasChanged();
                await GetCoverArtAsync(currentTrack.Cover);
            }
            
        }

        [RelayCommand]
        public void Pause()
        {
            _playbackService.Pause();
            EnsureTimerStopped(_playbackTimer);

            OnPropertyChanged(nameof(PlaybackState), nameof(PlayPauseIcon));
        }

        [RelayCommand]
        public void Stop()
        {
            _playbackService.Stop();
            EnsureTimerStopped(_playbackTimer);

            OnPropertyChanged(
                nameof(PlaybackState),
                nameof(PlayPauseIcon), 
                nameof(CurrentPlaybackTime),
                nameof(CurrentTrackPositionUI)
            );

            PlaylistVM.GetCurrentActiveView()?.SetState(true, PlaybackState);
        }

        public void StopEmpty() 
        {
            EnsureTimerStopped(_playbackTimer); 
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
            _playbackService.Rewind(delta);
            OnPropertyChanged(nameof(CurrentPlaybackTime));
        }

        [RelayCommand]
        public async Task NextTrack()
        {
            var track = _playbackQueue.GetNext();
            if (track != null)
            {
                await PlayTrack(track);
                var view = PlaylistVM.GetCurrentActiveView();
                view?.SetState(true, PlaybackState);
            }
        }

        [RelayCommand]
        public async Task PreviousTrack()
        {
            var track = _playbackQueue.GetPrevious();
            if (track != null)
            {
                await PlayTrack(track);
                var view = PlaylistVM.GetCurrentActiveView();
                view?.SetState(true, PlaybackState);
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

        public async Task<PlaybackState> PlayTrackFromQueue(TrackRecord track)
        {
            await Play(
                track,
                () =>
                {
                    SetCurrentIndexOfQ(track);
                    _playbackService.ReloadAndPlay(track.FilePath, PlaybackSpeed);
                }
            );
            return PlaybackState;
        }

        private async Task Play(TrackRecord track, Action playbackAction)
        {
            playbackAction();
            EnsureTimerStarted(_playbackTimer);
            NotifyTrackWasChanged();

            await GetCoverArtAsync(track.Cover);
            PlaylistVM.SetActive(track);
            var activeView = PlaylistVM.GetCurrentActiveView();
            activeView?.SetState(true, PlaybackState);
        }

        private void SetCurrentIndexOfQ(TrackRecord track)
        {
            _playbackQueue.SetCurrent(track.FilePath);
        }

        private static void EnsureTimerStarted(DispatcherQueueTimer timer)
        {
            if (!timer.IsRunning)
            {
                timer.Start();
            }
        }

        private static void EnsureTimerStopped(DispatcherQueueTimer timer)
        {
            if (timer.IsRunning)
            {
                timer.Stop();
            }
        }

        private void NotifyTrackWasChanged()
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
        }

        [RelayCommand]
        public void Seek(double seconds)
        {
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
        public async Task SelectFiles()
        {
            await ExecImportPipline(_filePicker.PickFilesAsync);
        }

        [RelayCommand]
        public async Task SelectFolder()
        {
            await ExecImportPipline(_filePicker.PickFolderAsync);
        }

        private async Task ExecImportPipline(Func<Task<FilePickerResponse>> pickerAction)
        {
            var response = await pickerAction();

            if (!response.HasContent)
            {
                return;
            }

            await InitializeNewTracks(response.Files);
        }

        private async Task InitializeNewTracks(IReadOnlyList<string> files)
        {
            var importResponse = _mediaLibraryService.ImportTracks(files);
            bool isPlaybackServiceInitedNow = await LoadTracksAndGetPlaylistStatus(importResponse.TrackRecords);

            if (isPlaybackServiceInitedNow)
            {
                OnPropertyChanged(nameof(CurrentTrackNameUI));
            }

            if (importResponse.Skipped > 0)
            {
                SetNotificationMessage($"Треков загружено: {importResponse.TrackRecords.Count}. Пропущено дубликатов: {importResponse.Skipped}");
            } else {
                SetNotificationMessage($"Треков загружено: {importResponse.TrackRecords.Count}");
            }
        }

        [RelayCommand]
        private async Task ClearPlaylist()
        {
            Stop();
            PlaylistVM.Clear();
            StopEmpty();
        }

        private async Task<bool> LoadTracksAndGetPlaylistStatus(IReadOnlyList<TrackRecord> tracks)
        {
            bool wasPlaybackInitializedNow = await InitPlaybackOrPass(tracks);
            AddToPlaylist(tracks);

            if (wasPlaybackInitializedNow && tracks.Count > 0)
            {
                PlaylistVM.SetActive(tracks[0]);
            }
            
            return wasPlaybackInitializedNow;
        }

        private async Task<bool> InitPlaybackOrPass(IReadOnlyList<TrackRecord> tracks)
        {
            bool wasPlaybackInitializedNow = false;

            if (!_playbackService.IsReady && tracks.Count > 0)
            {
                var track = tracks[0];
                _playbackService.Init(track.FilePath, PlaybackSpeed);
                await GetCoverArtAsync(track.Cover);
                
                wasPlaybackInitializedNow = true;
            }

            return wasPlaybackInitializedNow;
        }

        private void AddToPlaylist(IReadOnlyList<TrackRecord> tracks)
        {
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
