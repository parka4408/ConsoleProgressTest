using Interfaces;
using System;

namespace AppProgress
{
    /// <summary>
    /// デフォルトのログ色設定
    /// </summary>
    public class DefaultLoggerColorScheme : ILoggerColorScheme
    {
        public ConsoleColor ErrorColor => ConsoleColor.Red;
        public ConsoleColor WarningColor => ConsoleColor.Yellow;
        public ConsoleColor InfoColor => ConsoleColor.Cyan;
        public ConsoleColor ProgressColor => ConsoleColor.DarkGray;
        public ConsoleColor NormalColor => ConsoleColor.Gray;
    }
}