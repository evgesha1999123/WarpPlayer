using Microsoft.Extensions.DependencyInjection;
using Player.Database;
using Player.Repo;
using Player.Schema;
using Player.Service;
using Player.Settings;
using Player.Utils;
using Player.Utils.File;
using Player.Utils.Helper;
using Player.ViewModel;
using System;
using static Player.Settings.ResourceManifest;


namespace Player.DependencyInjection
{
    public static class ApplicationServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<ApplicationSettings>();
            services.AddSingleton<DatabaseResources>(
                provider =>
                {
                    var settings = provider.GetRequiredService<ApplicationSettings>();
                    return settings.GetDatabase();
                });

            services.AddSingleton<ImageResources>(
                provider =>
                {
                    var settings = provider.GetRequiredService<ApplicationSettings>();
                    var images = settings.GetImages();
                    return settings.GetImages();
                });

            services.AddSingleton<ISqliteConnectionFactory>(provider =>
            {
                var settings = provider.GetRequiredService<ApplicationSettings>();
                return new SqliteConnectionFactory(settings.GetDatabase().Settings["Source"]);
            });

            services.AddSingleton<ITrackRepository<Track, TrackRecord>, TrackRepository>();
            services.AddSingleton<IPlaylistRepository<Playlist, PlaylistRecord>, PlaylistRepository>();

            services.AddSingleton<FilePicker>();

            services.AddSingleton<PlaybackService>();

            services.AddSingleton<FileLoader>();
            services.AddSingleton<DatabaseHealthChecker>();
            services.AddSingleton<PlaybackQueue>();

            services.AddSingleton<MediaLibraryService>();

            services.AddTransient<PlaybackViewModel>();

            services.AddSingleton<MainWindow>();

            return services;
        }
    }
}
