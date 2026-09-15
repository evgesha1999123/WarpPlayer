using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Diagnostics;

using Test.Service;
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
        public MainWindow()
        {
            InitializeComponent();

            var playbackService = new PlaybackService();

            PlaybackVM = new PlaybackViewModel(playbackService);
        }

        private void PlaybackSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            PlaybackVM.Seek(e.NewValue);
        }
    }
}
