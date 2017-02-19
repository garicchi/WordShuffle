using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using WordShuffle.ViewModels;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace WordShuffle.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class ResultPage : Page
    {
        public ResultPage()
        {
            this.InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Messenger.Default.Register<bool>(this, "FileSaving", (bln) =>
            {
                if (bln)
                {
                    text_saveStatus.Text = "ファイルを保存しています...";
                }
                else
                {
                    text_saveStatus.Text = "ファイルを保存完了しました";
                }
            });

            Messenger.Default.Register<string>(this, "ShareResult", (message) =>
            {
                DataTransferManager manager = DataTransferManager.GetForCurrentView();
                manager.DataRequested += async (s, ex) =>
                {
                    DataRequest request = ex.Request;
                    request.Data.Properties.Title = "学習結果";
                    request.Data.Properties.Description = "WordShuffleの学習結果を共有します";
                    request.Data.SetText(message);

                    RenderTargetBitmap rtb = new RenderTargetBitmap();
                    await rtb.RenderAsync(stack_result);

                    StorageFile tempFile = await KnownFolders.PicturesLibrary.CreateFileAsync("WordShuffleShareImage.jpg", CreationCollisionOption.ReplaceExisting);
                    using (var stream = await tempFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                        IBuffer pixelBuffer = await rtb.GetPixelsAsync();
                        byte[] pixels = pixelBuffer.ToArray();

                        float dpi = DisplayInformation.GetForCurrentView().LogicalDpi;

                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                            (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, dpi, dpi, pixels);
                        await encoder.FlushAsync();

                        request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(tempFile));

                    }
                };
                flyout_share.ShowAt(button_share);


            });


            var vm = DataContext as MainViewModel;
            vm.SaveLearningResultCommand.Execute(null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Messenger.Default.Unregister(this);
        }

        private void button_backTitle_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StudyStartPage));
        }

        private void button_repeat_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm.RepeatStudyCommand.Execute(null);
            this.Frame.Navigate(typeof(StudyPage));
        }

        private async void listBox_noRemember_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_noRemember.SelectedIndex != -1)
            {
                var dialog = new MessageDialog("お気に入りに登録しますか？", "確認");
                dialog.Commands.Add(new UICommand("OK", arg =>
                {
                    var vm = DataContext as MainViewModel;
                    var item = vm.MainModel.TopNoRememberList.ElementAt(listBox_noRemember.SelectedIndex);
                    vm.AddFavoriteWithItemCommand.Execute(item);
                }));
                dialog.Commands.Add(new UICommand("Cancel", arg => { }));

                await dialog.ShowAsync();
                
                
            }
        }

        private void button_share_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm.ShareResultCommand.Execute(null);
        }

        private void button_shareOK_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
            flyout_share.Hide();
        }
    }
}
