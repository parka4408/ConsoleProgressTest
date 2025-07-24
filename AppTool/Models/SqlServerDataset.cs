using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AppTool.Models;

public partial class SqlServerDataset : ObservableObject
{
    [ObservableProperty]
    private int? recordNum;

    [ObservableProperty]
    private DateTime lastSynchronizedDate;

    [ObservableProperty]
    private DateTime createdDate;
}
