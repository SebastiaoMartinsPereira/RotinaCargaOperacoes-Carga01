using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Core
{

    [Flags]
    public enum TipoExtencao
    {
        /// <summary>Arquivos anexos a partir da tela de processos,na aba Agenda durante a inserção de uma descricação do agendamento </summary>
        [Description(".txt")]
        TXT = 0,
        [Description(".rem")]
        REM = 1,
        [Description(".doc")]
        DOC = 2,
        [Description(".docx")]
        DOCX = 3,
        [Description(".csv")]
        CSV = 4,
        [Description(".xls")]
        XLS = 5,
        [Description(".xlsx")]
        XLSX = 6,
        [Description(".xlsm")]
        XLSM = 7,
        [Description(".ret")]
        RET = 8,
        [Description("INVALIDO")]
        INVALIDO = 9999
    };

    public class GeradorArquivo
    {
        public static void ArrayListParaCsv(ArrayList dados, string caminhoParaSalvar)
        {
            StreamWriter arquivoCsv = CriarArquivo(caminhoParaSalvar);

            try
            {
                string linha = string.Empty;

                foreach (Dictionary<string, object> item in dados)
                {
                    foreach (KeyValuePair<string, object> _x in item)
                    {
                        linha = linha + _x.Key.ToString() + ";";
                    }

                    linha = linha.Substring(0, linha.Length - 1);
                    arquivoCsv.WriteLine(linha);
                    break;
                }

                linha = string.Empty;

                foreach (Dictionary<string, object> item in dados)
                {
                    foreach (KeyValuePair<string, object> _x in item)
                    {
                        linha = linha + _x.Value.ToString() + ";";
                    }

                    linha = linha.Substring(0, linha.Length - 1);
                    arquivoCsv.WriteLine(linha);
                    linha = string.Empty;
                }

                Console.WriteLine("\nArquivo {caminhoNome} gerado com sucesso !");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\n Erro na geração do arquivo Excel: {e.Message}");
            }
            finally
            {
                arquivoCsv.Close();
            }
        }

        /// <summary>
        /// VArre uma lista de objetos e monta um arquivo CSV com base nas suas propriedades
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dados"></param>
        /// <param name="caminhoParaSalvar"></param>
        /// <param name="separador"></param>
        public static void ListaParaCsv<T>(IEnumerable<T> dados, string caminhoParaSalvar, char separador = ';')
        {

            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            using (var writer = CriarArquivo(caminhoParaSalvar))
            {
                writer.WriteLine(string.Join($"{separador}", props.Select(p => p.Name.ToUpper())));

                foreach (var item in dados)
                {
                    writer.WriteLine(System.Text.RegularExpressions.Regex.Replace(string.Join($"{separador}", props.Select(p => p.GetValue(item, null))), @"\r\n?|\n", " "));
                }
            }
        }

        private static StreamWriter CriarArquivo(string caminhoParaSalvar)
        {
            StreamWriter arquivoCsv;
            string caminhoNome = caminhoParaSalvar;

            if (File.Exists(caminhoNome))
                File.Delete(caminhoNome);

            arquivoCsv = File.CreateText(caminhoNome);
            return arquivoCsv;
        }

        public static void ExcelToCsv(string excelFilePath, string csvOutputFile, TipoExtencao extensao, int worksheetNumber = 1)
        {
            if (!File.Exists(excelFilePath)) throw new FileNotFoundException(excelFilePath);
            if (File.Exists(csvOutputFile)) throw new ArgumentException("File exists: " + csvOutputFile);
            if (!Array.Exists(new TipoExtencao[] { TipoExtencao.XLSX, TipoExtencao.XLS, TipoExtencao.XLSM }, tipo => tipo == extensao)) throw new ArgumentException("tipo de arquivo inválido." + csvOutputFile);

            string cnnStr = string.Empty;
            // connection string

            if (extensao == TipoExtencao.XLS)
            {
                cnnStr = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=\"Excel 8.0;IMEX=1;HDR=YES\"", excelFilePath);
            }
            else
            {
                cnnStr = String.Format("Provider= Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=YES\"", excelFilePath);
            }

            var cnn = new OleDbConnection(cnnStr);

            // get schema, then data
            var dt = new DataTable();
            try
            {
                cnn.Open();
                var schemaTable = cnn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                if (schemaTable.Rows.Count < worksheetNumber) throw new ArgumentException("The worksheet number provided cannot be found in the spreadsheet");
                string worksheet = schemaTable.Rows[worksheetNumber - 1]["table_name"].ToString().Replace("'", "");
                string sql = String.Format("select * from [{0}]", worksheet);
                var da = new OleDbDataAdapter(sql, cnn);
                da.Fill(dt);
            }
            catch (Exception e)
            {
                if ("External table is not in the expected format.".Equals(e.Message) && TipoExtencao.XLS.Equals(extensao))
                {
                    ExcelToCsv(excelFilePath, csvOutputFile, TipoExtencao.XLSX, worksheetNumber = 1);
                    return;
                }

                throw e;
            }
            finally
            {
                // free resources
                cnn.Close();
            }

            try
            {
                // write out CSV data
                using (var wtr = new StreamWriter(csvOutputFile, false, Encoding.GetEncoding("iso-8859-1")))
                {

                    bool firstCol = true;
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (!firstCol) { wtr.Write(";"); } else { firstCol = false; }
                        var data = col.ColumnName.ToString().Replace("\"", "\"\"");
                        wtr.Write(String.Format("\"{0}\"", data));
                    }
                    wtr.WriteLine();

                    foreach (DataRow row in dt.Rows)
                    {
                        bool firstLine = true;
                        foreach (DataColumn col in dt.Columns)
                        {
                            if (!firstLine) { wtr.Write(";"); } else { firstLine = false; }
                            var data = row[col.ColumnName].ToString().Replace("\"", "\"\"");
                            wtr.Write(String.Format("\"{0}\"", data));
                        }
                        wtr.WriteLine();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
