using Interfaces;
using System;

namespace AppProgress
{
    /// <summary>
    /// マルチスレッド対応のログ出力クラス
    /// </summary>
    public class SynchronizedProgressLogger : IConsoleWriter, ILogger
    {
        private static readonly object _lock = new object();

        public void Write(string message)
        {
            lock (_lock)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(message);
                Console.ResetColor();
            }
        }

        public void WriteLine(string message = "")
        {
            lock (_lock)
            {
                Console.WriteLine(message);
            }
        }

        public void Error(string message, Exception exception = null, bool clearLine = true)
        {
            lock (_lock)
            {
                if (clearLine)
                {
                    ClearLine();
                }
                Console.WriteLine(message);
            }
        }

        public void Warning(string message, bool clearLine = true)
        {
            lock (_lock)
            {
                if (clearLine)
                {
                    ClearLine();
                }
                Console.WriteLine(message);
            }
        }

        private void ClearLine()
        {
            var width = Math.Max(0, Console.WindowWidth - 1);
            Console.Write("\r" + new string(' ', width) + "\r");
        }
    }
}