using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Player.Schema;
using Player.Service;
using Player.Utils;


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

            Debug.WriteLine(
                $"Prev: {PreviousActiveTrack?.Trecord.FilePath.Split('/')[^1] ?? "null"} " +
                $"| Current: {CurrentActiveTrack.Trecord.FilePath.Split('/')[^1]}"
            );
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

                if (replacement != null)
                {
                    await _playbackVM.PlayTrackMatchState(replacement);
                } else {
                    _playbackVM.StopEmpty();
                }
            } else {
                PlaybackQ.Delete(track.Trecord);
            }

            Library.DeleteTrack(track.Id.ToString());
            Tracks.Remove(track);
            _playbackVM.SetNotificationMessage($"Трек удален из плейлиста: '{System.IO.Path.GetFileName(track.Trecord.FilePath).Split(".")[0]}'");
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