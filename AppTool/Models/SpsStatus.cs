using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AppTool.Models;

public partial class SpsStatus : ObservableObject
{
    [ObservableProperty]
    private int succeededCount;

    [ObservableProperty]
    private int failedCount;

    [ObservableProperty]
    private DateTime? lastUpdateCheckDate;

    [ObservableProperty]
    private DateTime createdDate;

    [ObservableProperty]
    private string? status;
}
