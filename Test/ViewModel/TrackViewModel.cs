using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Test.Schema;
using Test.Utils;


namespace Test.ViewModel
{
    public partial class TrackViewModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public Guid Id { get; init; }
        public string? Album { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationUI { get; set; }
        public TrackRecord Trecord;

        private readonly Func<TrackRecord, bool, Task> _play;
        private readonly Action<TrackViewModel> _remove;

        public TrackViewModel(TrackRecord track, Func<TrackRecord, bool, Task> play, Action<TrackViewModel> remove)
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
        }

        [RelayCommand]
        public async Task Play()
        {
            await _play(Trecord, true);
        }

        [RelayCommand]
        public void Remove()
        {
            _remove(this);
        }
    }
}
