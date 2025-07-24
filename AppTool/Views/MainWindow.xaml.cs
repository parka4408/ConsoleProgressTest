using AppTool.Dialogs;
using AppTool.Models;
using AppTool.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
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
                Title = "�m�F",
                Content = "���X�g�X�V���������s���܂��B��낵���ł����H",
                PrimaryButtonText = "�͂�",
                CloseButtonText = "������",
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
                    Title = "�m�F",
                    Content = $"'{selectedItem.Info.Name}' �̓������J�n���܂��B��낵���ł����H",
                    PrimaryButtonText = "�͂�",
                    CloseButtonText = "������",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var processingdialog = new ProcessingDialog()
                    {
                        Title = "������",
                        XamlRoot = Content.XamlRoot,
                        MaxValue = 15000,
                        ProgressingContent = $"'{selectedItem.Info.Name}' �̍X�V���R�[�h�𓯊���...",
                    };

                    var logger = new ProcessingLogger();
                    int func(IProgress<int> progress) => ViewModel.TestExecute(selectedItem.Info, progress, logger);
                    var testProcesser = new DelegateProcesser<int, int>(func);

                    var totalCount = await processingdialog.RunAsync<int>(testProcesser);

                    selectedItem.Status.Status = "�X�V�ς�";
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
                    Title = "�m�F",
                    Content = $"SharePoint���X�g '{selectedItem.Info.Name}' �̃��R�[�h��S�č폜���܂��B��낵���ł����H",
                    PrimaryButtonText = "�͂�",
                    CloseButtonText = "������",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = Content.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var processingdialog = new ProcessingDialog
                    {
                        Title = "�폜��",
                        XamlRoot = Content.XamlRoot,
                        MaxValue = 15000,
                        ProgressingContent = $"SharePoint���X�g '{selectedItem.Info.Name}' �̑S���R�[�h�폜��...",
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

        private async void TrySqlServerConnectAsync(SqlServerConnectInfo connectInfo)
        {
            connectInfo.IsConnecting = true;

            SsSuccessInfoBar.Visibility = Visibility.Collapsed;
            SsSuccessInfoBar.Opacity = 1;
            SsErrorInfoBar.Visibility = Visibility.Collapsed;
            SsErrorInfoBar.Opacity = 1;
            SsConnectingInfoBar.Visibility = Visibility.Visible;
            SsConnectingInfoBar.Opacity = 1;

            SsConnectingPulseStoryboard.Begin();

            bool result = await Task.Run(() =>
            {
                Task.Delay(3000).Wait(); // �͋[�ڑ�����
                return new Random().Next(2) == 0; // ���� or ���s
            });

            SsConnectingPulseStoryboard.Stop();

            SsConnectingInfoBar.Visibility = Visibility.Collapsed;

            if (result)
            {
                SsSuccessInfoBar.Visibility = Visibility.Visible;
                SsSuccessInfoBar.Opacity = 1;

                var storyboard = (Storyboard)RootGrid.Resources["SsFadeOutStoryboard"];
                storyboard.Begin();
                storyboard.Completed += (sender, e) =>
                {
                    connectInfo.IsConnecting = false;
                };
            }
            else
            {
                SsErrorInfoBar.Visibility = Visibility.Visible;
                connectInfo.IsConnecting = false;
            }
        }

        private void SsFadeOutStoryboard_Completed(object sender, object e)
        {
            SsSuccessInfoBar.Visibility = Visibility.Collapsed;
        }

        private async void TrySharePointConnectAsync(SharePointConnectInfo connectInfo)
        {
            connectInfo.IsConnecting = true;

            SpSuccessInfoBar.Visibility = Visibility.Collapsed;
            SpSuccessInfoBar.Opacity = 1;
            SpErrorInfoBar.Visibility = Visibility.Collapsed;
            SpErrorInfoBar.Opacity = 1;
            SpConnectingInfoBar.Visibility = Visibility.Visible;
            SpConnectingInfoBar.Opacity = 1;

            SpConnectingPulseStoryboard.Begin();

            bool result = await Task.Run(() =>
            {
                Task.Delay(2000).Wait(); // �͋[�ڑ�����
                return new Random().Next(2) == 0; // ���� or ���s
            });

            SpConnectingPulseStoryboard.Stop();

            SpConnectingInfoBar.Visibility = Visibility.Collapsed;

            if (result)
            {
                SpSuccessInfoBar.Visibility = Visibility.Visible;

                var storyboard = (Storyboard)RootGrid.Resources["SpFadeOutStoryboard"];
                storyboard.Begin();
                storyboard.Completed += (sender, e) =>
                {
                    connectInfo.IsConnecting = false;
                };
            }
            else
            {
                SpErrorInfoBar.Visibility = Visibility.Visible;
                connectInfo.IsConnecting = false;
            }
        }

        private void SpFadeOutStoryboard_Completed(object sender, object e)
        {
            SpSuccessInfoBar.Visibility = Visibility.Collapsed;
        }

        private void SqlServerConnectButton_Click(object sender, RoutedEventArgs e)
        {
            TrySqlServerConnectAsync(ViewModel.SsConnectInfo);
        }

        private void SharePointConnectButton_Click(object sender, RoutedEventArgs e)
        {
            TrySharePointConnectAsync(ViewModel.SpConnectInfo);
        }
    }
}
