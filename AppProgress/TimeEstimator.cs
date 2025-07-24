using System;

namespace AppProgress
{
    public class TimeEstimator
    {
        private readonly DateTime _start;

        public TimeEstimator(DateTime? start = null)
        {
            _start = start ?? DateTime.UtcNow;
        }

        public TimeSpan Elapsed => DateTime.UtcNow - _start;

        public TimeSpan? EstimateRemaining(int current, int max)
        {
            if (current == 0 || max == 0) return null;
            double rate = (double)current / max;
            double totalSeconds = Elapsed.TotalSeconds / rate;
            return TimeSpan.FromSeconds(totalSeconds - Elapsed.TotalSeconds);
        }
    }
}
