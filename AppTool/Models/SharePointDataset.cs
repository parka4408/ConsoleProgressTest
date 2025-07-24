using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Data;

namespace AppTool.Models;

public partial class SharePointDataset : ObservableObject
{
    [ObservableProperty]
    private int? recordNum;

    [ObservableProperty]
    private DateTime lastSynchronizedDate;

    [ObservableProperty]
    private DateTime createdDate;
}
