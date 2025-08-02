using System;
using System.Threading.Tasks;

namespace ConsoleProgressTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
    }
}
