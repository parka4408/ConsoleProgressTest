using AppTool.Contracts.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AppTool.Dialogs;

public sealed partial class ProcessingDialog : ContentDialog
{
    public string? ContentText { get; set; }
    public int? MaxValue { get; set; }

    public ProcessingDialog()
    {
        InitializeComponent();
    }

    public async Task<T> RunAsync<T>(IProcesser<int, T> processer)
    {
        var tcs = new TaskCompletionSource<T>();

        // 処理開始（非同期）
        _ = StartProcessing(processer, tcs);

        // ダイアログを表示（閉じるまで待機）
        await ShowAsync();

        return await tcs.Task;
    }

    private async Task StartProcessing<T>(IProcesser<int, T> processer, TaskCompletionSource<T> tcs)
    {
        try
        {
            var progress = MaxValue.HasValue ?
                new Progress<int>(ProgressingPercentageRender) : new Progress<int>(ProgressingCountRender);
            var result = await processer.ExecuteAsync(progress);

            DispatcherQueue.TryEnqueue(() =>
            {
                StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                StatusTextBlock.Text = $"全ての処理が正常に終了しました。";

                PrimaryButtonText = "OK";
                IsPrimaryButtonEnabled = true;
            });

            tcs.TrySetResult(result);
        }
        catch (Exception ex)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                StatusTextBlock.Text = $"例外発生: {ex.Message}";

                PrimaryButtonText = "閉じる";
                IsPrimaryButtonEnabled = true;
            });

            tcs.TrySetException(ex);
        }
    }

    private void ProgressingPercentageRender(int value)
    {
        if (!MaxValue.HasValue)
        {
            throw new ArgumentException("MaxValueが設定されていないため、パーセンテージ表示ができません。");
        }

        double ratio = (double)value / MaxValue.Value;

        var sb = new StringBuilder();
        sb.AppendFormat("{0} / {1} ", value, MaxValue);
        sb.Append('(');
        sb.AppendFormat("{0,4:##0.0}%", ratio * 100);
        sb.Append(')');

        ProgressingTextBlock.Text = sb.ToString();
        ContentProgressBar.Value = ratio * 100;
    }

    private void ProgressingCountRender(int value)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("{0} 件", value);

        ProgressingTextBlock.Text = sb.ToString();
        ContentProgressBar.IsIndeterminate = true;
    }
}