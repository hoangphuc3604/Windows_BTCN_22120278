using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows_22120278.Services;
using Windows_22120278.ViewModels;
using Windows_22120278_Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Windows_22120278
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Services = ConfigureServices();
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
            {
                string folderPath = ApplicationData.Current.LocalFolder.Path;
                string dbPath = System.IO.Path.Combine(folderPath, "paint.db");
                options.UseSqlite($"Data Source={dbPath}", 
                    b => b.MigrationsAssembly("Windows_22120278_Data"));
            });

            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IDrawingService, DrawingService>();
            services.AddTransient<ITemplateService, TemplateService>();
            services.AddTransient<INavigationService, NavigationService>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<DrawingViewModel>();
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Ensure database is created and migrated
            using (var scope = Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await context.Database.MigrateAsync();
            }

            _window = Services.GetRequiredService<MainWindow>();
            _window.Activate();
        }
    }
}
