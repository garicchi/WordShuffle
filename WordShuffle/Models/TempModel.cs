using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WordShuffle.Utils;

namespace WordShuffle.Models
{
    public class TempModel:ObservableObject
    {
        private CsvObservableCollection<WordItem> _favoriteList;

        public CsvObservableCollection<WordItem> FavoriteList
        {
            get { return _favoriteList; }
            set { this.Set(ref _favoriteList, value); }
        }

        private CsvObservableCollection<WordFile> _learnedFileList;

        public CsvObservableCollection<WordFile> LearnedFileList
        {
            get { return _learnedFileList; }
            set { this.Set(ref _learnedFileList, value); }
        }

        private CsvObservableCollection<WordItem> _topNoRememberList;

        public CsvObservableCollection<WordItem> TopNoRememberList
        {
            get { return _topNoRememberList; }
            set { this.Set(ref _topNoRememberList,value); }
        }

        private CsvObservableCollection<WordItem> _editingWordList;

        public CsvObservableCollection<WordItem> EditingWordList
        {
            get { return _editingWordList; }
            set { this.Set(ref _editingWordList,value); }
        }


        private ObservableCollection<WordFile> _topForgetFileList;

        public ObservableCollection<WordFile> TopForgetFileList
        {
            get { return _topForgetFileList; }
            set { this.Set(ref _topForgetFileList,value); }
        }
        

        private Dictionary<int, List<WordFile>> _learnedFileDayDic;

        public Dictionary<int, List<WordFile>> LearnedFileDayDic
        {
            get { return _learnedFileDayDic; }
            set { this.Set(ref _learnedFileDayDic,value); }
        }

        public async Task UpdateLearnedDayDicAsync()
        {
            await Task.Run(()=>
            {
                LearnedFileDayDic.Clear();
                var group = LearnedFileList.GroupBy(q => q.LastLearnDate.DayOfYear);
                foreach (var list in group)
                {
                    var wordList = new List<WordFile>();
                    foreach (var item in list)
                    {
                        wordList.Add(item);
                    }
                    LearnedFileDayDic.Add(list.Key, wordList);
                }
            });
            
        }

        public async Task UpdateForgetFileListAsync()
        {
            await App.AppDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,() =>
            {
                TopForgetFileList.Clear();
                var list = LearnedFileList.OrderByDescending(q=>Math.Abs(DateTime.Now.DayOfYear - q.LastLearnDate.DayOfYear));
                foreach (var i in list)
                {
                    TopForgetFileList.Add(i);
                }
            });
        }

        public TempModel()
        {
            FavoriteList = new CsvObservableCollection<WordItem>(ApplicationData.Current.RoamingFolder,"Favorite",App.AppDispatcher);
            LearnedFileList = new CsvObservableCollection<WordFile>(ApplicationData.Current.RoamingFolder,"LearnedFile", App.AppDispatcher);
            TopNoRememberList = new CsvObservableCollection<WordItem>(ApplicationData.Current.RoamingFolder, "TopNoRemember", App.AppDispatcher);
            EditingWordList = new CsvObservableCollection<WordItem>(ApplicationData.Current.RoamingFolder, "EditingWordList", App.AppDispatcher);
            LearnedFileDayDic = new Dictionary<int, List<WordFile>>();
            TopForgetFileList = new ObservableCollection<WordFile>();
        }

        public async Task SaveEditingWordAsync()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("CSV", new string[] { ".csv" });

            StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
                throw new Exception("ファイルが選択されていません");

            List<WordItem> items = new List<WordItem>();
            foreach (var item in EditingWordList)
            {
                items.Add(item);
            }
            await SaveWordFileAsync(file, items);
        }

        public async Task SaveTopNoRememberAsync()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("CSV", new string[] { ".csv" });

            StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
                throw new Exception("ファイルが選択されていません");

            List<WordItem> items = new List<WordItem>();
            foreach (var item in TopNoRememberList)
            {
                items.Add(item);
            }
            await SaveWordFileAsync(file, items);
        }

        public async Task SaveFavoriteAsync()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("CSV", new string[] { ".csv" });

            StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
                throw new Exception("ファイルが選択されていません");

            List<WordItem> items = new List<WordItem>();
            foreach (var item in FavoriteList)
            {
                items.Add(item);
            }
            await SaveWordFileAsync(file, items);
        }

        private async Task SaveWordFileAsync(StorageFile file, List<WordItem> wordList)
        {
            List<string> lines = new List<string>();
            foreach (WordItem item in wordList)
            {
                lines.Add(item.Word + "," + item.Mean);
            }
            string text = string.Join("\n", lines);
            byte[] buff = Portable.Text.Encoding.GetEncoding("Shift-JIS").GetBytes(text);
            await FileIO.WriteBytesAsync(file, buff);

        }

    }
}
