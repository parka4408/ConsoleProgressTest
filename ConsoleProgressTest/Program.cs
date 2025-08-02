using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleProgressTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // コマンドライン引数をチェック
            if (args.Contains("--help") || args.Contains("-h"))
            {
                ShowHelp();
                return;
            }
            else if (args.Contains("--license") || args.Contains("-l"))
            {
                ShowLicenseInfo();
                return;
            }

            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("ConsoleProgressTest プログラム");
            Console.WriteLine("どちらを実行しますか？");
            Console.WriteLine();
            Console.WriteLine("1. ProgressConsoleReporter テストプログラム");
            Console.WriteLine("0. 終了");
            Console.WriteLine();
            Console.Write("選択してください (0-2): ");

            var input = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (input)
                {
                    case "1":
                        await ProgressTestRunner.RunTests();
                        break;
                    case "0":
                        Console.WriteLine("プログラムを終了します。");
                        return;
                    default:
                        Console.WriteLine("無効な選択です。");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"実行中にエラーが発生しました: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("ConsoleProgressTest - プログレスバー機能のテストプログラム");
            Console.WriteLine();
            Console.WriteLine("使用方法:");
            Console.WriteLine("  ConsoleProgressTest.exe [オプション]");
            Console.WriteLine();
            Console.WriteLine("オプション:");
            Console.WriteLine("  -h, --help      このヘルプメッセージを表示");
            Console.WriteLine("  -l, --license   プロジェクト依存関係のライセンス情報を表示");
            Console.WriteLine();
            Console.WriteLine("引数なしで実行した場合は、対話モードで動作します。");
        }

        static void ShowLicenseInfo()
        {
            string licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.md");

            if (!File.Exists(licensePath))
            {
                Console.WriteLine("ライセンス情報が見つかりません。");
                Console.WriteLine("ビルドを実行してlicense.mdを生成してください。");
                return;
            }

            try
            {
                string licenseContent = File.ReadAllText(licensePath);
                Console.WriteLine("=== プロジェクト依存関係ライセンス情報 ===");
                Console.WriteLine();
                Console.WriteLine(licenseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ライセンス情報の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }
    }
}
