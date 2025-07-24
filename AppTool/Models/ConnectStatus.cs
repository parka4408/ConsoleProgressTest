using AppTool.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppTool.Models;

public abstract partial class ConnectStatus : ObservableObject
{
    [ObservableProperty]
    private bool isConnecting;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private bool isConnectionFailed;

    public bool IsTryConnect => !IsConnecting;

    partial void OnIsConnectingChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(IsTryConnect));
    }
}
