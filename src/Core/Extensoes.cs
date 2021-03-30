using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public static class Extensoes
    {
        public static DateTime TryGetDate(this string dateString)
        {
            DateTime.TryParse(dateString, out DateTime data);
            return data;
        }

        public static void InvertarDatas(ref DateTime dataInicio, ref DateTime dataFim)
        {
            if (dataFim < dataInicio)
            {
                var aux = dataInicio;
                dataInicio = dataFim;
                dataFim = aux;
            }
        }

        public static void InvertarDatas(this DateTime dataInicio, ref DateTime dataFim)
        {
            if (dataFim < dataInicio)
            {
                var aux = dataInicio;
                dataInicio = dataFim;
                dataFim = aux;
            }
        }

        public static int ToInt(this string valor)
        {
            int.TryParse(valor, out int Resultado);
            return Resultado;
        }

    }
}
