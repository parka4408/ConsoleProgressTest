using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace AppTool.Dialogs;

public sealed partial class ProcessingDialog : ContentDialog, IDisposable, INotifyPropertyChanged
{
    /// <summary>
    /// Contentプロパティの外部設定を禁止
    /// </summary>
    public new object Content { get; private set; }

    // 最大値（設定あり：進捗率を表示、設定なし：件数カウントのみ）
    public int? MaxValue { get; set; }

    // INotifyPropertyChanged実装
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // カスタマイズ可能なテキスト
    private string _contentText = "処理中";
    public string ContentText
    {
        get => _contentText;
        set
        {
            if (_contentText != value)
            {
                _contentText = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string CancelButtonText { get; set; } = "キャンセル";
    public string CompletedMessage { get; set; } = "すべての処理が正常に終了しました。";
    public string CanceledMessage { get; set; } = "処理がキャンセルされました。";
    public string TimeoutMessage { get; set; } = "処理がタイムアウトしました。";

    // ロックオブジェクト
    private readonly object _lock = new();

    // キャンセル用
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning = false;

    // 進捗更新のスロットリング用
    private DateTime _lastProgressUpdate = DateTime.MinValue;
    private const int ProgressUpdateIntervalMs = 50; // 50ms間隔で更新制限

    // 最新の進捗値を保持
    private volatile int _latestProgressValue = 0;
    private volatile bool _hasPendingUpdate = false;
    private Timer? _progressFlushTimer;

    /// <summary>
    /// ProcessingDialogクラスの新しいインスタンスを初期化
    /// </summary>
    public ProcessingDialog()
    {
        InitializeComponent();

        // XAMLで定義されたContentを取得して固定化
        Content = base.Content;
        
        // DataContextを自分自身に設定してBindingを有効化
        DataContext = this;

        SecondaryButtonClick += OnCancelButtonClick;
    }

    /// <summary>
    /// ダイアログの状態を初期状態にリセット
    /// </summary>
    public async Task ResetState()
    {
        var tcs = new TaskCompletionSource<bool>();
        
        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                // UI要素を初期状態にリセット
                ProgressingTextBlock.Text = " - ";
                ContentProgressBar.IsIndeterminate = !MaxValue.HasValue;
                ContentProgressBar.Value = 0;
                StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                StatusTextBlock.Text = "";
                
                // ボタン状態をリセット
                PrimaryButtonText = "";
                SecondaryButtonText = "";
                IsPrimaryButtonEnabled = false;
                
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        
        // UI更新の完了を待機
        await tcs.Task;
    }

    /// <summary>
    /// キャンセルボタンクリック時の処理
    /// </summary>
    private void OnCancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _cancellationTokenSource?.Cancel();

        args.Cancel = true;
    }

    /// <summary>
    /// 同期処理を進捗表示付きで実行
    /// </summary>
    /// <param name="action">実行する処理</param>
    public async Task RunAsync(Action<IProgress<int>> action)
    {
        await ExecuteRunAsync(async tcs =>
        {
            await StartProcessing(() => { action(CreateProgress()); return true; }, tcs, _cancellationTokenSource!.Token);
        }, false);
    }

    /// <summary>
    /// 同期処理を進捗表示付きで実行し、結果を返却
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="func">実行する処理</param>
    /// <returns>処理の実行結果</returns>
    public async Task<T> RunAsync<T>(Func<IProgress<int>, T> func)
    {
        return await ExecuteRunAsync<T>(async tcs =>
        {
            await StartProcessing(() => func(CreateProgress()), tcs, _cancellationTokenSource!.Token);
        }, false);
    }

    /// <summary>
    /// 非同期処理をキャンセルとタイムアウト制御付きで実行
    /// </summary>
    /// <param name="asyncAction">実行する非同期処理</param>
    /// <param name="timeout">タイムアウト時間（nullの場合は無制限）</param>
    public async Task RunAsync(Func<IProgress<int>, CancellationToken, Task> asyncAction, TimeSpan? timeout = null)
    {
        await ExecuteRunAsync(async tcs =>
        {
            await StartProcessing(async () => { await asyncAction(CreateProgress(), _cancellationTokenSource!.Token); return true; }, tcs, _cancellationTokenSource!.Token);
        }, true, timeout);
    }

    /// <summary>
    /// 非同期処理をキャンセルとタイムアウト制御付きで実行し、結果を返却
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="asyncFunc">実行する非同期処理</param>
    /// <param name="timeout">タイムアウト時間（nullの場合は無制限）</param>
    /// <returns>処理の実行結果</returns>
    public async Task<T> RunAsync<T>(Func<IProgress<int>, CancellationToken, Task<T>> asyncFunc, TimeSpan? timeout = null)
    {
        return await ExecuteRunAsync<T>(async tcs =>
        {
            await StartProcessing(() => asyncFunc(CreateProgress(), _cancellationTokenSource!.Token), tcs, _cancellationTokenSource!.Token);
        }, true, timeout);
    }

    /// <summary>
    /// 実行開始を試行（重複実行を防止）
    /// </summary>
    /// <returns>実行開始できる場合はtrue、既に実行中の場合はfalse</returns>
    private bool TryStartExecution()
    {
        lock (_lock)
        {
            if (_isRunning) return false;
            _isRunning = true;
            return true;
        }
    }

    /// <summary>
    /// 実行を完了し、リソースをクリーンアップ
    /// </summary>
    private void CompleteExecution()
    {
        lock (_lock)
        {
            _isRunning = false;

            // CancellationTokenSourceの状態をチェックしてから安全にキャンセル
            if (_cancellationTokenSource != null)
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // まだキャンセルされていない場合のみ
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }

    /// <summary>
    /// 処理実行とダイアログ表示を統合制御し、例外ハンドリングとUI状態管理を一元化
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="processStarter">処理開始デリゲート</param>
    /// <param name="showCancelButton">キャンセルボタンを表示するか</param>
    /// <param name="timeout">タイムアウト時間</param>
    /// <returns>処理の実行結果</returns>
    private async Task<T> ExecuteRunAsync<T>(Func<TaskCompletionSource<T>, Task> processStarter, bool showCancelButton, TimeSpan? timeout = null)
    {
        if (!TryStartExecution())
        {
            if (typeof(T) == typeof(bool))
                return default!; // void相当の場合はreturn
            else
                throw new InvalidOperationException("処理が既に実行中です。");
        }

        // ダイアログの状態を初期状態にリセット
        await ResetState();

        // ダイアログ表示（処理完了まで待機）
        var dialogTask = ShowAsync();

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<T>();

            // UI設定
            SecondaryButtonText = showCancelButton ? CancelButtonText : "";
            ContentProgressBar.IsIndeterminate = !MaxValue.HasValue;

            // 処理開始（バックグラウンドで実行、awaitしない）
            _ = processStarter(tcs);

            // 処理完了タスクとタイムアウトを並行実行
            var (result, timeoutOccurred) = await ExecuteWithTimeout(tcs.Task, timeout);

            // 正常完了の場合のUI状態設定
            SetCompletedState();

            // 最終的な進捗値を確実に表示
            FlushFinalProgress();

            // ダイアログがまだ開いている場合はユーザーの応答を待つ
            await dialogTask;

            return result;
        }
        catch (TimeoutException)
        {
            // タイムアウト発生時
            SetTimeoutState();
            await dialogTask;
            
            // 戻り値を持つメソッドの場合はTimeoutExceptionを再スロー
            if (typeof(T) != typeof(bool))
            {
                throw;
            }
            return default!;
        }
        catch (OperationCanceledException) when (_cancellationTokenSource?.Token.IsCancellationRequested == true)
        {
            // ユーザーキャンセル時
            SetCanceledState();
            await dialogTask;
            return default!;
        }
        catch (Exception ex)
        {
            // 例外
            SetErrorState(ex);
            await dialogTask;
            return default!;
        }
        finally
        {
            CompleteExecution();
        }
    }

    /// <summary>
    /// 処理実行とダイアログ表示を統合制御（戻り値なし版）
    /// </summary>
    /// <param name="processStarter">処理開始デリゲート</param>
    /// <param name="showCancelButton">キャンセルボタンを表示するか</param>
    /// <param name="timeout">タイムアウト時間</param>
    private async Task ExecuteRunAsync(Func<TaskCompletionSource<bool>, Task> processStarter, bool showCancelButton, TimeSpan? timeout = null)
    {
        await ExecuteRunAsync<bool>(processStarter, showCancelButton, timeout);
    }

    /// <summary>
    /// 保留中の進捗更新を即座にフラッシュして最終値を確実に表示
    /// </summary>
    private void FlushFinalProgress()
    {
        lock (_lock)
        {
            if (_hasPendingUpdate)
            {
                // 保留中の更新を即座に実行
                var finalValue = _latestProgressValue;
                _hasPendingUpdate = false;

                // タイマーをクリア
                _progressFlushTimer?.Dispose();
                _progressFlushTimer = null;

                // UI更新を実行
                DispatcherQueue.TryEnqueue(() => UpdateProgress(finalValue));
            }
        }
    }

    /// <summary>
    /// MaxValueの設定に応じて適切なProgressインスタンスを作成
    /// </summary>
    /// <returns>パーセンテージまたはカウント表示用のProgressインスタンス</returns>
    private Progress<int> CreateProgress()
    {
        return MaxValue.HasValue ?
            new Progress<int>(ThrottledProgressingPercentageRender) :
            new Progress<int>(ThrottledProgressingCountRender);
    }

    /// <summary>
    /// MaxValueの設定に応じて適切な方法で進捗表示を更新
    /// </summary>
    /// <param name="value">進捗値</param>
    private void UpdateProgress(int value)
    {
        if (MaxValue.HasValue)
        {
            ProgressingPercentageRender(value);
        }
        else
        {
            ProgressingCountRender(value);
        }
    }

    /// <summary>
    /// パーセンテージ進捗をスロットリング機能付きで更新
    /// </summary>
    /// <param name="value">進捗値</param>
    private void ThrottledProgressingPercentageRender(int value)
    {
        lock (_lock)
        {
            _latestProgressValue = value;
            var now = DateTime.UtcNow;

            // スロットリング適用
            if ((now - _lastProgressUpdate).TotalMilliseconds < ProgressUpdateIntervalMs)
            {
                _hasPendingUpdate = true;
                EnsureFlushTimer();
                return;
            }

            _lastProgressUpdate = now;
            _hasPendingUpdate = false;
        }

        ProgressingPercentageRender(value);
    }

    /// <summary>
    /// カウント進捗をスロットリング機能付きで更新
    /// </summary>
    /// <param name="value">進捗値</param>
    private void ThrottledProgressingCountRender(int value)
    {
        lock (_lock)
        {
            _latestProgressValue = value;
            var now = DateTime.UtcNow;

            // スロットリング適用
            if ((now - _lastProgressUpdate).TotalMilliseconds < ProgressUpdateIntervalMs)
            {
                _hasPendingUpdate = true;
                EnsureFlushTimer();
                return;
            }

            _lastProgressUpdate = now;
            _hasPendingUpdate = false;
        }

        ProgressingCountRender(value);
    }

    /// <summary>
    /// スロットリングされた進捗更新をフラッシュするタイマーを確保
    /// </summary>
    private void EnsureFlushTimer()
    {
        if (_progressFlushTimer == null)
        {
            // 遅延フラッシュタイマーを設定（スロットリング間隔の1.5倍後に実行）
            _progressFlushTimer = new Timer(FlushPendingProgress, null,
                (int)(ProgressUpdateIntervalMs * 1.5), System.Threading.Timeout.Infinite);
        }
    }

    /// <summary>
    /// 保留中の進捗更新をUIにフラッシュするタイマーコールバック
    /// </summary>
    /// <param name="state">タイマーコールバック状態（未使用）</param>
    private void FlushPendingProgress(object? state)
    {
        int valueToFlush;
        bool shouldFlush;

        lock (_lock)
        {
            shouldFlush = _hasPendingUpdate;
            valueToFlush = _latestProgressValue;
            _hasPendingUpdate = false;
            _lastProgressUpdate = DateTime.UtcNow;

            // タイマーを再設定
            _progressFlushTimer?.Dispose();
            _progressFlushTimer = null;
        }

        if (shouldFlush)
        {
            // UI更新（最新値で）
            UpdateProgress(valueToFlush);
        }
    }

    /// <summary>
    /// タスクをタイムアウト制御付きで実行し、結果とタイムアウト状態を返す
    /// </summary>
    /// <typeparam name="T">タスクの戻り値型</typeparam>
    /// <param name="task">実行するタスク</param>
    /// <param name="timeout">タイムアウト時間（nullの場合は無制限）</param>
    /// <returns>タスクの実行結果とタイムアウト状態のタプル</returns>
    private async Task<(T Result, bool TimeoutOccurred)> ExecuteWithTimeout<T>(Task<T> task, TimeSpan? timeout)
    {
        if (!timeout.HasValue)
        {
            var result = await task;
            return (result, false);
        }

        var timeoutTask = Task.Delay(timeout.Value);
        var completedTask = await Task.WhenAny(task, timeoutTask);
        
        if (completedTask == timeoutTask)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            throw new TimeoutException("処理がタイムアウトしました。");
        }
        else
        {
            var result = await task;
            return (result, false);
        }
    }

    /// <summary>
    /// 同期処理をバックグラウンドで実行し、結果をTaskCompletionSourceに設定
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="syncFunc">実行する同期処理</param>
    /// <param name="tcs">完了通知用のTaskCompletionSource</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    private async Task StartProcessing<T>(Func<T> syncFunc, TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
    {
        try
        {
            T result = await Task.Run(syncFunc, cancellationToken).ConfigureAwait(false);
            tcs.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            tcs.TrySetCanceled(cancellationToken);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
    }

    /// <summary>
    /// 非同期処理を実行し、結果をTaskCompletionSourceに設定
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="asyncFunc">実行する非同期処理</param>
    /// <param name="tcs">完了通知用のTaskCompletionSource</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    private async Task StartProcessing<T>(Func<Task<T>> asyncFunc, TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
    {
        try
        {
            T result = await asyncFunc().ConfigureAwait(false);
            tcs.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            tcs.TrySetCanceled(cancellationToken);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
    }

    /// <summary>
    /// 処理正常完了時のUI状態を設定
    /// </summary>
    private void SetCompletedState()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // 処理完了時に最終進捗を確実に表示
            if (MaxValue.HasValue)
            {
                ProgressingPercentageRender(MaxValue.Value);
            }
            else
            {
                ContentProgressBar.IsIndeterminate = false;
                ContentProgressBar.Value = 100;
            }

            StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            StatusTextBlock.Text = CompletedMessage;
            PrimaryButtonText = "OK";
            SecondaryButtonText = "";
            IsPrimaryButtonEnabled = true;
        });
    }

    /// <summary>
    /// ユーザーキャンセル時のUI状態を設定
    /// </summary>
    private void SetCanceledState()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            StatusTextBlock.Text = CanceledMessage;
            PrimaryButtonText = "閉じる";
            SecondaryButtonText = "";
            IsPrimaryButtonEnabled = true;
        });
    }

    /// <summary>
    /// タイムアウト時のUI状態を設定
    /// </summary>
    private void SetTimeoutState()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            StatusTextBlock.Text = TimeoutMessage;
            PrimaryButtonText = "閉じる";
            SecondaryButtonText = "";
            IsPrimaryButtonEnabled = true;
        });
    }

    /// <summary>
    /// エラー発生時のUI状態を設定
    /// </summary>
    /// <param name="ex">発生した例外</param>
    private void SetErrorState(Exception ex)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            StatusTextBlock.Text = $"エラーが発生しました:\n{ex.Message}";
            PrimaryButtonText = "閉じる";
            SecondaryButtonText = "";
            IsPrimaryButtonEnabled = true;
        });
    }

    /// <summary>
    /// 進捗をパーセンテージ形式で表示
    /// </summary>
    /// <param name="value">現在の進捗値</param>
    /// <exception cref="ArgumentException">MaxValueが設定されていない場合</exception>
    private void ProgressingPercentageRender(int value)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (!MaxValue.HasValue)
            {
                throw new ArgumentException("MaxValueが設定されていないため、パーセンテージ表示できません。");
            }

            double ratio = (double)value / MaxValue.Value;

            var sb = new StringBuilder();
            sb.AppendFormat("{0} / {1} ", value, MaxValue);
            sb.Append('(');
            sb.AppendFormat("{0,4:##0.0}%", ratio * 100);
            sb.Append(')');

            ProgressingTextBlock.Text = sb.ToString();
            ContentProgressBar.Value = ratio * 100;
        });
    }

    /// <summary>
    /// 進捗をカウント形式で表示
    /// </summary>
    /// <param name="value">現在のカウント値</param>
    private void ProgressingCountRender(int value)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} 件", value);

            ProgressingTextBlock.Text = sb.ToString();
        });
    }

    /// <summary>
    /// ProcessingDialogが使用するすべてのリソースを解放
    /// </summary>
    public void Dispose()
    {
        // タイマーリソースを解放
        _progressFlushTimer?.Dispose();
        _progressFlushTimer = null;

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}