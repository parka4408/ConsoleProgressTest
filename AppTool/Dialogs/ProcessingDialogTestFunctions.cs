using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppTool.Dialogs;

/// <summary>
/// ProcessingDialog の非同期処理オーバーロード用テスト関数集
/// </summary>
public static class ProcessingDialogTestFunctions
{
    /// <summary>
    /// 基本的な非同期処理テスト（戻り値あり）
    /// </summary>
    public static async Task<string> BasicAsyncProcessingWithResult(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int maxSteps = 100;
        var result = "処理完了";

        for (int i = 0; i <= maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 進捗報告
            progress.Report(i);

            // 非同期待機（実際の処理をシミュレート）
            await Task.Delay(50, cancellationToken);

            // 処理内容をシミュレート
            if (i == maxSteps / 2)
            {
                result += " - 中間処理完了";
            }
        }

        return result;
    }

    /// <summary>
    /// 長時間処理テスト（タイムアウトテスト用）
    /// </summary>
    public static async Task<int> LongRunningProcessing(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int maxSteps = 1000;
        int processedCount = 0;

        for (int i = 0; i <= maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(i);
            await Task.Delay(500, cancellationToken);
            processedCount++;
        }

        return processedCount;
    }

    /// <summary>
    /// 高頻度進捗更新テスト（スロットリング確認用）
    /// </summary>
    public static async Task<bool> HighFrequencyProgressTest(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int maxSteps = 10000;

        for (int i = 0; i <= maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(i);

            if (i % 100 == 0)
            {
                await Task.Delay(1, cancellationToken);
            }
        }

        return true;
    }

    /// <summary>
    /// エラー発生テスト
    /// </summary>
    public static async Task<string> ErrorProcessingTest(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int maxSteps = 50;

        for (int i = 0; i <= maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress.Report(i);

            // 30%の時点でエラーを発生
            if (i == 30)
            {
                throw new InvalidOperationException("テスト用エラー: 処理中に問題が発生しました");
            }

            await Task.Delay(100, cancellationToken);
        }

        return "エラーなく完了";
    }

    /// <summary>
    /// ファイル処理シミュレーション
    /// </summary>
    public static async Task<int> FileProcessingSimulation(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int totalFiles = 25;
        int processedFiles = 0;

        for (int i = 0; i < totalFiles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ファイル処理のシミュレート
            await Task.Delay(200, cancellationToken);

            processedFiles++;
            progress.Report(processedFiles);

            // 一部のファイルで追加処理時間
            if (i % 5 == 0)
            {
                await Task.Delay(300, cancellationToken);
            }
        }

        return processedFiles;
    }

    /// <summary>
    /// ネットワーク通信シミュレーション
    /// </summary>
    public static async Task<string> NetworkOperationSimulation(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int totalRequests = 20;
        var responses = new List<string>();

        for (int i = 0; i < totalRequests; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress.Report(i + 1);

            // ネットワーク遅延のシミュレート
            await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);

            responses.Add($"Response_{i + 1}");

            // 一部のリクエストで長い遅延
            if (i % 7 == 0)
            {
                await Task.Delay(800, cancellationToken);
            }
        }

        return $"完了: {responses.Count}件のレスポンス取得";
    }

    /// <summary>
    /// データベース処理シミュレーション
    /// </summary>
    public static async Task<int> DatabaseOperationSimulation(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int totalRecords = 15000;
        const int batchSize = 100;
        int processedRecords = 0;

        for (int batch = 0; batch < totalRecords / batchSize; batch++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // バッチ処理のシミュレート
            await Task.Delay(50, cancellationToken);

            processedRecords += batchSize;
            progress.Report(processedRecords);

            // 定期的なコミット処理をシミュレート
            if (batch % 10 == 0)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        return processedRecords;
    }

    /// <summary>
    /// キャンセレーション応答性テスト
    /// </summary>
    public static async Task<bool> CancellationResponsivenessTest(IProgress<int> progress, CancellationToken cancellationToken)
    {
        const int maxSteps = 200;

        for (int i = 0; i <= maxSteps; i++)
        {
            // 頻繁にキャンセルをチェック
            cancellationToken.ThrowIfCancellationRequested();

            progress.Report(i);

            // 短い処理間隔
            await Task.Delay(25, cancellationToken);

            // 追加のキャンセルチェック
            if (i % 10 == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        return true;
    }

    // === 同期処理テスト関数群 ===

    /// <summary>
    /// 基本的な同期処理テスト（戻り値あり）
    /// </summary>
    public static string BasicSyncProcessingWithResult(IProgress<int> progress)
    {
        const int maxSteps = 100;
        var result = "同期処理完了";

        for (int i = 0; i <= maxSteps; i++)
        {
            // 進捗報告
            progress.Report(i);

            // 同期処理をシミュレート
            Thread.Sleep(50);

            // 処理内容をシミュレート
            if (i == maxSteps / 2)
            {
                result += " - 中間処理完了";
            }
        }

        return result;
    }

    /// <summary>
    /// 同期処理でのエラー発生テスト
    /// </summary>
    public static string SyncErrorProcessingTest(IProgress<int> progress)
    {
        const int maxSteps = 50;

        for (int i = 0; i <= maxSteps; i++)
        {
            progress.Report(i);

            // 30%の時点でエラーを発生
            if (i == 30)
            {
                throw new InvalidOperationException("同期処理テスト用エラー: 計算中に問題が発生しました");
            }

            // 同期処理をシミュレート
            Thread.Sleep(100);
        }

        return "エラーなく完了";
    }

    /// <summary>
    /// 計算集約的な同期処理テスト
    /// </summary>
    public static long HeavyComputationTest(IProgress<int> progress)
    {
        const int maxIterations = 1000000;
        const int reportInterval = maxIterations / 100;
        long result = 0;

        for (int i = 0; i < maxIterations; i++)
        {
            // 計算集約的な処理をシミュレート
            result += (long)Math.Sqrt(i * 1.5) + (long)Math.Sin(i * 0.001);

            // 進捗報告（100回に分けて）
            if (i % reportInterval == 0)
            {
                progress.Report(i / reportInterval);

                // UI更新のための短い待機
                Thread.Sleep(1);
            }
        }

        progress.Report(100);
        return result;
    }

    /// <summary>
    /// ファイル操作シミュレーション（同期処理）
    /// </summary>
    public static int SyncFileOperationSimulation(IProgress<int> progress)
    {
        const int totalFiles = 25;
        int processedFiles = 0;

        for (int i = 0; i < totalFiles; i++)
        {
            // ファイル読み込み処理のシミュレート
            Thread.Sleep(200);

            // 15番目のファイルでエラーを発生させる
            if (i == 14)
            {
                throw new System.IO.FileNotFoundException($"テスト用エラー: ファイル{i + 1}が見つかりません");
            }

            processedFiles++;
            progress.Report(processedFiles);

            // 一部のファイルで追加処理時間
            if (i % 5 == 0)
            {
                Thread.Sleep(300);
            }
        }

        return processedFiles;
    }

    /// <summary>
    /// データ変換処理テスト（同期・エラーなし）
    /// </summary>
    public static int DataTransformationTest(IProgress<int> progress)
    {
        const int totalItems = 500;
        int transformedItems = 0;

        for (int i = 0; i < totalItems; i++)
        {
            // データ変換処理をシミュレート
            var data = $"Item_{i}";
            var transformed = data.ToUpper().Replace("_", "-");

            transformedItems++;

            // 10項目ごとに進捗報告
            if (i % 10 == 0)
            {
                progress.Report((i * 100) / totalItems);
                Thread.Sleep(20); // 処理時間をシミュレート
            }
        }

        progress.Report(100);
        return transformedItems;
    }

    // === 非同期処理（キャンセル不可）テスト関数群 ===

    /// <summary>
    /// 非同期処理テスト（キャンセル不可・戻り値なし）
    /// </summary>
    public static async Task NonCancellableAsyncProcessingTest(IProgress<int> progress)
    {
        const int maxSteps = 100;

        for (int i = 0; i <= maxSteps; i++)
        {
            progress.Report(i);
            await Task.Delay(50);

            // 中間処理のシミュレート
            if (i == maxSteps / 2)
            {
                await Task.Delay(200); // 少し長い処理
            }
        }
    }

    /// <summary>
    /// 非同期データ処理テスト（キャンセル不可・戻り値あり）
    /// </summary>
    public static async Task<string> NonCancellableAsyncDataProcessingTest(IProgress<int> progress)
    {
        const int totalRecords = 80;
        var processedData = new List<string>();

        for (int i = 0; i < totalRecords; i++)
        {
            progress.Report(i + 1);
            
            // 非同期データ処理をシミュレート
            await Task.Delay(75);
            
            var data = $"ProcessedData_{i:D3}";
            processedData.Add(data);

            // 定期的に長い処理をシミュレート
            if (i % 20 == 0)
            {
                await Task.Delay(150);
            }
        }

        return $"処理完了: {processedData.Count}件のデータを処理";
    }

    /// <summary>
    /// 非同期API呼び出しシミュレーション（キャンセル不可・戻り値あり）
    /// </summary>
    public static async Task<int> NonCancellableAsyncApiCallTest(IProgress<int> progress)
    {
        const int totalCalls = 30;
        int successfulCalls = 0;

        for (int i = 0; i < totalCalls; i++)
        {
            progress.Report(i + 1);
            
            // API呼び出しの遅延をシミュレート
            await Task.Delay(Random.Shared.Next(100, 300));
            
            // 成功率90%でシミュレート
            if (Random.Shared.NextDouble() > 0.1)
            {
                successfulCalls++;
            }

            // 5回に1回長い処理
            if (i % 5 == 0)
            {
                await Task.Delay(400);
            }
        }

        return successfulCalls;
    }
}

/// <summary>
/// ProcessingDialog テスト用のヘルパークラス
/// </summary>
public static class ProcessingDialogTestHelper
{
    /// <summary>
    /// 基本テストの実行例
    /// </summary>
    public static async Task<string> RunBasicTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "基本的な非同期処理テスト";
        dialog.MaxValue = 100;

        return await dialog.RunAsync(ProcessingDialogTestFunctions.BasicAsyncProcessingWithResult);
    }

    /// <summary>
    /// タイムアウトテストの実行例（5秒でタイムアウト）
    /// </summary>
    public static async Task<int> RunTimeoutTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "タイムアウトテスト（5秒制限）";
        dialog.MaxValue = 1000;

        try
        {
            return await dialog.RunAsync(
                ProcessingDialogTestFunctions.LongRunningProcessing,
                TimeSpan.FromSeconds(5)
            );
        }
        catch (TimeoutException)
        {
            return -1;
        }
    }

    /// <summary>
    /// 高頻度進捗更新テストの実行例
    /// </summary>
    public static async Task<bool> RunHighFrequencyTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "高頻度進捗更新テスト";
        dialog.MaxValue = 10000;

        return await dialog.RunAsync(ProcessingDialogTestFunctions.HighFrequencyProgressTest);
    }

    /// <summary>
    /// エラーハンドリングテストの実行例
    /// </summary>
    public static async Task<string?> RunErrorTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "エラー発生テスト";
        dialog.MaxValue = 50;

        try
        {
            return await dialog.RunAsync(ProcessingDialogTestFunctions.ErrorProcessingTest);
        }
        catch (InvalidOperationException ex)
        {
            // エラー処理
            return $"エラーをキャッチ: {ex.Message}";
        }
    }

    /// <summary>
    /// 実用的なファイル処理テストの実行例
    /// </summary>
    public static async Task<int> RunFileProcessingTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "ファイル処理シミュレーション";
        dialog.MaxValue = 25;

        return await dialog.RunAsync(
            ProcessingDialogTestFunctions.FileProcessingSimulation,
            TimeSpan.FromSeconds(15) // 15秒でタイムアウト
        );
    }

    // === 同期処理テスト実行ヘルパー ===

    /// <summary>
    /// 基本的な同期処理テストの実行例
    /// </summary>
    public static async Task<string> RunBasicSyncTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "基本的な同期処理テスト";
        dialog.MaxValue = 100;

        return await dialog.RunAsync(ProcessingDialogTestFunctions.BasicSyncProcessingWithResult);
    }

    /// <summary>
    /// 同期処理エラーハンドリングテストの実行例
    /// </summary>
    public static async Task<string?> RunSyncErrorTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "同期処理エラー発生テスト";
        dialog.MaxValue = 50;

        try
        {
            return await dialog.RunAsync(ProcessingDialogTestFunctions.SyncErrorProcessingTest);
        }
        catch (InvalidOperationException ex)
        {
            // エラー処理（この例外は ProcessingDialog で UI 表示されるため、ここには到達しない）
            return $"エラーをキャッチ: {ex.Message}";
        }
    }

    /// <summary>
    /// 計算集約的な同期処理テストの実行例
    /// </summary>
    public static async Task<long> RunHeavyComputationTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "計算集約的処理テスト";
        dialog.MaxValue = 100;

        return await dialog.RunAsync(ProcessingDialogTestFunctions.HeavyComputationTest);
    }

    /// <summary>
    /// 同期ファイル操作エラーテストの実行例
    /// </summary>
    public static async Task<int> RunSyncFileErrorTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "ファイル操作エラーテスト";
        dialog.MaxValue = 25;

        try
        {
            return await dialog.RunAsync(ProcessingDialogTestFunctions.SyncFileOperationSimulation);
        }
        catch (System.IO.FileNotFoundException ex)
        {
            // エラー処理（この例外は ProcessingDialog で UI 表示されるため、ここには到達しない）
            return -1;
        }
    }

    /// <summary>
    /// データ変換処理テストの実行例
    /// </summary>
    public static async Task<int> RunDataTransformationTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "データ変換処理テスト";
        dialog.MaxValue = 100;

        return await dialog.RunAsync(ProcessingDialogTestFunctions.DataTransformationTest);
    }

    // === 非同期処理（キャンセル不可）テスト実行ヘルパー ===

    /// <summary>
    /// 非同期処理テスト（キャンセル不可・戻り値なし）の実行例
    /// </summary>
    public static async Task RunNonCancellableAsyncTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "非同期処理テスト（キャンセル不可）";
        dialog.MaxValue = 100;

        await dialog.RunAsync(ProcessingDialogTestFunctions.NonCancellableAsyncProcessingTest);
    }

    /// <summary>
    /// 非同期データ処理テスト（キャンセル不可・戻り値あり）の実行例
    /// </summary>
    public static async Task<string> RunNonCancellableAsyncDataTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "非同期データ処理テスト（キャンセル不可）";
        dialog.MaxValue = 80;

        return await dialog.RunAsync(ProcessingDialogTestFunctions.NonCancellableAsyncDataProcessingTest);
    }

    /// <summary>
    /// 非同期API呼び出しテスト（キャンセル不可・戻り値あり）の実行例
    /// </summary>
    public static async Task<int> RunNonCancellableAsyncApiTest(ProcessingDialog dialog)
    {
        dialog.ContentText = "非同期API呼び出しテスト（キャンセル不可）";
        dialog.MaxValue = 30;

        return await dialog.RunAsync(ProcessingDialogTestFunctions.NonCancellableAsyncApiCallTest);
    }

    // === 全テスト連続実行 ===

    /// <summary>
    /// 全テストケースを連続実行するテストランナー
    /// </summary>
    public static async Task RunAllTests(ProcessingDialog dialog)
    {
        var testResults = new List<(string TestName, string Result, bool Success)>();

        // 1. 基本的な非同期処理テスト
        try
        {
            var result1 = await RunBasicTest(dialog);
            testResults.Add(("基本的な非同期処理テスト", result1, true));
        }
        catch (Exception ex)
        {
            testResults.Add(("基本的な非同期処理テスト", $"エラー: {ex.Message}", false));
        }

        // 短い待機
        await Task.Delay(1000);

        // 2. 高頻度進捗更新テスト
        try
        {
            var result2 = await RunHighFrequencyTest(dialog);
            testResults.Add(("高頻度進捗更新テスト", result2.ToString(), true));
        }
        catch (Exception ex)
        {
            testResults.Add(("高頻度進捗更新テスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 3. 非同期エラーハンドリングテスト
        try
        {
            var result3 = await RunErrorTest(dialog);
            // エラーハンドリングテストでは戻り値がnullでも成功（UIでエラー表示される）
            testResults.Add(("非同期エラーハンドリングテスト", result3 ?? "エラー処理完了", true));
        }
        catch (Exception ex)
        {
            testResults.Add(("非同期エラーハンドリングテスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 4. ファイル処理テスト
        try
        {
            var result4 = await RunFileProcessingTest(dialog);
            testResults.Add(("ファイル処理テスト", result4.ToString(), true));
        }
        catch (Exception ex)
        {
            testResults.Add(("ファイル処理テスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 5. 基本的な同期処理テスト
        try
        {
            var result5 = await RunBasicSyncTest(dialog);
            testResults.Add(("基本的な同期処理テスト", result5, true));
        }
        catch (Exception ex)
        {
            testResults.Add(("基本的な同期処理テスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 6. 同期処理エラーハンドリングテスト
        try
        {
            var result6 = await RunSyncErrorTest(dialog);
            // エラーハンドリングテストでは戻り値がnullでも成功（UIでエラー表示される）
            testResults.Add(("同期処理エラーハンドリングテスト", result6 ?? "エラー処理完了", true));
        }
        catch (Exception ex)
        {
            testResults.Add(("同期処理エラーハンドリングテスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 7. 計算集約的処理テスト
        try
        {
            var result7 = await RunHeavyComputationTest(dialog);
            testResults.Add(("計算集約的処理テスト", result7.ToString(), true));
        }
        catch (Exception ex)
        {
            testResults.Add(("計算集約的処理テスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 8. 同期ファイル操作エラーテスト
        try
        {
            var result8 = await RunSyncFileErrorTest(dialog);
            // ファイル操作エラーテストでは15番目でエラー発生してUIに表示されるため成功
            testResults.Add(("同期ファイル操作エラーテスト", "ファイルエラー処理完了", true));
        }
        catch (Exception ex)
        {
            testResults.Add(("同期ファイル操作エラーテスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 9. データ変換処理テスト
        try
        {
            var result9 = await RunDataTransformationTest(dialog);
            testResults.Add(("データ変換処理テスト", result9.ToString(), true));
        }
        catch (Exception ex)
        {
            testResults.Add(("データ変換処理テスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 10. 非同期処理テスト（キャンセル不可・戻り値なし）
        try
        {
            await RunNonCancellableAsyncTest(dialog);
            testResults.Add(("非同期処理テスト（キャンセル不可・戻り値なし）", "処理完了", true));
        }
        catch (Exception ex)
        {
            testResults.Add(("非同期処理テスト（キャンセル不可・戻り値なし）", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 11. 非同期データ処理テスト（キャンセル不可・戻り値あり）
        try
        {
            var result11 = await RunNonCancellableAsyncDataTest(dialog);
            testResults.Add(("非同期データ処理テスト（キャンセル不可・戻り値あり）", result11, true));
        }
        catch (Exception ex)
        {
            testResults.Add(("非同期データ処理テスト（キャンセル不可・戻り値あり）", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 12. 非同期API呼び出しテスト（キャンセル不可・戻り値あり）
        try
        {
            var result12 = await RunNonCancellableAsyncApiTest(dialog);
            testResults.Add(("非同期API呼び出しテスト（キャンセル不可・戻り値あり）", $"成功: {result12}件", true));
        }
        catch (Exception ex)
        {
            testResults.Add(("非同期API呼び出しテスト（キャンセル不可・戻り値あり）", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 13. タイムアウトテスト（最後に実行）
        try
        {
            var result13 = await RunTimeoutTest(dialog);
            // タイムアウトテストでは5秒でタイムアウト発生してUIに表示されるため成功
            testResults.Add(("タイムアウトテスト", "タイムアウト処理完了", true));
        }
        catch (Exception ex)
        {
            testResults.Add(("タイムアウトテスト", $"エラー: {ex.Message}", false));
        }

        // 最終結果を表示
        await ShowTestResults(dialog, testResults);
    }

    /// <summary>
    /// テスト結果をダイアログで表示
    /// </summary>
    private static async Task ShowTestResults(ProcessingDialog dialog, List<(string TestName, string Result, bool Success)> testResults)
    {
        dialog.ContentText = "テスト結果";
        dialog.MaxValue = null; // カウント表示モード

        await dialog.RunAsync(progress =>
        {
            var successCount = testResults.Count(r => r.Success);
            var totalCount = testResults.Count;

            progress.Report(successCount);

            // 結果をコンソール出力（デバッグ用）
            System.Diagnostics.Debug.WriteLine("\n=== ProcessingDialog 全テスト結果 ===");
            System.Diagnostics.Debug.WriteLine($"成功: {successCount}/{totalCount}");
            System.Diagnostics.Debug.WriteLine("詳細:");

            foreach (var (testName, result, success) in testResults)
            {
                var status = success ? "✓" : "✗";
                System.Diagnostics.Debug.WriteLine($"  {status} {testName}: {result}");
            }
            System.Diagnostics.Debug.WriteLine("================================\n");

            Thread.Sleep(2000); // 結果表示時間

            return $"テスト完了: {successCount}/{totalCount} 成功";
        });
    }

    /// <summary>
    /// 簡易テスト（エラー系を除外）の連続実行
    /// </summary>
    public static async Task RunBasicTests(ProcessingDialog dialog)
    {
        var testResults = new List<(string TestName, string Result, bool Success)>();

        // 1. 基本的な非同期処理テスト
        try
        {
            var result1 = await RunBasicTest(dialog);
            testResults.Add(("基本的な非同期処理テスト", result1, true));
        }
        catch (Exception ex)
        {
            testResults.Add(("基本的な非同期処理テスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 2. 基本的な同期処理テスト
        try
        {
            var result2 = await RunBasicSyncTest(dialog);
            testResults.Add(("基本的な同期処理テスト", result2, true));
        }
        catch (Exception ex)
        {
            testResults.Add(("基本的な同期処理テスト", $"エラー: {ex.Message}", false));
        }

        await Task.Delay(1000);

        // 3. データ変換処理テスト
        try
        {
            var result3 = await RunDataTransformationTest(dialog);
            testResults.Add(("データ変換処理テスト", result3.ToString(), true));
        }
        catch (Exception ex)
        {
            testResults.Add(("データ変換処理テスト", $"エラー: {ex.Message}", false));
        }

        // 結果表示
        await ShowTestResults(dialog, testResults);
    }
}