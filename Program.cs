using Microsoft.Extensions.DependencyInjection;
using wmine.Services;

namespace wmine
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Build DI container
            var services = new ServiceCollection();
            services.AddSingleton<IMineralRepository, MineralRepository>();
            services.AddSingleton<PhotoService>();
            services.AddSingleton<FilonDataService>();

            var provider = services.BuildServiceProvider();
            Services.AppServiceProvider.SetProvider(provider);

            // Resolve required services for Form1 and pass them via constructor
            var filonDataService = provider.GetService<FilonDataService>() ?? new FilonDataService();
            var mineralRepo = provider.GetService<IMineralRepository>() ?? new MineralRepository();
            var photoService = provider.GetService<PhotoService>() ?? new PhotoService();

            Application.Run(new Form1(filonDataService, mineralRepo, photoService));
        }

        /// <summary>
        /// Lance le formulaire de test du service de routing
        /// </summary>
        private static void TestRouteService()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Forms.RouteTestForm());
        }

        /// <summary>
        /// Lance les tests console du service de routing
        /// </summary>
        private static async Task TestRouteServiceConsole()
        {
            await Tests.RouteServiceTests.RunAllTests();
        }
    }
}
