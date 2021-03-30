using Microsoft.Extensions.Logging;
using System;

namespace Core
{

    public static class SpinnerExtension
    {
        public static void SetCursorSpinner(this Spinner spinner, ILogger _logger, string msgLogger)
        {
            _logger.LogInformation(msgLogger);
            spinner.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
        }

        public static void SetCursorPosition(this Spinner spinner)
        {
            spinner.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);
        }
    }

}
