using Interfaces;
using System;

namespace AppProgress
{
    /// <summary>
    /// プログレスバークラス
    /// </summary>
    public class ProgressConsoleReporter : IDisposable
    {
        private readonly ProgressState _state;
        private readonly ProgressBarOptions _options;
        private readonly ProgressBarRender _render;
        private readonly IConsoleWriter _logger;
        private readonly SynchronizedProgress _progress;
        private bool _disposed;

        public IProgress<int> Progress => _progress;

        public ProgressConsoleReporter(IConsoleWriter logger, int? maxValue = null, ProgressBarOptions options = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _state = new ProgressState(maxValue);
            _options = options ?? new ProgressBarOptions();
            _render = new ProgressBarRender(_options);
            
            _progress = new SynchronizedProgress(this);
        }

        internal void ProgressingReport(int value)
        {
            if (_disposed) return;
            
            _state.Update(value);
            var progressText = _render.Render(_state);
            
            _logger.Write("\r" + progressText);
        }

        public void Dispose()
        {
            if (_disposed) return;

            // 最終状態を表示して確定
            if (!_state.MaxValue.HasValue)
            {
                _state.Complete();
            }
            
            var finalProgress = _render.Render(_state);
            _logger.Write("\r" + finalProgress);
            _logger.WriteLine("");

            _disposed = true;
        }

        private class SynchronizedProgress : IProgress<int>
        {
            private readonly ProgressConsoleReporter _parent;

            public SynchronizedProgress(ProgressConsoleReporter parent)
            {
                _parent = parent;
            }

            public void Report(int value)
            {
                _parent.ProgressingReport(value);
            }
        }
    }
}
