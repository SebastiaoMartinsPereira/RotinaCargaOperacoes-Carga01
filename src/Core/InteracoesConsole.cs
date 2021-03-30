using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class InteracoesConsole
    {

        /// <summary>
        /// Solicitar que o usuário informe uma data
        /// </summary>
        /// <param name="mensagem"></param>
        /// <returns></returns>
        public static DateTime SoliticarData(string mensagem)
        {
            Console.WriteLine(mensagem);
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
            var strdataInicial = Console.ReadLine();
            if (!DateTime.TryParse(strdataInicial, out DateTime data))
            {
                return DateTime.Now;
            };
            return data;
        }
    }
}
