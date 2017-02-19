using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class SettingPage : Page
    {
        MainViewModel _viewModel;
        public SettingPage()
        {
            this.InitializeComponent();
            _viewModel = DataContext as MainViewModel;
        }

        //設定ページに遷移してきたとき
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            comboNotifyRange.SelectedIndex = (int)_viewModel.MainModel.Settings.NotifyStudyRange;
        }

        private void comboNotifyRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboNotifyRange.SelectedIndex != -1)
            {
                _viewModel.MainModel.Settings.NotifyStudyRange = (StudyRangeType)comboNotifyRange.SelectedIndex;
            }
        }
    }
}
