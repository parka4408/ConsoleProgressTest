using AppTool.Dialogs;
using AppTool.Models;
using AppTool.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics;

namespace AppTool
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel
        {
            get;
        }

        public MainWindow()
        {
            ViewModel = new MainViewModel();
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;

            AppWindow.ResizeClient(new SizeInt32(
                (int)(1280),
                (int)(720)));

            TrySqlServerConnectAsync(ViewModel.SsConnectInfo);
            TrySharePointConnectAsync(ViewModel.SpConnectInfo);
        }

        private async void UpdateListButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "確認",
                Content = "リスト更新処理を実行します。よろしいですか？",
                PrimaryButtonText = "はい",
                CloseButtonText = "いいえ",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                UpdateListButton.IsEnabled = false;

                var logger = new ProcessingLogger();
                await ViewModel.TestWithTasksAsync(logger);

                UpdateListButton.IsEnabled = true;
            }
        }

        private async void SynchronizeUpdatedRecordsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is SpsItemViewModel selectedItem)
            {
                var dialog = new ContentDialog
                {
                    Title = "確認",
                    Content = $"'{selectedItem.Info.Name}' の同期を開始します。よろしいですか？",
                    PrimaryButtonText = "はい",
                    CloseButtonText = "いいえ",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var processingdialog = new ProcessingDialog()
                    {
                        Title = "同期中",
                        XamlRoot = Content.XamlRoot,
                        MaxValue = 15000,
                        ContentText = $"'{selectedItem.Info.Name}' の更新レコードを同期中...",
                    };

                    var logger = new ProcessingLogger();
                    int func(IProgress<int> progress) => ViewModel.TestExecute(selectedItem.Info, progress, logger);
                    var testProcesser = new DelegateProcesser<int, int>(func);

                    var totalCount = await processingdialog.RunAsync<int>(testProcesser);

                    selectedItem.Status.Status = "更新済み";
                    selectedItem.Status.CreatedDate = DateTime.Now;
                    selectedItem.Status.LastUpdateCheckDate = DateTime.Now;
                    selectedItem.SsDataset = new SqlServerDataset()
                    {
                        CreatedDate = DateTime.Now,
                        RecordNum = totalCount,
                        LastSynchronizedDate = DateTime.Now,
                    };
                    selectedItem.SpDataset = new SharePointDataset()
                    {
                        CreatedDate = DateTime.Now,
                        RecordNum = totalCount,
                        LastSynchronizedDate = DateTime.Now,
                    };
                }
            }
        }

        private async void DeleteSharePointListRecordsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem is SpsItemViewModel selectedItem)
            {
                var dialog = new ContentDialog
                {
                    Title = "確認",
                    Content = $"SharePointリスト '{selectedItem.Info.Name}' のレコードを全て削除します。よろしいですか？",
                    PrimaryButtonText = "はい",
                    CloseButtonText = "いいえ",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var processingdialog = new ProcessingDialog
                    {
                        Title = "削除中",
                        XamlRoot = Content.XamlRoot,
                        MaxValue = 15000,
                        ContentText = $"SharePointリスト '{selectedItem.Info.Name}' の全レコード削除中...",
                    };

                    var logger = new ProcessingLogger();
                    int func(IProgress<int> progress) => ViewModel.TestExecute(selectedItem.Info, progress, logger);
                    var testProcesser = new DelegateProcesser<int, int>(func);

                    await processingdialog.RunAsync<int>(testProcesser);
                }
            }
        }

        private void SqlServerConnectInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SqlServerConnectTeachingTip.IsOpen = true;
        }

        private void SharePointConnectInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SharePointConnectTeachingTip.IsOpen = true;
        }

        private void SqlServerConnectButton_Click(object sender, RoutedEventArgs e)
        {
            TrySqlServerConnectAsync(ViewModel.SsConnectInfo);
        }

        private void SharePointConnectButton_Click(object sender, RoutedEventArgs e)
        {
            TrySharePointConnectAsync(ViewModel.SpConnectInfo);
        }

        private async void TrySqlServerConnectAsync(SqlServerConnectInfo connectInfo)
        {
            connectInfo.IsConnecting = true;

            // 既存のStoryboardを停止
            var fadeOutStoryboard = (Storyboard)RootGrid.Resources["SsFadeOutStoryboard"];
            fadeOutStoryboard.Stop();

            // InfoBarの状態をリセット
            SsSuccessInfoBar.IsOpen = true;
            SsSuccessInfoBar.Visibility = Visibility.Collapsed;
            SsSuccessInfoBar.Opacity = 1;
            SsErrorInfoBar.IsOpen = true;
            SsErrorInfoBar.Visibility = Visibility.Collapsed;
            SsErrorInfoBar.Opacity = 1;

            // 接続中の表示
            SsConnectingInfoBar.IsOpen = true;
            SsConnectingInfoBar.Visibility = Visibility.Visible;
            SsConnectingInfoBar.Opacity = 1;
            SsConnectingPulseStoryboard.Begin();

            bool result = await Task.Run(() =>
            {
                Task.Delay(3000).Wait(); // 模擬接続処理
                return new Random().Next(2) == 0; // 成功 or 失敗
            });

            SsConnectingPulseStoryboard.Stop();
            SsConnectingInfoBar.Visibility = Visibility.Collapsed;

            connectInfo.IsConnecting = false;

            if (result)
            {
                // 成功時の表示
                SsSuccessInfoBar.Visibility = Visibility.Visible;

                fadeOutStoryboard.Begin();

                connectInfo.IsConnected = true;
                connectInfo.IsConnectionFailed = false;
                Debug.WriteLine("SQLServer接続成功");
            }
            else
            {
                // 失敗時の表示
                SsErrorInfoBar.Visibility = Visibility.Visible;

                connectInfo.IsConnected = false;
                connectInfo.IsConnectionFailed = true;
                Debug.WriteLine("SQLServer接続失敗");
            }
        }

        private void SsFadeOutStoryboard_Completed(object sender, object e)
        {
            SsSuccessInfoBar.Visibility = Visibility.Collapsed;
        }

        private async void TrySharePointConnectAsync(SharePointConnectInfo connectInfo)
        {
            connectInfo.IsConnecting = true;

            // 既存のStoryboardを停止
            var fadeOutStoryboard = (Storyboard)RootGrid.Resources["SpFadeOutStoryboard"];
            fadeOutStoryboard.Stop();

            // InfoBarの状態をリセット
            SpSuccessInfoBar.IsOpen = true;
            SpSuccessInfoBar.Visibility = Visibility.Collapsed;
            SpSuccessInfoBar.Opacity = 1;
            SpErrorInfoBar.IsOpen = true;
            SpErrorInfoBar.Visibility = Visibility.Collapsed;
            SpErrorInfoBar.Opacity = 1;

            // 接続中の表示
            SpConnectingInfoBar.IsOpen = true;
            SpConnectingInfoBar.Visibility = Visibility.Visible;
            SpConnectingInfoBar.Opacity = 1;
            SpConnectingPulseStoryboard.Begin();

            bool result = await Task.Run(() =>
            {
                Task.Delay(3000).Wait(); // 模擬接続処理
                return new Random().Next(2) == 0; // 成功 or 失敗
            });

            SpConnectingPulseStoryboard.Stop();
            SpConnectingInfoBar.Visibility = Visibility.Collapsed;

            connectInfo.IsConnecting = false;

            if (result)
            {
                // 成功時の表示
                SpSuccessInfoBar.Visibility = Visibility.Visible;

                fadeOutStoryboard.Begin();

                connectInfo.IsConnected = true;
                connectInfo.IsConnectionFailed = false;
                Debug.WriteLine("SQLServer接続成功");
            }
            else
            {
                // 失敗時の表示
                SpErrorInfoBar.Visibility = Visibility.Visible;

                connectInfo.IsConnected = false;
                connectInfo.IsConnectionFailed = true;
                Debug.WriteLine("SQLServer接続失敗");
            }
        }

        private void SpFadeOutStoryboard_Completed(object sender, object e)
        {
            SpSuccessInfoBar.Visibility = Visibility.Collapsed;
        }
    }
}
