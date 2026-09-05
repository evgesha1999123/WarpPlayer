using Test.Service;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Test.ViewModel
{
    public partial class PlaybackViewModel : ObservableObject
    {
        private readonly PlaybackService playbackService;

        [ObservableProperty]
        private PlaybackState playbackState;
        public string FilePath => playbackService.filePath;

        public int ForwardPlaybackRewindDelta { get; private set; } = 0;
        public int BackwardPlaybackRewindDelta { get; private set; } = 0;

        public PlaybackViewModel(PlaybackService playbackService)
        {
            this.playbackService = playbackService;
            playbackState = playbackService.playbackState;

            ForwardPlaybackRewindDelta = playbackService.ForwardPlaybackRewindDelta;
            BackwardPlaybackRewindDelta = playbackService.BackwardPlaybackRewindDelta;
            
        }

        [RelayCommand]
        private void Play()
        {
            playbackService.Play();
            playbackState = playbackService.playbackState;
        }

        [RelayCommand]
        private void Pause()
        {
            playbackService.Pause();
            playbackState = playbackService.playbackState;
        }

        [RelayCommand]
        private void Stop()
        {
            playbackService.Stop();
            playbackState = playbackService.playbackState;
        }

        [RelayCommand]
        private void Rewind(int delta)
        {
            playbackService.Rewind(delta);
        }

    }
}
