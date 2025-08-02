using AppProgress;
using System;
using System.Threading.Tasks;

namespace ConsoleProgressTest
{
    /// <summary>
    /// ProgressConsoleReporter と SynchronizedProgressLogger のテスト実行クラス
    /// </summary>
    public class ProgressTestRunner
    {
        /// <summary>
        /// テストプログラムのメインエントリーポイント
        /// </summary>
        public static async Task RunTests()
        {
            Console.WriteLine("ProgressConsoleReporter テストプログラム");
            Console.WriteLine("どのテストを実行しますか？");
            Console.WriteLine();
            Console.WriteLine("1. すべてのテストを実行");
            Console.WriteLine("2. 基本的な進捗バーテスト");
            Console.WriteLine("3. 進捗バーとログの共存テスト");
            Console.WriteLine("4. 不定進捗テスト");
            Console.WriteLine("5. マルチスレッドテスト");
            Console.WriteLine("6. 高速更新テスト");
            Console.WriteLine("7. エラーハンドリングテスト");
            Console.WriteLine("8. カスタムオプションテスト");
            Console.WriteLine("0. 終了");
            Console.WriteLine();
            Console.Write("選択してください (0-8): ");

            var input = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (input)
                {
                    case "1":
                        await TestProgressReporter.RunAllTests();
                        break;
                    case "2":
                        await TestProgressReporter.TestBasicProgress();
                        break;
                    case "3":
                        await TestProgressReporter.TestProgressWithLogs();
                        break;
                    case "4":
                        await TestProgressReporter.TestIndeterminateProgress();
                        break;
                    case "5":
                        await TestProgressReporter.TestMultiThreaded();
                        break;
                    case "6":
                        TestProgressReporter.TestHighFrequencyUpdates();
                        break;
                    case "7":
                        await TestProgressReporter.TestErrorHandling();
                        break;
                    case "8":
                        await TestProgressReporter.TestCustomOptions();
                        break;
                    case "0":
                        Console.WriteLine("テストを終了します。");
                        return;
                    default:
                        Console.WriteLine("無効な選択です。");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"テスト実行中にエラーが発生しました: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            Console.WriteLine("\nEnterキーを押して終了してください...");
            Console.ReadLine();
        }
    }
}