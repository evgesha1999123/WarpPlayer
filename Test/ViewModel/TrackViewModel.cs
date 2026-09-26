using CommunityToolkit.Mvvm.Input;
using System;
using Test.Schema;


namespace Test.ViewModel
{
    public partial class TrackViewModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public Guid Id { get; init; }
        public string? Album { get; set; }
        public TimeSpan Duration { get; set; }

        public TrackRecord Trecord;

        private readonly Action<string, bool> _play;
        private readonly Action<TrackViewModel> _remove;


        public TrackViewModel(TrackRecord track, Action<string, bool> play, Action<TrackViewModel> remove)
        {
            Title = track.Title;
            Artist = track.Artist ?? "";
            Id = track.TrackId;
            Album = track.Album;
            Duration = track.Duration;

            Trecord = track;

            _play = play;
            _remove = remove;
        }

        [RelayCommand]
        public void Play()
        {
            _play(Trecord.FilePath, true);
        }

        [RelayCommand]
        public void Remove()
        {
            _remove(this);
        }
    }
}
