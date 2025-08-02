using System;

namespace Interfaces
{
    /// <summary>
    /// ログ出力の色設定を定義するインターフェース
    /// </summary>
    public interface ILoggerColorScheme
    {
        ConsoleColor ErrorColor { get; }
        ConsoleColor WarningColor { get; }
        ConsoleColor InfoColor { get; }
        ConsoleColor ProgressColor { get; }
        ConsoleColor NormalColor { get; }
    }
}