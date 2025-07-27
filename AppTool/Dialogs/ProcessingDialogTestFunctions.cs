using System;
using System.Collections.Generic;
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

            // 長時間処理をシミュレート（500ms間隔）
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

            // 高頻度で進捗更新
            progress.Report(i);

            // 短い間隔で処理
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
            // タイムアウト時の処理
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
}