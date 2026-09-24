using Microsoft.UI.Xaml;
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
        public PlaybackService PlaybackService;

        public PlaylistViewModel(Visibility visibility, MediaLibraryService mediaLibrary, PlaybackQueue playbackQ, PlaybackService playbackService)
        {
            Visibility = visibility;
            Library = mediaLibrary;
            PlaybackQ = playbackQ;
            PlaybackService = playbackService;

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
            Tracks.Add(new TrackViewModel(track, PlaybackService.ResetAndPlay, RemoveTrack));
        }

        public void RemoveTrack(TrackViewModel track)
        {
            Library.DeleteTrack(track.Id.ToString());
            PlaybackQ.Delete(track.Trecord);
            Tracks.Remove(track);
        }
    }
}
