using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Player.Schema;
using Player.Utils.Converter;


namespace Player.ViewModel
{
    public partial class TrackViewModel : ObservableObject
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public Guid Id { get; init; }
        public string? Album { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationUI { get; set; }
        public TrackRecord Trecord;

        private bool IsActive = false;
        private readonly string ActiveColor = "LimeGreen";
        private readonly string InactiveColor = "Transparent";
        public string StatusColor => IsActive ? ActiveColor : InactiveColor;

        private readonly Func<TrackRecord, Task<PlaybackState>> _play;
        private readonly Func<TrackViewModel, Task> _remove;
        private readonly Func<PlaybackState> _toggle;

        private PlaybackState PlaybackState { get; set; }
        public Symbol PlayPauseIcon => (PlaybackState == PlaybackState.Playing && IsActive) ? Symbol.Pause : Symbol.Play;

        public TrackViewModel(TrackRecord track, Func<TrackRecord, Task<PlaybackState>> play, Func<TrackViewModel, Task> remove, Func<PlaybackState> toggle)
        {
            Title = track.Title;
            Artist = track.Artist ?? "";
            Id = track.TrackId;
            Album = track.Album;
            Duration = track.Duration;

            DurationUI = PlaybackTimeConverter.ToUI_MM_SS(Duration);

            Trecord = track;

            _play = play;
            _remove = remove;
            _toggle = toggle;
        }

        [RelayCommand]
        public async Task Play()
        {
            if (IsActive) {
                PlaybackState = _toggle();
            } else {
                PlaybackState = await _play(Trecord);
            }

            NotifyActivityStatus();
            NotifyPlaybackStatus();
        }

        [RelayCommand]
        public async Task Remove()
        {
            await _remove(this);
        }

        public void Activate()
        {
            IsActive = true;
            NotifyPlaybackStatus();
            NotifyActivityStatus();
        }

        public void Deactivate()
        {
            IsActive = false;
            NotifyPlaybackStatus();
            NotifyActivityStatus();
        }

        public void SetState(bool isActive, PlaybackState playbackState)
        {
            IsActive = isActive;
            PlaybackState = playbackState;
            NotifyActivityStatus();
            NotifyPlaybackStatus();
        }

        private void NotifyActivityStatus()
        {
            OnPropertyChanged(nameof(IsActive));
            OnPropertyChanged(nameof(StatusColor));
        }

        private void NotifyPlaybackStatus()
        {
            OnPropertyChanged(nameof(PlaybackState));
            OnPropertyChanged(nameof(PlayPauseIcon));
        }
    }
}
