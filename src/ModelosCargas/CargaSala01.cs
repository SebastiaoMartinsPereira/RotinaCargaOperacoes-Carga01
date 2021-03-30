using Core;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace ModelosCargas
{
    public class CargaSala01
    {
        public IConfiguration Configuration { get; }
        public ILogger _logger { get; }
        public Spinner _spinner { get; }

        private readonly ConnDbDapper _conn;

        private readonly TipoExtencao[] ExtensoesPossiveis = new TipoExtencao[] { TipoExtencao.CSV, TipoExtencao.XLS, TipoExtencao.XLSX, TipoExtencao.XLSM };
        private readonly string PathOrigin;
        private readonly string PathProcessing;
        private readonly string PathProcessed;
        private readonly string PathBdProcessing;
        private readonly string PathBdProcessed;

        public CargaSala01(IConfiguration configuration, ILogger logger, Spinner spinner)
        {
            Configuration = configuration;
            _logger = logger;
            _spinner = spinner;
            PathOrigin = Configuration.GetSection("GeneralConfig").GetSection("PastaArquivoOrigem").Value;
            PathProcessing = Configuration.GetSection("GeneralConfig").GetSection("PastaArquivoProcessando").Value;
            PathProcessed = Configuration.GetSection("GeneralConfig").GetSection("PastaArquivoProcessado").Value;
            PathBdProcessing = Configuration.GetSection("GeneralConfig").GetSection("PastaProcessamentoBancoDeDados").Value;
            PathBdProcessed = Configuration.GetSection("GeneralConfig").GetSection("PastaProcessadosBancoDeDados").Value;


            _conn = new ConnDbDapper(Configuration);
        }

        public bool ContemArquivosAProcessar()
        {
            if (!System.IO.Directory.Exists(PathOrigin))
            {
                _logger.LogError($"Diretório base não identificado em \"{PathOrigin}\"");
            }
            return HasFiles(PathOrigin);
        }

        public bool ContemArquivosProcessando()
        {
            if (!System.IO.Directory.Exists(PathProcessing))
            {
                _logger.LogError($"Diretório base não identificado em \"{PathProcessing}\"");
            }
            return HasFiles(PathProcessing);
        }


        private bool HasFiles(string path)
        {
            var hasFiles = new System.IO.DirectoryInfo(path);
            return hasFiles.GetFiles().Length > 0;
        }

        public void ProcessarCarga()
        {
            if (!ContemArquivosAProcessar())
            {
                _logger.LogInformation("Sem arquivos para serem processados!");
                return;
            }
            if (ContemArquivosProcessando())
            {
                _logger.LogInformation("Existem arquivos sendo processados, fila atualizada!");
                return;
            }

            FileInfo primeiroArquivoNaFila = ObterProximoArquivo();
            if (primeiroArquivoNaFila is null) return;

            if (!ExtensaoValida(primeiroArquivoNaFila)) return;

            var nomeArquivoProcessando = DateTime.Now.ToString("ddMMyyyy_HHmmssffff");
            if (EUmCSV(primeiroArquivoNaFila))
            {
                nomeArquivoProcessando = $"{nomeArquivoProcessando}_{primeiroArquivoNaFila.Name}";
                File.Copy($"{PathOrigin}{primeiroArquivoNaFila.Name}", $"{PathProcessing}{nomeArquivoProcessando}");
            }
            else
            {
                nomeArquivoProcessando = $"{nomeArquivoProcessando}_{(primeiroArquivoNaFila.Name.Replace(primeiroArquivoNaFila.Extension, ".csv"))}";
                GeradorArquivo.ExcelToCsv(primeiroArquivoNaFila.FullName, $"{PathProcessing}{nomeArquivoProcessando}", ObterTipoExtensaoArquivo(primeiroArquivoNaFila));
            }

            try
            {
                _conn.GetConn().
                    Query("select * from public.fn_carga_sala01(pasta_arquivo:=@_pasta_arquivo,nome_arquivo:=@_nome_arquivo,pasta_arquivo_processado:=@_pasta_arquivo_processado)"
                    , new
                    {
                        _pasta_arquivo = PathBdProcessing,
                        _nome_arquivo = nomeArquivoProcessando,
                        _pasta_arquivo_processado = PathBdProcessed
                    });

                File.Copy(primeiroArquivoNaFila.FullName, $"{PathProcessed}{primeiroArquivoNaFila.Name}", true);
                File.Delete(primeiroArquivoNaFila.FullName);

                var pathProcessing = new System.IO.DirectoryInfo(PathProcessing);
                pathProcessing.GetFiles().All(f =>
                {
                    f.Delete();
                    return true;
                });

            }
            finally
            {
                _conn.CloseConn();
            }
        }

        private static bool EUmCSV(FileInfo primeiroArquivoNaFila)
        {
            return ObterExtensaoArquivo(primeiroArquivoNaFila).Equals(TipoExtencao.CSV.ToString("G").ToUpper());
        }

        private bool ExtensaoValida(FileInfo primeiroArquivoNaFila)
        {
            try
            {
                if (Enum.TryParse(ObterExtensaoArquivo(primeiroArquivoNaFila), out Core.TipoExtencao extensao))
                {
                    return ExtensoesPossiveis.Contains(extensao);
                }

                _logger.LogError("Extensão de arquivo utilizado é inválido.");
                return false;
            }
            catch (Exception)
            {
                _logger.LogError("Extensão de arquivo utilizado é inválido.");
                return false;
            }
        }

        private static string ObterExtensaoArquivo(FileInfo fileInfo)
        {
            return fileInfo.Extension.Replace(".", "").ToUpper();
        }

        private static Core.TipoExtencao ObterTipoExtensaoArquivo(FileInfo fileInfo)
        {
            if (Enum.TryParse(ObterExtensaoArquivo(fileInfo), out Core.TipoExtencao extensao))
            {
                return extensao;
            }
            return TipoExtencao.INVALIDO;
        }

        private FileInfo ObterProximoArquivo()
        {
            FileInfo ret = null;
            try
            {
                ret = new System.IO.DirectoryInfo(PathOrigin).GetFiles().OrderBy(f => f.Name).First();
                if (ret is null) _logger.LogError("Não foi possível obter o arquivo para processamento.");
            }
            catch (Exception)
            {
                _logger.LogError("Não foi possível obter o arquivo para processamento.");
            }

            return ret;
        }
    }
}
