using AppTool.Dialogs;
using AppTool.Models;
using AppTool.ViewModels;
using AppTool.Views;
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
                Title = "�m�F",
                Content = "���X�g�X�V���������s���܂��B��낵���ł����H",
                PrimaryButtonText = "�͂�",
                CloseButtonText = "������",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            //if (result == ContentDialogResult.Primary)
            //{
            //    UpdateListButton.IsEnabled = false;

            //    var logger = new ProcessingLogger();
            //    await ViewModel.TestWithTasksAsync(logger);

            //    UpdateListButton.IsEnabled = true;
            //}

            var window = new LicenseViewerWindow();
            window.Activate();
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
                    using var processingdialog = new ProcessingDialog()
                    {
                        Title = "������",
                        ContentText = $"'{selectedItem.Info.Name}' �̍X�V���R�[�h�𓯊���...",
                        MaxValue = 15000,
                        XamlRoot = Content.XamlRoot,
                    };

                    var logger = new ProcessingLogger();

                    int totalCount = await processingdialog.RunAsync(progress =>
                    {
                        return ViewModel.TestExecute(selectedItem.Info, progress, logger);
                    });

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
                    //using var processingdialog = new ProcessingDialog
                    //{
                    //    Title = "�폜��",
                    //    ContentText = $"SharePoint���X�g '{selectedItem.Info.Name}' �̑S���R�[�h�폜��...",
                    //    MaxValue = 15000,
                    //    XamlRoot = Content.XamlRoot,
                    //};

                    //var logger = new ProcessingLogger();

                    //await processingdialog.RunAsync(progress =>
                    //{
                    //    ViewModel.TestExecute(selectedItem.Info, progress, logger);
                    //});

                    //using var processingdialog = new ProcessingDialog()
                    //{
                    //    XamlRoot = Content.XamlRoot,
                    //};

                    //var resultText = await ProcessingDialogTestHelper.RunTimeoutTest(processingdialog);

                    using var processingdialog = new ProcessingDialog
                    {
                        Title = "�e�X�g��",
                        XamlRoot = Content.XamlRoot,
                    };

                    //// ���������G���[�e�X�g
                    //var errorResult = await ProcessingDialogTestHelper.RunSyncErrorTest(processingdialog);

                    //// �t�@�C������G���[�e�X�g
                    //var fileResult = await ProcessingDialogTestHelper.RunSyncFileErrorTest(processingdialog);

                    await ProcessingDialogTestHelper.RunAllTests(processingdialog);
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

            // ������Storyboard���~
            var fadeOutStoryboard = (Storyboard)RootGrid.Resources["SsFadeOutStoryboard"];
            fadeOutStoryboard.Stop();

            // InfoBar�̏�Ԃ����Z�b�g
            SsSuccessInfoBar.IsOpen = true;
            SsSuccessInfoBar.Visibility = Visibility.Collapsed;
            SsSuccessInfoBar.Opacity = 1;
            SsErrorInfoBar.IsOpen = true;
            SsErrorInfoBar.Visibility = Visibility.Collapsed;
            SsErrorInfoBar.Opacity = 1;

            // �ڑ����̕\��
            SsConnectingInfoBar.IsOpen = true;
            SsConnectingInfoBar.Visibility = Visibility.Visible;
            SsConnectingInfoBar.Opacity = 1;
            SsConnectingPulseStoryboard.Begin();

            bool result = await Task.Run(() =>
            {
                Task.Delay(1000).Wait(); // �͋[�ڑ�����
                //return new Random().Next(2) == 0; // ���� or ���s
                return true;
            });

            SsConnectingPulseStoryboard.Stop();
            SsConnectingInfoBar.Visibility = Visibility.Collapsed;

            connectInfo.IsConnecting = false;

            if (result)
            {
                // �������̕\��
                SsSuccessInfoBar.Visibility = Visibility.Visible;

                fadeOutStoryboard.Begin();

                connectInfo.IsConnected = true;
                connectInfo.IsConnectionFailed = false;
                Debug.WriteLine("SQLServer�ڑ�����");
            }
            else
            {
                // ���s���̕\��
                SsErrorInfoBar.Visibility = Visibility.Visible;

                connectInfo.IsConnected = false;
                connectInfo.IsConnectionFailed = true;
                Debug.WriteLine("SQLServer�ڑ����s");
            }
        }

        private void SsFadeOutStoryboard_Completed(object sender, object e)
        {
            SsSuccessInfoBar.Visibility = Visibility.Collapsed;
        }

        private async void TrySharePointConnectAsync(SharePointConnectInfo connectInfo)
        {
            connectInfo.IsConnecting = true;

            // ������Storyboard���~
            var fadeOutStoryboard = (Storyboard)RootGrid.Resources["SpFadeOutStoryboard"];
            fadeOutStoryboard.Stop();

            // InfoBar�̏�Ԃ����Z�b�g
            SpSuccessInfoBar.IsOpen = true;
            SpSuccessInfoBar.Visibility = Visibility.Collapsed;
            SpSuccessInfoBar.Opacity = 1;
            SpErrorInfoBar.IsOpen = true;
            SpErrorInfoBar.Visibility = Visibility.Collapsed;
            SpErrorInfoBar.Opacity = 1;

            // �ڑ����̕\��
            SpConnectingInfoBar.IsOpen = true;
            SpConnectingInfoBar.Visibility = Visibility.Visible;
            SpConnectingInfoBar.Opacity = 1;
            SpConnectingPulseStoryboard.Begin();

            bool result = await Task.Run(() =>
            {
                Task.Delay(1000).Wait(); // �͋[�ڑ�����
                //return new Random().Next(2) == 0; // ���� or ���s
                return true;
            });

            SpConnectingPulseStoryboard.Stop();
            SpConnectingInfoBar.Visibility = Visibility.Collapsed;

            connectInfo.IsConnecting = false;

            if (result)
            {
                // �������̕\��
                SpSuccessInfoBar.Visibility = Visibility.Visible;

                fadeOutStoryboard.Begin();

                connectInfo.IsConnected = true;
                connectInfo.IsConnectionFailed = false;
                Debug.WriteLine("SQLServer�ڑ�����");
            }
            else
            {
                // ���s���̕\��
                SpErrorInfoBar.Visibility = Visibility.Visible;

                connectInfo.IsConnected = false;
                connectInfo.IsConnectionFailed = true;
                Debug.WriteLine("SQLServer�ڑ����s");
            }
        }

        private void SpFadeOutStoryboard_Completed(object sender, object e)
        {
            SpSuccessInfoBar.Visibility = Visibility.Collapsed;
        }
    }
}
