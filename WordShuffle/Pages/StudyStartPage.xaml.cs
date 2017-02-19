using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WordShuffle.Models;
using WordShuffle.ViewModels;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace WordShuffle.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class StudyStartPage : Page
    {
        public StudyStartPage()
        {
            this.InitializeComponent();
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Messenger.Default.Register<bool>(this, "FileLoadResult", async (isOK) =>
            {
                if (isOK)
                {
                    this.Frame.Navigate(typeof(StudyPage));
                }
                else
                {
                    var dialog = new MessageDialog("ファイルを読み込めませんでした。ファイルのフォーマットが間違っている可能性があります。", "エラー");
                    await dialog.ShowAsync();
                }
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Messenger.Default.Unregister(this);
        }

        private void button_startStudy_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm.PickFileCommand.Execute(null);
        }

    }
}
