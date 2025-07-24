using AppTool.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppTool.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<SpsItemViewModel> Items { get; } = [];

    [ObservableProperty]
    private SpsItemViewModel? selectedItem;

    [ObservableProperty]
    private SqlServerConnectInfo ssConnectInfo;

    [ObservableProperty]
    private SharePointConnectInfo spConnectInfo;

    public MainViewModel()
    {
        ssConnectInfo = new SqlServerConnectInfo()
        {
            AccessPoint = "????\\????",
            DataBase = "DEV"
        };

        spConnectInfo = new SharePointConnectInfo()
        {
            SiteUrl = "http://www.microsoft.com",
            UserName = "????",
            Password = "????",
        };

        Items.CollectionChanged += Items_CollectionChanged;

        var sampleItems = new TableInfo[]
        {
            new("Test1", string.Concat(Enumerable.Repeat("Description1", 100)), "Category1"),
            new("Test2", string.Concat(Enumerable.Repeat("Description2", 1)), "Category2"),
            new("Test3", string.Concat(Enumerable.Repeat("Description3", 1)), "Category3"),
            new("Test4", string.Concat(Enumerable.Repeat("Description4", 1)), "Category4"),
            new("Test5", string.Concat(Enumerable.Repeat("Description5", 1)), "Category5"),
            new("Test6", string.Concat(Enumerable.Repeat("Description6", 1)), "Category6"),
            new("Test7", string.Concat(Enumerable.Repeat("Description7", 1)), "Category7"),
            new("Test8", string.Concat(Enumerable.Repeat("Description8", 1)), "Category8"),
            new("Test9", string.Concat(Enumerable.Repeat("Description9", 1)), "Category9"),
            new("Test10", string.Concat(Enumerable.Repeat("Description10", 1)), "Category10"),
            new("Test11", string.Concat(Enumerable.Repeat("Description11", 1)), "Category11"),
            new("Test12", string.Concat(Enumerable.Repeat("Description12", 1)), "Category12"),
            new("Test13", string.Concat(Enumerable.Repeat("Description13", 1)), "Category13"),
            new("Test14", string.Concat(Enumerable.Repeat("Description14", 1)), "Category14"),
            new("Test15", string.Concat(Enumerable.Repeat("Description15", 1)), "Category15"),
        };

        foreach (var item in sampleItems)
        {
            Items.Add(new SpsItemViewModel(item));
        }
    }

    private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (SpsItemViewModel item in e.NewItems)
            {
                item.Status.PropertyChanged += Item_PropertyChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (SpsItemViewModel item in e.OldItems)
            {
                item.Status.PropertyChanged -= Item_PropertyChanged;
            }
        }
        OnPropertyChanged(nameof(CanUpdateList));
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SpsItemViewModel.Status.CreatedDate))
        {
            OnPropertyChanged(nameof(CanSynchronizeUpdatedRecords));
        }
    }

    partial void OnSelectedItemChanged(SpsItemViewModel? oldValue, SpsItemViewModel? newValue)
    {
        CloseCommand.NotifyCanExecuteChanged();
        OpenSpOutputDirCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsItemSelected));
        OnPropertyChanged(nameof(IsItemNonSelected));
        OnPropertyChanged(nameof(IsDispSqlServerDataset));
        OnPropertyChanged(nameof(IsDispSharePointDataset));
        OnPropertyChanged(nameof(CanDeleteSharePointList));
        OnPropertyChanged(nameof(CanSynchronizeUpdatedRecords));
    }

    public bool IsItemSelected => SelectedItem != null;

    public bool IsItemNonSelected => SelectedItem == null;

    public bool IsDispSqlServerDataset => SelectedItem?.SsDataset != null;

    public bool IsDispSharePointDataset => SelectedItem?.SpDataset != null;

    public bool CanUpdateList => Items.Any();

    public bool CanDeleteSharePointList => SelectedItem != null;

    public bool CanSynchronizeUpdatedRecords => SelectedItem != null && SelectedItem?.IsWarning == true;

    [RelayCommand(CanExecute = nameof(IsItemSelected))]
    public void OnClose()
    {
        SelectedItem = null;
    }

    [RelayCommand(CanExecute = nameof(IsItemSelected))]
    public void OnOpenSpOutputDir()
    {
        Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
    }

    public int TestExecute(TableInfo tableInfo, IProgress<int>? progress = null, ILogger? logger = null)
    {
        var count = 0;

        var totalCount = 15000;
        var threadNum = Items.Count;
        var batchNum = totalCount / threadNum;
        var errorBorder = 50;

        Parallel.For(0, threadNum, i =>
        {
            for (int j = 0; j < batchNum; j++)
            {
                int current = Interlocked.Increment(ref count);
                if (current % errorBorder == 0)
                {
                    logger?.Error($"スレッド{i}: エラー発生1");
                }
                else if (current % (errorBorder * 1.5) == 0)
                {
                    logger?.Error($"スレッド{i}: エラー発生2");
                }

                if (current % 10 == 0)
                {
                    progress?.Report(current);
                }
                Thread.Sleep(10);
            }
        });

        return count;
    }

    public async Task TestWithTasksAsync(ILogger ? logger = null)
    {
        int maxConcurrency = 5;
        int processedCount = 0;
        int errorCount = 0;
        int totalCount = 0;

        using var semaphore = new SemaphoreSlim(maxConcurrency);

        var tasks = Items.Select((Func<SpsItemViewModel, Task>)(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                var resultCount = 0;
                var progress = new Progress<int>(item.ProgressingRender);
                item.Status.Status = "処理中";

                Interlocked.Increment(ref processedCount);
                await Task.Run(() =>
                {
                    resultCount = TestExecute(item.Info, progress, logger);
                });
                totalCount += resultCount;

                item.Status.Status = "更新済み";
                item.Status.LastUpdateCheckDate = DateTime.Now;
                item.SsDataset = new SqlServerDataset()
                {
                    CreatedDate = DateTime.Now,
                    RecordNum = resultCount,
                    LastSynchronizedDate = processedCount % maxConcurrency == 0 ? new DateTime(2024, 1, 1) : DateTime.Now,
                };
                item.SpDataset = new SharePointDataset()
                {
                    CreatedDate = DateTime.Now,
                    RecordNum = resultCount,
                    LastSynchronizedDate = processedCount % maxConcurrency == 0 ? new DateTime(2024, 1, 1) : DateTime.Now,
                };
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref errorCount);
                logger?.Error($"アイテム '{item.Info.Name}' の処理でエラー: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }));

        await Task.WhenAll(tasks);

        logger?.Warning($"処理完了（{totalCount} 件）: スレッド合計 {processedCount} 件中 {errorCount} 件でエラーが発生しました。");
    }
}