using System;

namespace AppProgress
{
    /// <summary>
    /// プログレスバーの状態を管理するクラス
    /// </summary>
    public class ProgressState
    {
        private readonly TimeEstimator _timeEstimator;
        public int? MaxValue { get; }
        public int Current { get; private set; }
        public bool IsCompleted { get; private set; }

        public ProgressState(int? maxValue)
        {
            if (maxValue.HasValue && maxValue < 0)
                throw new ArgumentException("MaxValue cannot be negative");

            MaxValue = maxValue;
            _timeEstimator = new TimeEstimator(DateTime.UtcNow);
        }

        public void Update(int value)
        {
            Current = value;
            if (MaxValue is int maxValue && maxValue == value)
            {
                Complete();
            }
        }

        public void Complete()
        {
            IsCompleted = true;
        }

        public TimeSpan Elapsed => _timeEstimator.Elapsed;

        public TimeSpan? EstimatedRemaining
        {
            get
            {
                if (Current > 0 && MaxValue.HasValue)
                {
                    return _timeEstimator.EstimateRemaining(Current, MaxValue.Value);
                }
                return null;
            }
        }

        public double? ProgressRatio => MaxValue.HasValue ? (double)Current / MaxValue.Value : (double?)null;
    }
}