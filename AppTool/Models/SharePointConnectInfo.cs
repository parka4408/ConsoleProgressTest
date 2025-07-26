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
}
