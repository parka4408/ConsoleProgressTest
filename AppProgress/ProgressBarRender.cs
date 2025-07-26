using System.Text;

namespace AppProgress
{
    /// <summary>
    /// プログレスバーの描画を担当するクラス
    /// </summary>
    public class ProgressBarRender
    {
        private readonly ProgressBarOptions _options;
        private int _slidePosition = 0;
        private readonly int _blockSize = 5;
        private readonly object _lock = new object();

        public ProgressBarRender(ProgressBarOptions options)
        {
            _options = options;
        }

        public string Render(ProgressState state)
        {
            var sb = new StringBuilder();

            // Bar
            if (state.IsCompleted)
            {
                // バー全体を埋める
                sb.Append('[');
                sb.Append(new string(_options.CompletedChar, _options.BarWidth));
                sb.Append(']');
            }
            else if (state.ProgressRatio.HasValue)
            {
                var filled = (int)(state.ProgressRatio.Value * _options.BarWidth);
                sb.Append('[');
                sb.Append(new string(_options.CompletedChar, filled));
                sb.Append(new string(_options.IncompleteChar, _options.BarWidth - filled));
                sb.Append(']');
            }
            else
            {
                var maxPos = _options.BarWidth - _blockSize;

                // スレッドセーフな位置更新
                lock (_lock)
                {
                    // スライド位置更新（右端に行ったら左端へ戻す）
                    _slidePosition = (_slidePosition + 1) % (maxPos + 1);
                }

                sb.Append('[');
                for (int i = 0; i < _options.BarWidth; i++)
                {
                    if (i >= _slidePosition && i < _slidePosition + _blockSize)
                    {
                        sb.Append(_options.CompletedChar);
                    }
                    else
                    {
                        sb.Append(_options.IncompleteChar);
                    }
                }
                sb.Append(']');
            }

            // Count
            if (_options.ShowCount)
            {
                if (state.MaxValue.HasValue)
                {
                    sb.AppendFormat(" | {0}/{1}", (int)state.Current, state.MaxValue);
                }
                else
                {
                    sb.AppendFormat(" | {0}/{1}", (int)state.Current, state.IsCompleted ? state.Current.ToString() : "?");
                }
            }

            // Percentage
            if (_options.ShowPercentage && state.ProgressRatio.HasValue)
            {
                sb.AppendFormat(" | {0,4:##0.0}%", state.ProgressRatio.Value * 100);
            }

            // Elapsed Time
            if (_options.ShowElapsedTime)
            {
                sb.Append(" | 経過: " + state.Elapsed.ToString(@"hh\:mm\:ss"));
            }

            // Estimated Remaining
            if (_options.ShowEstimatedTime && state.EstimatedRemaining.HasValue)
            {
                sb.Append(" | ETA: " + state.EstimatedRemaining.Value.ToString(@"hh\:mm\:ss"));
            }

            return sb.ToString() + " ";
        }
    }
}