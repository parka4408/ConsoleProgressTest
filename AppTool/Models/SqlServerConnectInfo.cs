using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTool.Models;

public partial class SqlServerConnectInfo : ConnectStatus
{
    [ObservableProperty]
    private string? accessPoint;

    [ObservableProperty]
    private string? dataBase;

    public override string ServerName => "SQLServer";

    /// <summary>
    /// 環境種別をアクセスポイントから判定
    /// </summary>
    public override string EnvironmentType
    {
        get
        {
            if (string.IsNullOrEmpty(AccessPoint))
                return "未設定";

            // アクセスポイント（サーバー名）に基づいて環境を判定
            var server = AccessPoint.ToLower();
            if (server.Contains("prod") || server.Contains("production"))
                return "本番";
            if (server.Contains("test") || server.Contains("staging") || server.Contains("verify"))
                return "検証";
            if (server.Contains("dev") || server.Contains("development"))
                return "開発";

            return "不明"; // 判定不可の場合
        }
    }

    partial void OnAccessPointChanged(string? oldValue, string? newValue)
    {
        OnPropertyChanged(nameof(EnvironmentType));
        OnPropertyChanged(nameof(EnvironmentBadgeBrush));
    }
}