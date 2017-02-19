using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WordShuffle.ViewModels;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace WordShuffle.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class PlanningPage : Page
    {
        MainViewModel _viewModel;
        bool _isFetchCompleted;
        Color _themeColor;
        public PlanningPage()
        {
            this.InitializeComponent();
            _viewModel = DataContext as MainViewModel;
            
            calendar_learn.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _isFetchCompleted = false;
            _themeColor = Colors.Black;
            _isFetchCompleted = false;
            Messenger.Default.Register<bool>(this,"LearnFileFetched",(isOK)=>
            {
                _isFetchCompleted = true;
                calendar_learn.SetDisplayDate(DateTimeOffset.Now);
                calendar_learn.Visibility = Visibility.Visible;
                _viewModel.ChangeCurrentLearnedDayCommand.Execute(DateTime.Now);
            });
            _viewModel.FetchLearnedFileCommand.Execute(null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _viewModel.UnFetchLearnedFileCommand.Execute(null);
        }

        private void calendar_learn_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            if (calendar_learn.SelectedDates.Count > 0)
            {
                _viewModel.ChangeCurrentLearnedDayCommand.Execute(args.AddedDates.First().DateTime);
            }
        }

        private void pivot_learn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void calendar_learn_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            if (!_isFetchCompleted)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            if (_isFetchCompleted)
            {
                int dayOfYear = args.Item.Date.DayOfYear;
                var dic = _viewModel.TempModel.LearnedFileDayDic;
                if (dic.ContainsKey(args.Item.Date.DayOfYear))
                {
                    var list = dic[args.Item.Date.DayOfYear];
                    var colors = new List<Color>();
                    foreach(var i in list)
                    {
                        colors.Add(_themeColor);
                    }
                    args.Item.SetDensityColors(colors);
                }
            }
        }
    }
}
