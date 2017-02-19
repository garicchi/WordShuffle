using GalaSoft.MvvmLight.Messaging;
using WordShuffle.Commons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechSynthesis;
using WordShuffle.ViewModels;
using Windows.UI.Popups;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace WordShuffle.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaElement _mediaElement;
        SpeechSynthesizer _synthesizer;

        public MainPage()
        {
            this.InitializeComponent();
            //バックボタンをフックする
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            //アプリの状態が変わったとき、VisualStateManagerを変更する
            App.OnChangeAppState += (state, prev) =>
            {
                switch (state)
                {
                    case AppState.Mobile:

                        VisualStateManager.GoToState(this, "MobileState", false);
                        break;
                    case AppState.Normal:

                        VisualStateManager.GoToState(this, "NormalState", false);

                        break;
                    case AppState.Wide:
                        VisualStateManager.GoToState(this, "WideState", false);

                        break;
                }
            };
            var vm = this.DataContext as MainViewModel;
            _mediaElement = new MediaElement();
            _mediaElement.AutoPlay = true;

            var voices = SpeechSynthesizer.AllVoices.ToList();

            if (SpeechSynthesizer.AllVoices.Where(q => q.Language == "en-US").Count() > 0)
            {
                VoiceInformation voice = SpeechSynthesizer.AllVoices.Where(q => q.Language == "en-US").First();
                _synthesizer = new SpeechSynthesizer();
                _synthesizer.Voice = voice;
                vm.MainModel.CanSythesize = true;
            }
            else
            {
                vm.MainModel.CanSythesize = false;
            }

            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Messenger.Default.Register<string>(this, "Synthesize", async (str) =>
            {
                if (_synthesizer != null)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        SpeechSynthesisStream stream = await _synthesizer.SynthesizeTextToStreamAsync(str);
                        _mediaElement.SetSource(stream, stream.ContentType);
                    });
                }
            });
            Messenger.Default.Register<string>(this, "Success", async (message) =>
            {
                var dialog = new MessageDialog(message, "Information");
                await dialog.ShowAsync();
            });
            Messenger.Default.Register<string>(this, "Error", async (message) =>
            {
                var dialog = new MessageDialog(message, "エラー");
                await dialog.ShowAsync();
            });
            Messenger.Default.Register<bool>(this,"Progress",(isActive)=>
            {
                if (isActive)
                {
                    progress_main.Visibility = Visibility.Visible;
                }
                else
                {
                    progress_main.Visibility = Visibility.Collapsed;
                }
                progress_main.IsActive = isActive;
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Messenger.Default.Unregister(this);
        }

        //ページが読み込まれた時
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //HomePageにNavigateする
            NavigatePage(typeof(StudyStartPage), null);
        }

        //検索ボックスで検索しようとしたとき
        private void suggestBoxSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var text = args.QueryText;
        }

        //SplitViewのPaneに作ったナビゲーションボタンをおした時、各Pageに移動
        private void appButtonSetting_Click(object sender, RoutedEventArgs e)
        {
            NavigatePage(typeof(SettingPage), null);
        }

        private void appButtonHome_Click(object sender, RoutedEventArgs e)
        {
            NavigatePage(typeof(StudyStartPage), null);
        }

        //バックボタンが押された時
        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (frameContent.CanGoBack)
            {
                //HandledをtrueにすることでWindows10Mobileのバックボタンで終了しなくなる
                e.Handled = true;
                //コンテンツを表示しているFrameをBack
                frameContent.GoBack();
            }
        }

        private void appButtonFavorite_Click(object sender, RoutedEventArgs e)
        {
            NavigatePage(typeof(FavoritePage), null);
        }

        private void appButtonHelp_Click(object sender, RoutedEventArgs e)
        {
            NavigatePage(typeof(HelpPage), null);
        }

        private void appButtonPlaning_Click(object sender, RoutedEventArgs e)
        {
            NavigatePage(typeof(PlanningPage),null);
        }

        private void NavigatePage(Type pageType,object parameter)
        {
            frameContent.Navigate(pageType,parameter);
            if (App.StateManager.CurrentState != AppState.Wide)
            {
                splitView.IsPaneOpen = false;
            }
        }

        private void appButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigatePage(typeof(EditPage),null);
        }
    }
}
