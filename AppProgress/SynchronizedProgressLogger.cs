using Interfaces;
using System;

namespace AppProgress
{
    /// <summary>
    /// マルチスレッド対応のログ出力クラス
    /// </summary>
    public class SynchronizedProgressLogger : IConsoleWriter, ILogger, IDisposable
    {
        private readonly object _lock = new object(); // インスタンス毎のロック
        private static readonly string _clearString = new string(' ', 120); // 事前生成文字列
        private readonly int _defaultClearWidth;
        private readonly ILoggerColorScheme _colorScheme;
        private bool _needsClear = false;
        private bool _disposed;

        public SynchronizedProgressLogger(ILoggerColorScheme colorScheme = null)
        {
            _colorScheme = colorScheme ?? new DefaultLoggerColorScheme();
            
            // 初期化時に幅を取得（例外処理込み）
            try
            {
                _defaultClearWidth = Math.Max(0, Console.WindowWidth - 1);
            }
            catch
            {
                _defaultClearWidth = 79; // デフォルト値
            }
        }

        public void Write(string message)
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                // 進捗バー更新（\r で始まる）の場合、_needsClear フラグを設定
                if (message.StartsWith("\r"))
                {
                    _needsClear = true;
                    Console.ForegroundColor = _colorScheme.ProgressColor;
                }
                else
                {
                    Console.ForegroundColor = _colorScheme.NormalColor;
                }
                
                Console.Write(message);
                Console.ResetColor();
            }
        }

        public void WriteLine(string message = "")
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                // 空文字列の場合は進捗バー完了後の改行として扱い、クリアしない
                if (!string.IsNullOrEmpty(message))
                {
                    EnsureCleared();
                }
                Console.WriteLine(message);
                
                // 空文字列でWriteLineが呼ばれた場合、進捗バー状態をリセット
                if (string.IsNullOrEmpty(message))
                {
                    _needsClear = false;
                }
            }
        }

        private void EnsureCleared()
        {
            if (!_needsClear) return;

            // 進捗バーをクリア
            int width;
            try
            {
                width = Math.Min(Console.WindowWidth - 1, _clearString.Length);
            }
            catch
            {
                width = Math.Min(_defaultClearWidth, _clearString.Length);
            }

            Console.Write("\r");
            Console.Write(_clearString, 0, width);
            Console.Write("\r");
            
            _needsClear = false;
        }

        public void Error(string message, Exception exception = null)
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                EnsureCleared(); // 必要な場合のみクリア
                
                Console.ForegroundColor = _colorScheme.ErrorColor;
                Console.WriteLine($"ERROR: {message}");
                
                if (exception != null)
                {
                    Console.WriteLine($"Exception: {exception.Message}");
                    if (!string.IsNullOrEmpty(exception.StackTrace))
                    {
                        Console.WriteLine($"StackTrace: {exception.StackTrace}");
                    }
                }
                Console.ResetColor();
            }
        }

        public void Warning(string message)
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                EnsureCleared(); // 必要な場合のみクリア
                
                Console.ForegroundColor = _colorScheme.WarningColor;
                Console.WriteLine($"WARNING: {message}");
                Console.ResetColor();
            }
        }

        public void Info(string message)
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                EnsureCleared(); // 必要な場合のみクリア
                
                Console.ForegroundColor = _colorScheme.InfoColor;
                Console.WriteLine($"INFO: {message}");
                Console.ResetColor();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                Console.ResetColor(); // 確実に色をリセット
                _disposed = true;
            }
        }
    }
}