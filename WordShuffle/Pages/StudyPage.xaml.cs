using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using WordShuffle.Commons;
using WordShuffle.ViewModels;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace WordShuffle.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class StudyPage : Page
    {
        
        public StudyPage()
        {
            this.InitializeComponent();

            var vm = DataContext as MainViewModel;
            if (!vm.MainModel.CanSythesize)
            {
                this.button_Synthesize.IsEnabled = false;
                this.button_Synthesize.Label = "利用できません";
            }

            this.webView_flyout.NavigationStarting += (s, e) =>
            {
                this.progress_browser.IsActive = true;
            };
            this.webView_flyout.NavigationFailed += (s, e) =>
            {
                this.progress_browser.IsActive = false;
            };

            this.webView_flyout.NavigationCompleted += (s, e) =>
            {
                this.progress_browser.IsActive = false;
            };
            
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            var vm = DataContext as MainViewModel;
            vm.StartStudyCommand.Execute(null);

            Messenger.Default.Register<bool>(this, "GoResult", (bln) =>
            {
                this.Frame.Navigate(typeof(ResultPage));
            });

            Messenger.Default.Register<string>(this, "ShowBrowser", (url) =>
            {
                var requestMessage = new HttpRequestMessage();
                requestMessage.RequestUri = new Uri(url);
                requestMessage.Headers.Add("user-agent", "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; MCJ; MouseComputer MADOSMA Q501) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/13.10586");
                this.webView_flyout.NavigateWithHttpRequestMessage(requestMessage);
                this.flyout_browser.ShowAt(this);

            });
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            Messenger.Default.Unregister(this);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            
        }

        private async void button_browser_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(webView_flyout.Source);
        }

        private void button_closeFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.flyout_browser.Hide();
        }
    }
}
