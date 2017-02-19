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
    public sealed partial class EditPage : Page
    {
        string _flyoutType;
        MainViewModel _viewModel;
        int _edittingIndex;
        public EditPage()
        {
            this.InitializeComponent();
            _flyoutType = "none";
            _viewModel = DataContext as MainViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _viewModel.FetchEdittingWordCommand.Execute(null);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _viewModel.UnFetchEdittingWordCommand.Execute(null);
        }

        

        private void button_add_Click(object sender, RoutedEventArgs e)
        {
            _flyoutType = "add";
            flyout_edit.ShowAt(commandBar);
        }

        private async void button_edit_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_editing.SelectedIndex != -1)
            {
                _flyoutType = "edit";
                _edittingIndex = listBox_editing.SelectedIndex;
                var word = _viewModel.TempModel.EditingWordList.ElementAt(_edittingIndex);
                _viewModel.MainModel.CurrentEditingWord.Word = word.Word;
                _viewModel.MainModel.CurrentEditingWord.Mean = word.Mean;
                flyout_edit.ShowAt(commandBar);

            }
            else
            {
                var dialog = new MessageDialog("単語を選択してください", "エラー");
                await dialog.ShowAsync();
            }
            
        }

        private async void button_remove_Click(object sender, RoutedEventArgs e)
        {
            if (listBox_editing.SelectedIndex != -1)
            {
                var item = _viewModel.TempModel.EditingWordList.ElementAt(listBox_editing.SelectedIndex);
                _viewModel.RemoveEditingWordCommand.Execute(item.Word);
            }
            else
            {
                var dialog = new MessageDialog("単語を選択してください", "エラー");
                await dialog.ShowAsync();
            }
        }

        private void button_export_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportEditingWordCommand.Execute(null);
        }

        private void button_flyoutOk_Click(object sender, RoutedEventArgs e)
        {
            if(_flyoutType == "add")
            {
                _viewModel.AddEditingWordCommand.Execute(new WordItem(_viewModel.MainModel.CurrentEditingWord.Word,_viewModel.MainModel.CurrentEditingWord.Mean,0,0));
            }else if(_flyoutType == "edit")
            {
                _viewModel.EditEditingWordCommand.Execute(_edittingIndex);
            }

            flyout_edit.Hide();
        }
    }
}
