using AppProgress;
using Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleProgressTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var totalCount = 15000;

            var logger = new SynchronizedProgressLogger();

            Console.WriteLine("処理開始");
            using (var prog = new ProgressReporter(totalCount, logger: logger))
            {
                LongTask(totalCount, prog, logger);
            }
            Console.WriteLine("処理終了");
        }

        private static void LongTask(int totalCount, IProgress<int> progress = null, ILogger logger = null)
        {
            var threadNum = 10;
            var batchNum = totalCount / threadNum;
            var errorBorder = 50;
            var count = 0;

            Parallel.For(0, threadNum, i =>
            {
                for (int j = 0; j < batchNum; j++)
                {
                    int current = Interlocked.Increment(ref count);
                    if (current % errorBorder == 0)
                    {
                        logger?.Error($"スレッド{i}: エラー発生1");

                    }
                    else if (current % (errorBorder * 1.5) == 0)
                    {
                        logger?.Error($"スレッド{i}: エラー発生2");
                    }

                    progress?.Report(current);
                    Thread.Sleep(10);
                }
            });

            //for (int j = 0; j <= totalCount; j += 1)
            //{
            //    prog.Report(j);
            //    Thread.Sleep(10);

            //    if ((j + 1) % 50 == 0)
            //    {
            //        logger.Error($"スレッド: エラー発生");
            //    }
            //}
        }
    }
}
