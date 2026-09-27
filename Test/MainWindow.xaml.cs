using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Test.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Test
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public PlaybackViewModel PlaybackVM { get; }
        public MainWindow(PlaybackViewModel playbackVM)
        {
            InitializeComponent();

            PlaybackVM = playbackVM;
        }

        private void PlaybackSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            PlaybackVM.Seek(e.NewValue);
        }

        private void SpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            PlaybackVM?.SetSpeed((float)e.NewValue);
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            PlaybackVM?.SetVolume((float)e.NewValue);
        }
    }
}
