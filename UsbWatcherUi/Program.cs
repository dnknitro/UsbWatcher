using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UsbWatcherUi.Utils;

namespace UsbWatcherUi
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(config)
                .AddSingleton<StringAppendLogger>()
                .AddTransient<ILogger>(sp => sp.GetRequiredService<StringAppendLogger>())
                .AddSingleton(sp => config.GetRequiredSection("DisplayConfig").Get<DisplayConfig>()!)
                .AddSingleton(sp => config.GetRequiredSection("UsbDeviceConfig").Get<UsbDeviceConfig>()!)
                .AddTransient<DisplayUtils>()
                .AddTransient<UsbDeviceUtils>()
                .AddTransient<AppForm>()
                .BuildServiceProvider();
            ;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            AppForm appForm = serviceProvider.GetRequiredService<AppForm>();
            Application.Run();
        }
    }
}