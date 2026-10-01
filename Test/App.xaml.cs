using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using Test.Database;
using Test.Repo;
using Test.Schema;
using Test.Service;
using Test.Utils;
using Test.Utils.File;
using Test.Utils.Helper;
using Test.ViewModel;
using WinRT.PlayerVtableClasses;


namespace Test
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider Services { get; }
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            ConfigureServices(services);

            Services = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<ISqliteConnectionFactory>(_ => new SqliteConnectionFactory(@"C:/Users/evges/source/repos/Test/Test/playlist.db"));
            services.AddSingleton<ITrackRepository<Track, TrackRecord>, TrackRepository>();

            services.AddSingleton<FilePicker>();

            services.AddSingleton<PlaybackService>();

            services.AddSingleton<FileLoader>();
            services.AddSingleton<DatabaseHealthChecker>();
            services.AddSingleton<PlaybackQueue>();

            services.AddSingleton<MediaLibraryService>();

            services.AddTransient<PlaybackViewModel>();

            services.AddSingleton<MainWindow>();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = Services.GetRequiredService<MainWindow>();
            FilePicker picker = Services.GetRequiredService<FilePicker>();
            picker.SetWindow(_window);
            _window.Activate();
        }
    }
}
