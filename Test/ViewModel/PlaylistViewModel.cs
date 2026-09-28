using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
            Tracks.Add(new TrackViewModel(track, _playbackVM.PlayTrackFromQueue, RemoveTrack));
        }

        public async Task RemoveTrack(TrackViewModel track)
        {
            bool isCurrent = PlaybackQ.Current == track.Trecord;

            if (isCurrent)
            {
                var replacement = PlaybackQ.GetTrackAfterCurrentIsRemoved();

                PlaybackQ.Delete(track.Trecord);

                if (replacement != null)
                {
                    await _playbackVM.PlayTrackMatchState(replacement);
                }
                else
                {
                    _playbackVM.StopEmpty();
                }
            }
            else
            {
                PlaybackQ.Delete(track.Trecord);
            }

            Library.DeleteTrack(track.Id.ToString());
            Tracks.Remove(track);
        }
    }
}