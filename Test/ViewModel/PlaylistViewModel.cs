using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using Test.Schema;
using Test.Service;
using Test.Utils;


namespace Test.ViewModel
{
    public class PlaylistViewModel
    {
        public Visibility Visibility;
        public ObservableCollection<TrackViewModel> Tracks { get; } = [];
        
        public MediaLibraryService Library { get; }
        public PlaybackQueue PlaybackQ;
        private PlaybackViewModel _playbackVM;

        public PlaylistViewModel(Visibility visibility, MediaLibraryService mediaLibrary, PlaybackQueue playbackQ, PlaybackViewModel playbackVM)
        {
            Visibility = visibility;
            Library = mediaLibrary;
            PlaybackQ = playbackQ;
            _playbackVM = playbackVM;

            InitExistingTracks();
        }

        private void InitExistingTracks()
        {
            foreach (var tRecord in Library.LookupLibrary())
            {
                AddTrack(tRecord);
            }
        }

        public void AddTrack(TrackRecord track)
        {
            PlaybackQ.Add(track);
            Tracks.Add(new TrackViewModel(track, _playbackVM.UpdateUIandPlay, RemoveTrack));
        }

        public void RemoveTrack(TrackViewModel track)
        {
            Library.DeleteTrack(track.Id.ToString());
            PlaybackQ.Delete(track.Trecord);
            Tracks.Remove(track);
        }
    }
}
