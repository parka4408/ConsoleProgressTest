namespace AppProgress
{
    /// <summary>
    /// プログレスバーのオプション
    /// </summary>
    public class ProgressBarOptions
    {
        public int BarWidth { get; set; } = 50;
        public char CompletedChar { get; set; } = '#';
        public char IncompleteChar { get; set; } = '-';
        public bool ShowCount { get; set; } = true;
        public bool ShowPercentage { get; set; } = true;
        public bool ShowElapsedTime { get; set; } = true;
        public bool ShowEstimatedTime { get; set; } = true;
    }
}