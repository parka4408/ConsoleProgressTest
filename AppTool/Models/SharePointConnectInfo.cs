using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTool.Models;

public partial class SharePointConnectInfo : ConnectStatus
{
    [ObservableProperty]
    private string? siteUrl;

    [ObservableProperty]
    private string? userName;

    [ObservableProperty]
    private string? password;

    public override string ServerName => "SharePoint";

    /// <summary>
    /// 環境種別を URL から判定
    /// </summary>
    public override string EnvironmentType
    {
        get
        {
            if (string.IsNullOrEmpty(SiteUrl))
                return "未設定";

            // URL に基づいて環境を判定
            var url = SiteUrl.ToLower();
            if (url.Contains("prod") || url.Contains("production"))
                return "本番";
            if (url.Contains("test") || url.Contains("staging") || url.Contains("verify"))
                return "検証";
            if (url.Contains("dev") || url.Contains("development"))
                return "開発";

            return "不明"; // 判定不可の場合
        }
    }

    partial void OnSiteUrlChanged(string? oldValue, string? newValue)
    {
        OnPropertyChanged(nameof(EnvironmentType));
        OnPropertyChanged(nameof(EnvironmentBadgeBrush));
    }
}
