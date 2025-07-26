using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTool.Models;

public partial class SqlServerConnectInfo : ConnectStatus
{
    [ObservableProperty]
    private string? accessPoint;

    [ObservableProperty]
    private string? dataBase;

    public override string ServerName => "SQLServer";
}