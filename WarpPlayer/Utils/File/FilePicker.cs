using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;


namespace Player.Utils.File
{
    public class FilePicker
    {
        public record class FilePickerResponse
        {
            public bool HasContent => Files.Count > 0;
            public IReadOnlyList<string> Files { get; init; } = [];

            public FilePickerResponse(IReadOnlyList<string> files)
            {
                Files = files;
            }
        }
        public static readonly string[] SupportedFileTypes = [".mp3"];

        private Window? _window;

        public FilePicker()
        {
            _window = null;
        }

        public async Task<FilePickerResponse> PickFolderAsync()
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            ConfigurePicker(picker);

            var folder = await picker.PickSingleFolderAsync();
            var files = await GetStorageFilesAsync(folder);

            List<string> result = [];

            foreach (var file in files)
            {
                if (SupportedFileTypes.Contains(Path.GetExtension(file.Name)))
                {
                    result.Add(file.Path);
                }
            }

            return new FilePickerResponse(result);
        }

        private void ConfigurePicker(dynamic? picker)
        {
            InitLocationWithFilter(picker);
            InitHwnd(picker);
        }

        private static void InitLocationWithFilter(dynamic? picker)
        {
            if (picker == null)
            {
                return;
            }

            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;

            foreach (var filter in SupportedFileTypes)
            {
                picker.FileTypeFilter.Add(filter);
            }
        }

        private void InitHwnd(dynamic? picker)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        }

        private static async Task<IReadOnlyList<StorageFile>> GetStorageFilesAsync(StorageFolder? folder)
        {
            if (folder is null)
            {
                return [];
            }

            return await folder.GetFilesAsync();
        }

        public async Task<FilePickerResponse> PickFilesAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            ConfigurePicker(picker);

            var files = new List<string>();
            var fileObjs = await picker.PickMultipleFilesAsync();
            
            foreach(var file in fileObjs)
            {
                files.Add(file.Path);
            }

            return new FilePickerResponse(files);
        }

        public void SetWindow(Window window)
        {
            _window = window;
        }
    }
}
