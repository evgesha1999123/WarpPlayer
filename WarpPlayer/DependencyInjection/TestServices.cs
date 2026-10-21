using Microsoft.Extensions.DependencyInjection;
using Player.Database;
using Player.Settings;
using Player.Utils.Helper;
using System;
using static Player.Settings.ResourceManifest;


namespace Player.DependencyInjection
{
    public static class TestServices
    {
        public static IServiceCollection AddTestServices(this IServiceCollection services, string databasePath)
        {
            services.AddSingleton<ApplicationSettings>();

            services.AddSingleton<DatabaseResources>(
                provider =>
                {
                    var settings = provider.GetRequiredService<ApplicationSettings>();
                    return settings.GetDatabase();
                });

            services.AddSingleton<ISqliteConnectionFactory>(provider =>
            {
                var settings = provider.GetRequiredService<ApplicationSettings>();
                return new SqliteConnectionFactory(databasePath);
            });

            services.AddSingleton<DatabaseHealthChecker>();

            return services;
        }

        public static ServiceProvider Build(string databasePath)
        {
            return new ServiceCollection().AddTestServices(databasePath).BuildServiceProvider();
        }
    }
}
