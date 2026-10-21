using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using Player.Utils.File;
using Player.DependencyInjection;


namespace Player
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }
        private Window? _window;

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            ApplicationServices.AddApplicationServices(services);
            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = Services.GetRequiredService<MainWindow>();
            FilePicker picker = Services.GetRequiredService<FilePicker>();
            picker.SetWindow(_window);
            _window.Activate();
        }
    }
}
