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

        // �����J�n�i�񓯊��j
        _ = StartProcessing(processer, tcs);

        // �_�C�A���O��\���i����܂őҋ@�j
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
                StatusTextBlock.Text = $"�S�Ă̏���������ɏI�����܂����B";

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
                StatusTextBlock.Text = $"��O����: {ex.Message}";

                PrimaryButtonText = "����";
                IsPrimaryButtonEnabled = true;
            });

            tcs.TrySetException(ex);
        }
    }

    private void ProgressingPercentageRender(int value)
    {
        if (!MaxValue.HasValue)
        {
            throw new ArgumentException("MaxValue���ݒ肳��Ă��Ȃ����߁A�p�[�Z���e�[�W�\�����ł��܂���B");
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
        sb.AppendFormat("{0} ��", value);

        ProgressingTextBlock.Text = sb.ToString();
        ContentProgressBar.IsIndeterminate = true;
    }
}