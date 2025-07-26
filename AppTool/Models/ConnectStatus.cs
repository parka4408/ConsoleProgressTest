using AppTool.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AppTool.Models;

public abstract partial class ConnectStatus : ObservableObject
{
    [ObservableProperty]
    private bool isConnecting;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private bool isConnectionFailed;

    public abstract string ServerName { get; }

    public bool IsTryConnect => !IsConnecting;

    /// <summary>
    /// 接続状態のテキスト表示
    /// </summary>
    public string ConnectionStatusText
    {
        get
        {
            if (IsConnecting) return "接続中...";
            if (IsConnected) return "接続済み";
            if (IsConnectionFailed) return "接続失敗";
            return "未接続";
        }
    }

    /// <summary>
    /// ステータスインジケーターの色
    /// </summary>
    public Brush StatusIndicatorBrush
    {
        get
        {
            if (IsConnecting)
                return new SolidColorBrush(Color.FromArgb(255, 255, 193, 7)); // 黄色
            if (IsConnected)
                return new SolidColorBrush(Color.FromArgb(255, 76, 175, 80)); // 緑色
            if (IsConnectionFailed)
                return new SolidColorBrush(Color.FromArgb(255, 244, 67, 54)); // 赤色

            return new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)); // グレー
        }
    }

    partial void OnIsConnectingChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(IsTryConnect));
        OnPropertyChanged(nameof(ConnectionStatusText));
        OnPropertyChanged(nameof(StatusIndicatorBrush));
    }

    partial void OnIsConnectedChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(ConnectionStatusText));
        OnPropertyChanged(nameof(StatusIndicatorBrush));
    }

    partial void OnIsConnectionFailedChanged(bool oldValue, bool newValue)
    {
        OnPropertyChanged(nameof(ConnectionStatusText));
        OnPropertyChanged(nameof(StatusIndicatorBrush));
    }
}
