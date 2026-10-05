using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using System;
using System.Diagnostics;
using Test.ViewModel;


namespace Test
{
    public sealed partial class MainWindow : Window
    {
        public PlaybackViewModel PlaybackVM { get; }

        private bool _isUserDraggingPlaybackSlider;
        private Pointer? _capturedPointer;   // keep track of the pointer we captured
        public MainWindow(PlaybackViewModel playbackVM)
        {
            InitializeComponent();
            PlaybackVM = playbackVM;
            PlaybackSlider.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(PlaybackSlider_PointerPressed), true);
            PlaybackSlider.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(PlaybackSlider_PointerReleased), true);
            PlaybackSlider.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(PlaybackSlider_PointerCanceled), true);
        }

        private void PlaybackSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Only left mouse button
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse &&
                e.GetCurrentPoint(PlaybackSlider).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
            {
                return;
            }

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                PlaybackVM.IsSeeking = true;

                _isUserDraggingPlaybackSlider = true;
                _capturedPointer = e.Pointer;

                PlaybackSlider.CapturePointer(e.Pointer);
            }
        }

        private void PlaybackSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            // Only process the pointer we actually captured
            if (_capturedPointer == null || e.Pointer.PointerId != _capturedPointer.PointerId)
                return;

            // For mouse – only left button
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse &&
                e.GetCurrentPoint(PlaybackSlider).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
            {
                return;
            }

            FinishDrag(e);
        }

        private void PlaybackSlider_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (_capturedPointer != null && e.Pointer.PointerId == _capturedPointer.PointerId)
            {
                FinishDrag(e);
            }
        }

        private void FinishDrag(PointerRoutedEventArgs e)
        {
            if (_capturedPointer != null)
            {
                PlaybackSlider.ReleasePointerCapture(_capturedPointer);
                _capturedPointer = null;
            }

            _isUserDraggingPlaybackSlider = false;

            PlaybackVM.Seek(PlaybackSlider.Value);

            PlaybackVM.IsSeeking = false;
        }

        private void PlaybackSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Live seek only while the user is actively dragging
            if (!_isUserDraggingPlaybackSlider)
                return;

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