using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Test.Utils
{
    public class FilePicker
    {
        private Window? _window;

        public FilePicker()
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

        public async Task<IReadOnlyList<string>> PickFilesAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;

            picker.FileTypeFilter.Add(".mp3");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var files = new List<string>();
            var fileObjs = await picker.PickMultipleFilesAsync();
            
            foreach(var file in fileObjs)
            {
                files.Add(file.Path);
            }

            return files;
        }

        public void SetWindow(Window window)
        {
            _window = window;
        }
    }
}
