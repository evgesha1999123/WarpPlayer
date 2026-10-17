using Microsoft.UI.Xaml;
using Player.Schema;
using Player.Service;
using Player.Utils;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Player.ViewModel
{
    public class PlaylistViewModel
    {
        public Visibility Visibility;
        public ObservableCollection<TrackViewModel> Tracks { get; } = [];
        public MediaLibraryService Library { get; }
        public PlaybackQueue PlaybackQ;
        private readonly PlaybackViewModel _playbackVM;

        private TrackViewModel? CurrentActiveTrack;
        private TrackViewModel? PreviousActiveTrack; 


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

            if (Tracks.Count > 0)
            {
                SetActive(Tracks[0]);
            }
        }

        public void SetActive(TrackRecord track)
        {
            foreach(var tView in Tracks)
            {
                if (tView.Trecord == track)
                {
                    SetActive(tView);
                    return;
                }
            }
        }

        public void SetActive(TrackViewModel track)
        {
            var previous = PreviousActiveTrack;

            if (previous != null)
            {
                SetInactive(previous);
            }

            CurrentActiveTrack = track;
            track.Activate();

            PreviousActiveTrack = CurrentActiveTrack;
        }

        public TrackViewModel? GetCurrentActiveView()
        {
            return CurrentActiveTrack;
        }

        private static void SetInactive(TrackViewModel track)
        {
            track.Deactivate();
        }

        public void AddTrack(TrackRecord track)
        {
            PlaybackQ.Add(track);
            Tracks.Add(new TrackViewModel(track, _playbackVM.PlayTrackFromQueue, RemoveTrack, _playbackVM.TogglePlaybackWrapper));
        }

        public async Task RemoveTrack(TrackViewModel track)
        {
            bool isCurrent = PlaybackQ.Current == track.Trecord;

            if (isCurrent)
            {
                var replacement = PlaybackQ.GetTrackAfterCurrentIsRemoved();

                PlaybackQ.Delete(track.Trecord);

                Tracks.Remove(track);
                Library.DeleteTrack(track.Id.ToString());

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

                Tracks.Remove(track);
                Library.DeleteTrack(track.Id.ToString());
            }

            _playbackVM.SetNotificationMessage(
                $"Трек удален из плейлиста: " +
                $"'{System.IO.Path.GetFileNameWithoutExtension(track.Trecord.FilePath)}'");
        }

        public void RemoveTrackSilently(TrackRecord track)
        {
            PlaybackQ.Delete(track);

            var view = Tracks.FirstOrDefault(t => t.Trecord == track);

            if (view != null)
            {
                Tracks.Remove(view);
            }

            Library.DeleteTrack(track.TrackId.ToString());
        }

        public void Clear()
        {
            Tracks.Clear();
            PlaybackQ.Clear();
            Library.DeleteAllTracks();
            _playbackVM.SetNotificationMessage("Плейлист очищен");
        }
    }
}