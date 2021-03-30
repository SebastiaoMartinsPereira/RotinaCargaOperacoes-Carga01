using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SalaSolutions.Cargas
{
    class Program
    {
        private static string _environmentName;
        private static ServiceProvider serviceProvider;
        private static ILogger<Program> logger;
        private static Spinner spinner;

        public static IConfigurationRoot Configuration { get; set; }

        static async Task Main(string[] args)
        {
            await MainAsync(args);
        }

        static async Task MainAsync(string[] args)
        {

            DefinirConfiguracoes();
            logger.LogInformation("############ Iniciando a aplicação ############ \n\n\n");

            try
            {
                await Task.Run(() => { new App(args, Configuration, serviceProvider.GetService<ILogger<App>>(), spinner).Run(); });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreram problemas durante a execução.\n" + ex.Message);
            }
        }
        /// <summary>
        /// Define configurações da aplicação
        /// </summary>
        private static void DefinirConfiguracoes()
        {

            _environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var serviceCollection = new ServiceCollection();
            SetAppSettings();
            ConfigureServices(serviceCollection, Configuration);

            serviceProvider = serviceCollection.BuildServiceProvider();
            logger = serviceProvider.GetService<ILogger<Program>>();
            spinner = serviceProvider.GetService<Spinner>();

            logger.LogInformation("\n\n\n");
            logger.LogInformation("############ Definindo configurações da aplicação ############\n\n");
            logger.LogDebug($"_environmentName : {_environmentName} \n");
        }

        private static void SetAppSettings()
        {
            Configuration =
               new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{_environmentName ?? "Production"}.json", optional: false)
               .Build();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions()
                .AddSingleton(new Spinner(Console.CursorLeft, Console.CursorTop, 100))
                .AddLogging(configure =>
                             configure
                            .ClearProviders()
                            .AddSerilog()
                            .AddConsole()
                );

            if (configuration["LOG_LEVEL"] == "true")
            {
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
            }
            else
            {
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Error);
            }


            Log.Logger = new LoggerConfiguration()
              .ReadFrom
              .Configuration(Configuration)
              .WriteTo.File(@Configuration.GetSection("GeneralConfig")["logFilePath"] + "\\" + Assembly.GetEntryAssembly().GetName().Name + DateTime.Now.ToString("_dd_MM_yyyy") + ".log")
              .WriteTo.Console()
              .CreateLogger();

        }

    }
}
