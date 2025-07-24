using AppTool.Contracts.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AppTool.Dialogs;

public sealed partial class ProcessingDialog : ContentDialog
{
    public string ProgressingContent { get; set; } = "処理中...";
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

            StatusTextBlock.Text += " 完了しました。";
            await Task.Delay(1000);
            tcs.TrySetResult(result);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text += $" エラー: {ex.Message}";
            await Task.Delay(1000);
            tcs.TrySetException(ex);
        }
        finally
        {
            Hide();
        }
    }

    private void ProgressingPercentageRender(int value)
    {
        if (!MaxValue.HasValue) throw new Exception("想定外エラー");

        double ratio = (double)((double)value / MaxValue);

        var sb = new StringBuilder();
        sb.AppendFormat("{0} / {1} ", value, MaxValue);
        sb.Append('(');
        sb.AppendFormat("{0,4:##0.0}%", ratio * 100);
        sb.Append(')');

        StatusTextBlock.Text = sb.ToString();
        ContentProgressBar.Value = ratio * 100;
    }

    private void ProgressingCountRender(int value)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("{0} 件", value);

        StatusTextBlock.Text = sb.ToString();
        ContentProgressBar.IsIndeterminate = true;
    }
}