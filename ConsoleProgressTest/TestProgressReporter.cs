using AppProgress;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleProgressTest
{
    /// <summary>
    /// ProgressConsoleReporterとSynchronizedProgressLoggerのテスト用クラス
    /// </summary>
    public class TestProgressReporter
    {
        /// <summary>
        /// 基本的な進捗バーテスト
        /// </summary>
        public static async Task TestBasicProgress()
        {
            Console.WriteLine("=== 基本的な進捗バーテスト ===");
            
            using (var logger = new SynchronizedProgressLogger())
            using (var progress = new ProgressConsoleReporter(logger, 100))
            {
                for (int i = 0; i <= 100; i++)
                {
                    progress.Progress.Report(i);
                    await Task.Delay(30);
                }
            }
            
            Console.WriteLine("基本テスト完了");
        }

        /// <summary>
        /// 進捗バーとログの共存テスト
        /// </summary>
        public static async Task TestProgressWithLogs()
        {
            Console.WriteLine("\n=== 進捗バーとログの共存テスト ===");
            
            using (var logger = new SynchronizedProgressLogger())
            using (var progress = new ProgressConsoleReporter(logger, 200))
            {
                logger.Info("処理を開始します");
                
                for (int i = 0; i <= 200; i++)
                {
                    progress.Progress.Report(i);
                    
                    if (i == 50)
                    {
                        logger.Info("25%完了しました");
                    }
                    else if (i == 100)
                    {
                        logger.Warning("中間地点に到達。注意が必要です");
                    }
                    else if (i == 150)
                    {
                        logger.Error("軽微なエラーが発生しましたが続行します", new InvalidOperationException("テスト例外"));
                    }
                    else if (i == 180)
                    {
                        logger.Info("もうすぐ完了です");
                    }
                    
                    await Task.Delay(25);
                }
                
                logger.Info("すべての処理が完了しました");
            }
            Console.WriteLine("共存テスト完了");
        }

        /// <summary>
        /// 不定進捗テスト（MaxValue なし）
        /// </summary>
        public static async Task TestIndeterminateProgress()
        {
            Console.WriteLine("\n=== 不定進捗テスト ===");
            
            using (var logger = new SynchronizedProgressLogger())
            using (var progress = new ProgressConsoleReporter(logger)) // MaxValue なし
            {
                logger.Info("不定進捗処理を開始");
                
                for (int i = 0; i < 150; i++)
                {
                    progress.Progress.Report(i);
                    
                    if (i == 30)
                    {
                        logger.Warning("まだ処理中です...");
                    }
                    else if (i == 80)
                    {
                        logger.Info("処理を継続しています");
                    }
                    
                    await Task.Delay(20);
                }
                
                logger.Info("不定進捗処理完了");
            }
            Console.WriteLine("不定進捗テスト完了");
        }

        /// <summary>
        /// マルチスレッドテスト
        /// </summary>
        public static async Task TestMultiThreaded()
        {
            Console.WriteLine("\n=== マルチスレッドテスト ===");
            
            using (var logger = new SynchronizedProgressLogger())
            using (var progress = new ProgressConsoleReporter(logger, 1000))
            {
                logger.Info("マルチスレッド処理を開始");
                
                int completedCount = 0;
                var tasks = new Task[4];
                
                for (int threadId = 0; threadId < 4; threadId++)
                {
                    int id = threadId;
                    tasks[id] = Task.Run(async () =>
                    {
                        for (int i = 0; i < 250; i++)
                        {
                            // 作業をシミュレート
                            await Task.Delay(10);
                            
                            // アトミックにカウントを増加し、進捗を報告
                            int currentCount = Interlocked.Increment(ref completedCount);
                            progress.Progress.Report(currentCount);
                            
                            if (i % 50 == 0)
                            {
                                logger.Info($"スレッド {id}: {i}/250 完了");
                            }
                        }
                        
                        logger.Info($"スレッド {id} 完了");
                    });
                }
                
                await Task.WhenAll(tasks);
                logger.Info("すべてのスレッドが完了しました");
            }
            Console.WriteLine("マルチスレッドテスト完了");
        }

        /// <summary>
        /// 高速更新テスト（スロットリング確認）
        /// </summary>
        public static void TestHighFrequencyUpdates()
        {
            Console.WriteLine("\n=== 高速更新テスト ===");
            
            using (var logger = new SynchronizedProgressLogger())
            using (var progress = new ProgressConsoleReporter(logger, 10000))
            {
                logger.Info("高速更新テストを開始");
                
                for (int i = 0; i <= 10000; i++)
                {
                    progress.Progress.Report(i);
                    
                    if (i % 2000 == 0)
                    {
                        logger.Info($"高速更新: {i}/10000");
                    }
                    
                    // 高速更新（遅延なし）
                }
                
                logger.Info("高速更新テスト完了");
            }
            Console.WriteLine("高速更新テスト完了");
        }

        /// <summary>
        /// エラーハンドリングテスト
        /// </summary>
        public static async Task TestErrorHandling()
        {
            Console.WriteLine("\n=== エラーハンドリングテスト ===");
            
            using (var logger = new SynchronizedProgressLogger())
            {
                // 例外テスト
                try
                {
                    using (var progress = new ProgressConsoleReporter(null)) // null logger
                    {
                    }
                }
                catch (ArgumentNullException ex)
                {
                    logger.Error("期待される例外がキャッチされました", ex);
                }
                
                // 正常なテスト
                using (var progress2 = new ProgressConsoleReporter(logger, 50))
                {
                    for (int i = 0; i <= 50; i++)
                    {
                        progress2.Progress.Report(i);
                        
                        if (i == 25)
                        {
                            try
                            {
                                throw new InvalidOperationException("テスト用例外");
                            }
                            catch (Exception ex)
                            {
                                logger.Error("処理中にエラーが発生しました", ex);
                            }
                        }
                        
                        await Task.Delay(40);
                    }
                }
                
                logger.Info("エラーハンドリングテスト完了");
            }
            Console.WriteLine("エラーハンドリングテスト完了");
        }

        /// <summary>
        /// カスタムオプションテスト
        /// </summary>
        public static async Task TestCustomOptions()
        {
            Console.WriteLine("\n=== カスタムオプションテスト ===");
            
            using (var logger = new SynchronizedProgressLogger())
            {
                var options = new ProgressBarOptions
                {
                    // カスタムオプションがあれば設定
                };
                
                using (var progress = new ProgressConsoleReporter(logger, 75, options))
                {
                    logger.Info("カスタムオプションでテスト開始");
                    
                    for (int i = 0; i <= 75; i++)
                    {
                        progress.Progress.Report(i);
                        
                        if (i % 15 == 0)
                        {
                            logger.Info($"カスタム進捗: {i}/75");
                        }
                        
                        await Task.Delay(35);
                    }
                    
                    logger.Info("カスタムオプションテスト完了");
                }
            }
            Console.WriteLine("カスタムオプションテスト完了");
        }

        /// <summary>
        /// すべてのテストを実行
        /// </summary>
        public static async Task RunAllTests()
        {
            Console.WriteLine("ProgressConsoleReporter & SynchronizedProgressLogger テスト開始");
            Console.WriteLine("=" + new string('=', 60));
            
            try
            {
                await TestBasicProgress();
                await Task.Delay(1000);
                
                await TestProgressWithLogs();
                await Task.Delay(1000);
                
                await TestIndeterminateProgress();
                await Task.Delay(1000);
                
                await TestMultiThreaded();
                await Task.Delay(1000);
                
                TestHighFrequencyUpdates();
                await Task.Delay(1000);
                
                await TestErrorHandling();
                await Task.Delay(1000);
                
                await TestCustomOptions();
                
                Console.WriteLine("\n" + "=" + new string('=', 60));
                Console.WriteLine("すべてのテストが完了しました！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nテスト実行中にエラーが発生しました: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}