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

        private readonly Func<TrackRecord, Task> _play;
        private readonly Func<TrackViewModel, Task> _remove;

        public TrackViewModel(TrackRecord track, Func<TrackRecord, Task> play, Func<TrackViewModel, Task> remove)
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
            await _play(Trecord);
        }

        [RelayCommand]
        public async Task Remove()
        {
            await _remove(this);
        }
    }
}
