using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace AppTool.Dialogs;

public sealed partial class ProcessingDialog : ContentDialog, IDisposable
{
    /// <summary>
    /// �ő�l�i���ݒ�̏ꍇ�͌����J�E���g�̂݁j
    /// </summary>
    public int? MaxValue { get; set; }

    /// <summary>
    /// Content�v���p�e�B�̃I�[�o�[���C�h�i�O������̐ݒ���֎~�j
    /// </summary>
    public new object Content { get; private set; }

    // �J�X�^�}�C�Y�\�ȃe�L�X�g
    public string ContentText { get; set; } = "������";
    public string CancelButtonText { get; set; } = "�L�����Z��";
    public string CompletedMessage { get; set; } = "���ׂĂ̏���������ɏI�����܂����B";
    public string CanceledMessage { get; set; } = "�������L�����Z������܂����B";
    public string TimeoutMessage { get; set; } = "�������^�C���A�E�g���܂����B";

    // ���b�N�I�u�W�F�N�g
    private readonly object _lock = new();

    // �L�����Z���p
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning = false;

    // �i���X�V�̃X���b�g�����O�p
    private DateTime _lastProgressUpdate = DateTime.MinValue;
    private const int ProgressUpdateIntervalMs = 50; // 50ms�Ԋu�ōX�V����

    // �ŐV�̐i���l��ێ�
    private volatile int _latestProgressValue = 0;
    private volatile bool _hasPendingUpdate = false;
    private Timer? _progressFlushTimer;

    /// <summary>
    /// ProcessingDialog�N���X�̐V�����C���X�^���X��������
    /// </summary>
    public ProcessingDialog()
    {
        InitializeComponent();

        Content = base.Content;
        SecondaryButtonClick += OnCancelButtonClick;
    }

    /// <summary>
    /// �L�����Z���{�^���N���b�N���̏���
    /// </summary>
    private void OnCancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _cancellationTokenSource?.Cancel();

        args.Cancel = true;
    }

    /// <summary>
    /// ����������i���\���t���Ŏ��s
    /// </summary>
    /// <param name="action">���s���鏈��</param>
    public async Task RunAsync(Action<IProgress<int>> action)
    {
        await ExecuteRunAsync(tcs =>
        {
            _ = StartProcessing(() => { action(CreateProgress()); return true; }, tcs, _cancellationTokenSource!.Token);
            return Task.CompletedTask;
        }, false);
    }

    /// <summary>
    /// ����������i���\���t���Ŏ��s���A���ʂ�ԋp
    /// </summary>
    /// <typeparam name="T">�߂�l�̌^</typeparam>
    /// <param name="func">���s���鏈��</param>
    /// <returns>�����̎��s����</returns>
    public async Task<T> RunAsync<T>(Func<IProgress<int>, T> func)
    {
        return await ExecuteRunAsync<T>(tcs =>
        {
            _ = StartProcessing(() => func(CreateProgress()), tcs, _cancellationTokenSource!.Token);
            return Task.CompletedTask;
        }, false);
    }

    /// <summary>
    /// �񓯊��������L�����Z���ƃ^�C���A�E�g����t���Ŏ��s
    /// </summary>
    /// <param name="asyncAction">���s����񓯊�����</param>
    /// <param name="timeout">�^�C���A�E�g���ԁinull�̏ꍇ�͖������j</param>
    public async Task RunAsync(Func<IProgress<int>, CancellationToken, Task> asyncAction, TimeSpan? timeout = null)
    {
        await ExecuteRunAsync(tcs =>
        {
            _ = StartProcessing(async () => { await asyncAction(CreateProgress(), _cancellationTokenSource!.Token); return true; }, tcs, _cancellationTokenSource!.Token);
            return Task.CompletedTask;
        }, true, timeout);
    }

    /// <summary>
    /// �񓯊��������L�����Z���ƃ^�C���A�E�g����t���Ŏ��s���A���ʂ�ԋp
    /// </summary>
    /// <typeparam name="T">�߂�l�̌^</typeparam>
    /// <param name="asyncFunc">���s����񓯊�����</param>
    /// <param name="timeout">�^�C���A�E�g���ԁinull�̏ꍇ�͖������j</param>
    /// <returns>�����̎��s����</returns>
    public async Task<T> RunAsync<T>(Func<IProgress<int>, CancellationToken, Task<T>> asyncFunc, TimeSpan? timeout = null)
    {
        return await ExecuteRunAsync<T>(tcs =>
        {
            _ = StartProcessing(() => asyncFunc(CreateProgress(), _cancellationTokenSource!.Token), tcs, _cancellationTokenSource!.Token);
            return Task.CompletedTask;
        }, true, timeout);
    }

    /// <summary>
    /// ���s�J�n�����s�i�d�����s��h�~�j
    /// </summary>
    /// <returns>���s�J�n�ł���ꍇ��true�A���Ɏ��s���̏ꍇ��false</returns>
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
    /// ���s���������A���\�[�X���N���[���A�b�v
    /// </summary>
    private void CompleteExecution()
    {
        lock (_lock)
        {
            _isRunning = false;

            // CancellationTokenSource�̏�Ԃ��`�F�b�N���Ă�����S�ɃL�����Z��
            if (_cancellationTokenSource != null)
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // �܂��L�����Z������Ă��Ȃ��ꍇ�̂�
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }

    /// <summary>
    /// �������s�ƃ_�C�A���O�\���𓝍�����
    /// </summary>
    /// <typeparam name="T">�߂�l�̌^</typeparam>
    /// <param name="processStarter">�����J�n�f���Q�[�g</param>
    /// <param name="showCancelButton">�L�����Z���{�^����\�����邩</param>
    /// <param name="timeout">�^�C���A�E�g����</param>
    /// <returns>�����̎��s����</returns>
    private async Task<T> ExecuteRunAsync<T>(Func<TaskCompletionSource<T>, Task> processStarter, bool showCancelButton, TimeSpan? timeout = null)
    {
        if (!TryStartExecution())
        {
            if (typeof(T) == typeof(bool))
                return default!; // void�����̏ꍇ��return
            else
                throw new InvalidOperationException("���������Ɏ��s���ł��B");
        }

        // �_�C�A���O�\���i���������܂őҋ@�j
        var dialogTask = ShowAsync();

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<T>();

            // UI�ݒ�
            SecondaryButtonText = showCancelButton ? CancelButtonText : "";
            ContentProgressBar.IsIndeterminate = !MaxValue.HasValue;

            // �����J�n
            await processStarter(tcs);

            // ���������^�X�N�ƃ^�C���A�E�g����s���s
            var processTask = ExecuteWithTimeout(tcs.Task, timeout);

            // �����ƃ_�C�A���O�̊�����ҋ@
            T result = await processTask;

            // �ŏI�I�Ȑi���l���m���ɕ\��
            FlushFinalProgress();

            // �_�C�A���O���܂��J���Ă���ꍇ�̓��[�U�[�̉�����҂�
            await dialogTask;

            return result;
        }
        finally
        {
            CompleteExecution();
        }


    }

    /// <summary>
    /// �������s�ƃ_�C�A���O�\���𓝍�����i�߂�l�Ȃ��Łj
    /// </summary>
    /// <param name="processStarter">�����J�n�f���Q�[�g</param>
    /// <param name="showCancelButton">�L�����Z���{�^����\�����邩</param>
    /// <param name="timeout">�^�C���A�E�g����</param>
    private async Task ExecuteRunAsync(Func<TaskCompletionSource<bool>, Task> processStarter, bool showCancelButton, TimeSpan? timeout = null)
    {
        await ExecuteRunAsync<bool>(processStarter, showCancelButton, timeout);
    }

    /// <summary>
    /// �ۗ����̐i���X�V�𑦍��Ƀt���b�V�����čŏI�l���m���ɕ\��
    /// </summary>
    private void FlushFinalProgress()
    {
        lock (_lock)
        {
            if (_hasPendingUpdate)
            {
                // �ۗ����̍X�V�𑦍��Ɏ��s
                var finalValue = _latestProgressValue;
                _hasPendingUpdate = false;

                // �^�C�}�[���N���A
                _progressFlushTimer?.Dispose();
                _progressFlushTimer = null;

                // UI�X�V�����s
                DispatcherQueue.TryEnqueue(() => UpdateProgress(finalValue));
            }
        }
    }

    /// <summary>
    /// MaxValue�̐ݒ�ɉ����ēK�؂�Progress�C���X�^���X���쐬
    /// </summary>
    /// <returns>�p�[�Z���e�[�W�܂��̓J�E���g�\���p��Progress�C���X�^���X</returns>
    private Progress<int> CreateProgress()
    {
        return MaxValue.HasValue ?
            new Progress<int>(ThrottledProgressingPercentageRender) :
            new Progress<int>(ThrottledProgressingCountRender);
    }

    /// <summary>
    /// MaxValue�̐ݒ�ɉ����ēK�؂ȕ��@�Ői���\�����X�V
    /// </summary>
    /// <param name="value">�i���l</param>
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
    /// �p�[�Z���e�[�W�i�����X���b�g�����O�@�\�t���ōX�V
    /// </summary>
    /// <param name="value">�i���l</param>
    private void ThrottledProgressingPercentageRender(int value)
    {
        lock (_lock)
        {
            _latestProgressValue = value;
            var now = DateTime.UtcNow;

            // �X���b�g�����O�K�p
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
    /// �J�E���g�i�����X���b�g�����O�@�\�t���ōX�V
    /// </summary>
    /// <param name="value">�i���l</param>
    private void ThrottledProgressingCountRender(int value)
    {
        lock (_lock)
        {
            _latestProgressValue = value;
            var now = DateTime.UtcNow;

            // �X���b�g�����O�K�p
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
    /// �X���b�g�����O���ꂽ�i���X�V���t���b�V������^�C�}�[���m��
    /// </summary>
    private void EnsureFlushTimer()
    {
        if (_progressFlushTimer == null)
        {
            // �x���t���b�V���^�C�}�[��ݒ�i�X���b�g�����O�Ԋu��1.5�{��Ɏ��s�j
            _progressFlushTimer = new Timer(FlushPendingProgress, null,
                (int)(ProgressUpdateIntervalMs * 1.5), System.Threading.Timeout.Infinite);
        }
    }

    /// <summary>
    /// �ۗ����̐i���X�V��UI�Ƀt���b�V������^�C�}�[�R�[���o�b�N
    /// </summary>
    /// <param name="state">�^�C�}�[�R�[���o�b�N��ԁi���g�p�j</param>
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

            // �^�C�}�[���Đݒ�
            _progressFlushTimer?.Dispose();
            _progressFlushTimer = null;
        }

        if (shouldFlush)
        {
            // UI�X�V�i�ŐV�l�Łj
            UpdateProgress(valueToFlush);
        }
    }

    /// <summary>
    /// �^�X�N���^�C���A�E�g����t���Ŏ��s���A�قȂ�L�����Z���V�i���I������
    /// </summary>
    /// <typeparam name="T">�^�X�N�̖߂�l�^</typeparam>
    /// <param name="task">���s����^�X�N</param>
    /// <param name="timeout">�^�C���A�E�g���ԁinull�̏ꍇ�͖������j</param>
    /// <returns>�^�X�N�̎��s����</returns>
    private async Task<T> ExecuteWithTimeout<T>(Task<T> task, TimeSpan? timeout)
    {
        if (timeout.HasValue)
        {
            using var timeoutCts = new CancellationTokenSource(timeout.Value);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationTokenSource?.Token ?? default, timeoutCts.Token);

            try
            {
                return await task.WaitAsync(combinedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                // CancellationTokenSource�̏�Ԃ��`�F�b�N���Ă�����S�ɃL�����Z��
                if (_cancellationTokenSource != null)
                {
                    if (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        // �o�b�N�O���E���h�������L�����Z��
                        _cancellationTokenSource.Cancel();
                    }
                }
                // �^�C���A�E�g�\��
                SetTimeoutState();
                return default!;
            }
            catch (OperationCanceledException) when (_cancellationTokenSource?.Token.IsCancellationRequested == true)
            {
                // ���[�U�[�L�����Z���\���i�o�b�N�O���E���h�����͊��ɃL�����Z���ς݁j
                SetCanceledState();
                return default!;
            }
        }

        try
        {
            return await task;
        }
        catch (OperationCanceledException) when (_cancellationTokenSource?.Token.IsCancellationRequested == true)
        {
            // ���[�U�[�L�����Z���\���i�o�b�N�O���E���h�����͊��ɃL�����Z���ς݁j
            SetCanceledState();
            return default!;
        }
    }

    /// <summary>
    /// �����������o�b�N�O���E���h�Ŏ��s���A������Ԃ��Ǘ�
    /// </summary>
    /// <typeparam name="T">�߂�l�̌^</typeparam>
    /// <param name="syncFunc">���s���铯������</param>
    /// <param name="tcs">�����ʒm�p��TaskCompletionSource</param>
    /// <param name="cancellationToken">�L�����Z���g�[�N��</param>
    private async Task StartProcessing<T>(Func<T> syncFunc, TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
    {
        try
        {
            T result = await Task.Run(syncFunc, cancellationToken).ConfigureAwait(false);

            SetCompletedState();
            tcs.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            tcs.TrySetCanceled(cancellationToken);
        }
        catch (Exception ex)
        {
            SetErrorState(ex);
            tcs.TrySetException(ex);
        }
    }

    /// <summary>
    /// �񓯊����������s���A������Ԃ��Ǘ�
    /// </summary>
    /// <typeparam name="T">�߂�l�̌^</typeparam>
    /// <param name="asyncFunc">���s����񓯊�����</param>
    /// <param name="tcs">�����ʒm�p��TaskCompletionSource</param>
    /// <param name="cancellationToken">�L�����Z���g�[�N��</param>
    private async Task StartProcessing<T>(Func<Task<T>> asyncFunc, TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
    {
        try
        {
            T result = await asyncFunc().ConfigureAwait(false);

            SetCompletedState();
            tcs.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            tcs.TrySetCanceled(cancellationToken);
        }
        catch (Exception ex)
        {
            SetErrorState(ex);
            tcs.TrySetException(ex);
        }
    }

    /// <summary>
    /// �������튮������UI��Ԃ�ݒ�
    /// </summary>
    private void SetCompletedState()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // �����������ɍŏI�i�����m���ɕ\��
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
    /// ���[�U�[�L�����Z������UI��Ԃ�ݒ�
    /// </summary>
    private void SetCanceledState()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            StatusTextBlock.Text = CanceledMessage;
            PrimaryButtonText = "����";
            SecondaryButtonText = "";
            IsPrimaryButtonEnabled = true;
        });
    }

    /// <summary>
    /// �^�C���A�E�g����UI��Ԃ�ݒ�
    /// </summary>
    private void SetTimeoutState()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            StatusTextBlock.Text = TimeoutMessage;
            PrimaryButtonText = "����";
            SecondaryButtonText = "";
            IsPrimaryButtonEnabled = true;
        });
    }

    /// <summary>
    /// �G���[��������UI��Ԃ�ݒ�
    /// </summary>
    /// <param name="ex">����������O</param>
    private void SetErrorState(Exception ex)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            StatusTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            StatusTextBlock.Text = $"�G���[���������܂���:\n{ex.Message}";
            PrimaryButtonText = "����";
            SecondaryButtonText = "";
            IsPrimaryButtonEnabled = true;
        });
    }

    /// <summary>
    /// �i�����p�[�Z���e�[�W�`���ŕ\��
    /// </summary>
    /// <param name="value">���݂̐i���l</param>
    /// <exception cref="ArgumentException">MaxValue���ݒ肳��Ă��Ȃ��ꍇ</exception>
    private void ProgressingPercentageRender(int value)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (!MaxValue.HasValue)
            {
                throw new ArgumentException("MaxValue���ݒ肳��Ă��Ȃ����߁A�p�[�Z���e�[�W�\���ł��܂���B");
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
    /// �i�����J�E���g�`���ŕ\��
    /// </summary>
    /// <param name="value">���݂̃J�E���g�l</param>
    private void ProgressingCountRender(int value)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} ��", value);

            ProgressingTextBlock.Text = sb.ToString();
        });
    }

    /// <summary>
    /// ProcessingDialog���g�p���邷�ׂẴ��\�[�X�����
    /// </summary>
    public void Dispose()
    {
        // �^�C�}�[���\�[�X�����
        _progressFlushTimer?.Dispose();
        _progressFlushTimer = null;

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
}