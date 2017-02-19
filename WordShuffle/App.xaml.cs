using Microsoft.Practices.ServiceLocation;
using WordShuffle.Commons;
using WordShuffle.Models;
using WordShuffle.Pages;
using WordShuffle.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechSynthesis;
using Windows.UI;
using Windows.UI.Notifications;
using NotificationsExtensions.Toasts;
using Windows.UI.Popups;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Windows.ApplicationModel.VoiceCommands;

namespace WordShuffle
{
    sealed partial class App : Application
    {
        //アプリの状態を管理するManager
        public static AppStateManager StateManager { get; set; }
        //アプリの状態が変わったときに変更を通知するイベント
        public static event Action<AppState, AppState> OnChangeAppState;

        public static CoreDispatcher AppDispatcher
        {
            get
            {
                return Window.Current.Dispatcher;
            }
        }

        //アプリの最小幅を定義する
        private Size _appMinimumSize = new Size(340, 400);

        public App()
        {
            this.InitializeComponent();

            StateManager = new AppStateManager();
            //アプリの状態とその最小幅を定義する
            StateManager.StateList.Add(AppState.Mobile, 0);
            StateManager.StateList.Add(AppState.Normal, 600);
            StateManager.StateList.Add(AppState.Wide, 1200);

            OnChangeAppState += (s, s2) => { };

            //アプリのライフサイクルをフック
            App.Current.Resuming += App_Resuming;
            App.Current.Suspending += App_Suspending;
            
        }

        //アプリが起動したとき
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            await ActivateWindowAsync(e,typeof(MainPage));
            
        }

        private async Task ActivateWindowAsync(IActivatedEventArgs e,Type page)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                //以前のアプリ状態が中断で終了した場合
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //以前中断したアプリケーションから状態を読み込む
                }
                
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                /*
                var themeBrush = Resources["SplitViewBackgroundBrush"] as SolidColorBrush;
                var view = ApplicationView.GetForCurrentView();
                view.TitleBar.BackgroundColor = themeBrush.Color;
                view.TitleBar.ButtonForegroundColor = Colors.White;
                view.TitleBar.ButtonBackgroundColor = themeBrush.Color;
                */
                //アプリの最小幅を設定
                ApplicationView.GetForCurrentView().SetPreferredMinSize(_appMinimumSize);
                //ウインドウのサイズ変更がされたとき
                Window.Current.SizeChanged += (s, ex) =>
                {
                    OnWindowSizeChanged(ex.Size);
                };
                //MainPageへNavigate
                rootFrame.Navigate(page);

                OnWindowSizeChanged(new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height));
            }

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("Initialize"))
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/VcdFile.xml"));
                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(file);
                ApplicationData.Current.LocalSettings.Values["Initialize"] = true;
            }

            await LoadAllDataAsync();
            Window.Current.Activate();
            
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            if(args.Kind == ActivationKind.ToastNotification||args.Kind == ActivationKind.VoiceCommand)
            {
                await ActivateWindowAsync(args,typeof(MainPage));
            }
            
        }

        //ウインドウサイズが変更されたとき、それに応じてアプリの状態を変える
        private void OnWindowSizeChanged(Size newSize)
        {
            var prevState = App.StateManager.CurrentState;
            bool isChange = StateManager.TryChangeState(newSize.Width);
            if (isChange)
            {
                switch (StateManager.CurrentState)
                {
                    case AppState.Mobile:
                        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                        break;
                    case AppState.Normal:
                    case AppState.Wide:
                        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                        break;
                }
                OnChangeAppState(StateManager.CurrentState, prevState);
            }
        }

        //アプリが一時停止しようとしたとき
        private async void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            
            var deferral = e.SuspendingOperation.GetDeferral();
            
            //ここでアプリのデータや状態を保存するdeferral.Complete()が呼ばれるまではawaitしても待ってくれる
            var model = ServiceLocator.Current.GetInstance<MainViewModel>().MainModel;
            if (model.Settings.NotifyStudyRange != StudyRangeType.None)
            {
                var toast = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        TitleText = new ToastText()
                        {
                            Text = "WordShuffleで暗記しませんか？"
                        },
                        BodyTextLine1 = new ToastText()
                        {
                            Text = "継続して英単語を暗記しましょう！"
                        }
                    }
                };
                var notifir = ToastNotificationManager.CreateToastNotifier();
                var schdules = notifir.GetScheduledToastNotifications();
                foreach(var notify in schdules)
                {
                    notifir.RemoveFromSchedule(notify);
                }

                TimeSpan nextTime = TimeSpan.Zero;
                switch (model.Settings.NotifyStudyRange)
                {
                    case StudyRangeType.OneDay:
                        nextTime = TimeSpan.FromDays(1);
                        break;
                    case StudyRangeType.ThreeDays:
                        nextTime = TimeSpan.FromDays(3);
                        break;
                    case StudyRangeType.OneWeek:
                        nextTime = TimeSpan.FromDays(7);
                        break;
                    case StudyRangeType.TwoWeeks:
                        nextTime = TimeSpan.FromDays(14);
                        break;
                    case StudyRangeType.OneMonth:
                        nextTime = TimeSpan.FromDays(30);
                        break;
                }

                notifir.AddToSchedule(new ScheduledToastNotification(toast.GetXml(),new DateTimeOffset(DateTime.Now + nextTime)));
                
            }
            await SaveAllDataAsync();
            deferral.Complete();
        }

        //アプリが再開しようとしたとき
        private async void App_Resuming(object sender, object e)
        {
            //ここでアプリのデータや状態を復元する
            if(ApplicationData.Current.LocalSettings.Values.ContainsKey("IsPicking"))
            {
                if (!bool.Parse(ApplicationData.Current.LocalSettings.Values["IsPicking"].ToString()))
                {
                    await LoadAllDataAsync();
                }
            }
            else
            {
                await LoadAllDataAsync();
            }
        }

        private async Task SaveAllDataAsync()
        {
            await DataSaveAsync(ApplicationData.Current.LocalFolder,ServiceLocator.Current.GetInstance<MainViewModel>().MainModel,"MainModelData");
            await DataSaveAsync(ApplicationData.Current.RoamingFolder, ServiceLocator.Current.GetInstance<MainViewModel>().RoamingModel, "RoamingModelData");
        }

        private async Task LoadAllDataAsync()
        {
            var mainModel = await DataLoadAsync(ApplicationData.Current.LocalFolder, typeof(MainModel), "MainModelData");
            if (mainModel != null)
            {
                ServiceLocator.Current.GetInstance<MainViewModel>().MainModel = (MainModel)mainModel;
            }
            else
            {
                ServiceLocator.Current.GetInstance<MainViewModel>().MainModel = new MainModel();
            }
            var roamingModel = await DataLoadAsync(ApplicationData.Current.RoamingFolder, typeof(RoamingModel), "RoamingModelData");
            if (roamingModel != null)
            {
                ServiceLocator.Current.GetInstance<MainViewModel>().RoamingModel = (RoamingModel)roamingModel;
            }
            else
            {
                ServiceLocator.Current.GetInstance<MainViewModel>().RoamingModel = new RoamingModel();
            }
        }

        //アプリのデータを保存するメソッド
        //App_Suspendingから呼ばれる
        public static async Task DataSaveAsync(StorageFolder folder,object data,string fileName)
        {
            /*ファイルにデータを書き込む場合*/
            string mainModelJson = JsonConvert.SerializeObject(data);
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, mainModelJson);
        }

        //アプリのデータを復元するメソッド
        //App_ResumingとOnLaunchedから呼ばれる
        //正常復元した場合はtrue、アプリのデータがなかったか、復元失敗した場合はfalseを返す
        public static async Task<object> DataLoadAsync(StorageFolder folder,Type dataType,string fileName)
        {
            /*ファイルからデータをロードする場合*/
            try {
                var files = await folder.GetFilesAsync();
                if (files.Any(q => q.Name == fileName))
                {
                    var file = files.First(q => q.Name == fileName);
                    var saveData = await FileIO.ReadTextAsync(file);
                    var model = JsonConvert.DeserializeObject(saveData, dataType);
                    return model;
                }
                else
                {
                    return null;
                }
            }catch(Exception)
            {
                return null;
            }
        }
    }
}
