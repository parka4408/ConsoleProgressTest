using Interfaces;
using System;

namespace AppProgress
{
    /// <summary>
    /// プログレスバークラス
    /// </summary>
    public class ProgressReporter : IProgress<int>, IDisposable
    {
        private readonly ProgressState _state;
        private readonly ProgressBarOptions _options;
        private readonly ProgressBarRender _render;
        private readonly IConsoleWriter _logger;
        private bool _disposed;

        public ProgressReporter(int? maxValue = null, ProgressBarOptions options = null, IConsoleWriter logger = null)
        {
            _state = new ProgressState(maxValue);
            _options = options ?? new ProgressBarOptions();
            _render = new ProgressBarRender(_options);
            _logger = logger ?? new SynchronizedProgressLogger();
        }

        public void Report(int value)
        {
            if (_disposed) return;

            _state.Update(value);
            _logger.Write("\r" + _render.Render(_state));
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (!_state.MaxValue.HasValue)
            {
                _state.Complete();
                _logger.Write("\r" + _render.Render(_state));
            }
            _logger.WriteLine();

            _disposed = true;
        }
    }
}
