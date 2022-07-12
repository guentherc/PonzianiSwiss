using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvvmDialogs;
using PonzianiSwissLib;
using Serilog;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace PonzianiSwiss
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider? Services { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
#if DEBUG
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.debug.json", optional: false, reloadOnChange: true);
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
#endif
            IConfiguration Configuration = builder.Build();
            var serilogLogger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddSerilog(serilogLogger);
            var logger = loggerFactory.CreateLogger("PonzianiSwiss");
            logger.LogInformation("PonzianiSwiss started with {}", string.Join(' ', e.Args));
            var section = Configuration.GetSection(nameof(AppSettings));
            AppSettings appSettings = new()
            {
                Mode = (Mode)section.GetValue(typeof(Mode), nameof(Mode), Mode.Release)
            };
            //Adjust settings from start parameters
            int indx;
            if (e.Args.Length > 0 && File.Exists(e.Args[0]))
                appSettings.Filename = e.Args[0];
            for (indx = 0; indx < e.Args.Length - 1; ++indx)
            {
                if (e.Args[indx] == "-mode")
                {
                    if (Enum.TryParse<Mode>(e.Args[indx + 1], out Mode m))
                        appSettings.Mode = m;
                }
            }
            services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(logger);
            services.AddSingleton<AppSettings>(appSettings);
            services.AddTransient<PlayerSearchDialogViewModel>();
            services.AddTransient<ParticipantDialogViewModel>();
            services.AddTransient<TiebreakDialogViewModel>();
            services.AddTransient<TournamentDialogViewModel>();
            services.AddSingleton<IDialogService, DialogService>();
            Services = services.BuildServiceProvider();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            PairingTool.Initialize(logger);
            MainWindow wnd = new();
            wnd.Show();
        }

        public enum Mode { Release, Test }
    }

    public class AppSettings
    {
        public App.Mode Mode { get; set; }

        public string? Filename { get; set; }
    }


}
