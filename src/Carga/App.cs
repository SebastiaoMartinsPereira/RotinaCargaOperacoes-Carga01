
using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace SalaSolutions.Carga
{
    public class App
    {
        public IConfiguration Configuration { get; }
        private readonly ILogger _logger;
        private readonly Spinner _spinner;
        private readonly string[] _args;

        public App(string[] args, IConfiguration configuration, ILogger<App> logger, Spinner spinner)
        {
            Configuration = configuration;
            _logger = logger;
            _spinner = spinner;
            _args = args;
        }

        /// <summary>
        /// Start Application 
        /// </summary>
        public void Run()
        {
            try
            {
                if (_args.ToArray().Contains("-h")) Help();
                else if (_args.Length == 0 || _args.ToArray().Contains("-h") && _args.Length == 1)
                {
                    Help();
                    Console.ReadKey();
                }

                ////Processar Carga de acordo com o modelo
                if (_args[0] == typeof(ModelosCargas.CargaSala01).Name) new ModelosCargas.CargaSala01(this.Configuration, _logger, _spinner).ProcessarCarga();
                //if (_args[0] == typeof(SimularOcorrenciasOmni.SimularOcorrencias).Name) new SimularOcorrenciasOmni.SimularOcorrencias(this.Configuration, _logger, _spinner).ProcessarEnvio(_args.ToArray().Contains("-f"));


            }
            catch (Exception ex)
            {
                _logger.LogDebug($"\n Ocorreu um erro: {ex.Message} Data:{DateTime.Now.Date} Hora:{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}");
            }
            finally
            {
                _spinner.Stop();
                _spinner.Dispose();
            }
        }

        private void Help()
        {
            _logger.LogInformation("Definir a aplicação a ser executada via linha de comando\n\n");
            _logger.LogInformation("-- ---- ----                 Opções disponíveis            ---- ---- --");

            //_logger.LogInformation($"{typeof(EnvioOcorrenciasOmni.EnvioOcorrencias).Name} [-a]");
            //_logger.LogInformation($"Descrição \n Nome da Aplicação : {typeof(EnvioOcorrenciasOmni.EnvioOcorrencias).Name} \n Parâmetros: \n [-a] Opcional, informa se a data deve ser definida automaticamente como a data do dia atual.");

            //_logger.LogInformation($"{typeof(SimularOcorrenciasOmni.SimularOcorrencias).Name} [-f]");
            //_logger.LogInformation($"Descrição \n Nome da Aplicação : {typeof(SimularOcorrenciasOmni.SimularOcorrencias).Name} \n [-f] Opcional, forçar a consulta na base de dados (deconsidera a regra de 3 dias de intervalo do último envio).");
        }
    }
}
