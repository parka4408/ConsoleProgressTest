using AppTool.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using System;

namespace AppTool.ViewModels;

public partial class SpsItemViewModel : ObservableObject
{
    public TableInfo Info { get; }
    public SpsStatus Status { get; }

    [ObservableProperty]
    private SqlServerDataset? ssDataset;

    [ObservableProperty]
    private SharePointDataset? spDataset;

    [ObservableProperty]
    private bool progressIndeterminate;

    [ObservableProperty]
    private int progressValue;

    [ObservableProperty]
    private Visibility progressVisibility;

    public bool IsWarning =>
        SsDataset?.LastSynchronizedDate <= new DateTime(2024, 1, 1) ||
        SpDataset?.LastSynchronizedDate <= new DateTime(2024, 1, 1);

    partial void OnSsDatasetChanged(SqlServerDataset? oldValue, SqlServerDataset? newValue)
    {
        OnPropertyChanged(nameof(IsWarning));
    }

    partial void OnSpDatasetChanged(SharePointDataset? oldValue, SharePointDataset? newValue)
    {
        OnPropertyChanged(nameof(IsWarning));
    }

    public SpsItemViewModel(TableInfo info)
    {
        Info = info;
        Status = new SpsStatus
        {
            SucceededCount = 0,
            FailedCount = 0,
            LastUpdateCheckDate = null,
            CreatedDate = DateTime.Now,
            Status = "待機中",
        };

        ProgressValue = 0;
        ProgressVisibility = Visibility.Collapsed;
        ProgressIndeterminate = false;
    }

    public void ProgressingRender(int value)
    {
        double ratio = (double)value / 15000;
        ProgressValue = (int)(ratio * 100);
        if (ProgressValue > 0 && ProgressValue < 100)
        {
            ProgressVisibility = Visibility.Visible;
        }
        else
        {
            ProgressVisibility = Visibility.Collapsed;
        }
    }
}
