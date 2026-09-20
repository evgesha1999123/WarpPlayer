using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;


namespace Test.Utils
{
    public class FolderPicker
    {
        private Window? _window;

        public FolderPicker()
        {
            _window = null;
        }

        public async Task<string?> PickFolderAsync()
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;

            picker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);

            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();

            return folder?.Path;
        }

        public void SetWindow(Window window)
        {
            _window = window;
        }
    }
}
