using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using WordShuffle.ViewModels;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace WordShuffle.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class FavoritePage : Page
    {
        public FavoritePage()
        {
            this.InitializeComponent();
        }

        private async void button_synthesize_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_favorite.SelectedIndex != -1)
            {
                var index = listBox_favorite.SelectedIndex;
                var vm = DataContext as MainViewModel;
                var item = vm.TempModel.FavoriteList[index];
                vm.SynthesizeWithWordCommand.Execute(item);
            }
            else
            {
                var dialog = new MessageDialog("単語を選択してください","エラー");
                await dialog.ShowAsync();
            }
        }

        private async void button_unFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_favorite.SelectedIndex != -1)
            {
                var index = listBox_favorite.SelectedIndex;
                var vm = DataContext as MainViewModel;
                var item = vm.TempModel.FavoriteList[index];
                vm.RemoveFavoriteWithItemCommand.Execute(item);
            }
            else
            {
                var dialog = new MessageDialog("単語を選択してください", "エラー");
                await dialog.ShowAsync();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var vm = this.DataContext as MainViewModel;
            vm.UnFetchFavoriteCommand.Execute(null);
            vm.UnFetchNoRememberCommand.Execute(null);
        }


        private void pivot_main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.DataContext as MainViewModel;
            switch (pivot_main.SelectedIndex)
            {
                case 0:
                    vm.FetchFavoriteCommand.Execute(null);
                    break;
                case 1:
                    vm.FetchNoRememberCommand.Execute(null);
                    
                    break;
            }
            
        }

        private async void button_synthesizeTopNoRemember_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_noRemember.SelectedIndex != -1)
            {
                var index = listBox_noRemember.SelectedIndex;
                var vm = DataContext as MainViewModel;
                var item = vm.TempModel.TopNoRememberList[index];
                vm.SynthesizeWithWordCommand.Execute(item);
            }
            else
            {
                var dialog = new MessageDialog("単語を選択してください", "エラー");
                await dialog.ShowAsync();
            }
        }
    }
}
