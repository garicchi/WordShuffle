using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using NotificationsExtensions.Tiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace WordShuffle.Models
{
    public class MainModel : ObservableObject
    {

        public ObservableCollection<WordItem> WordList
        {
            get
            {
                return _wordList;
            }

            set
            {
                this.Set(ref _wordList,value);
            }
        }

        private ObservableCollection<WordItem> _wordList;

        private ObservableCollection<WordFile> _recentlyFileList;

        public ObservableCollection<WordFile> RecentlyFileList
        {
            get { return _recentlyFileList; }
            set { this.Set(ref _recentlyFileList, value); }
        }

        public WordItem CurrentWord
        {
            get
            {
                return _currentWord;
            }

            set
            {
                this.Set(ref _currentWord,value);
            }
        }

        private WordItem _currentWord;

        public SettingsModel Settings
        {
            get
            {
                return _settings;
            }

            set
            {
                this.Set(ref _settings, value);
            }
        }

        
        private SettingsModel _settings;

        private int _currentWordPosition;

        public int CurrentWordPosition
        {
            get
            {
                return _currentWordPosition;
            }
            set
            {
                this.Set(ref _currentWordPosition,value);
            }
        }

        private bool _isHiddenAnswer;

        public bool IsHiddenAnswer
        {
            get
            {
                return _isHiddenAnswer;
            }

            set
            {
                this.Set(ref _isHiddenAnswer, value);
            }
        }

        public int MaxWordNum
        {
            get { return WordList.Count; }
        }

        public int RememberCount
        {
            get
            {
                return WordList.Count(q => q.IsRemember);
            }
        }

        public int NoRememberCount
        {
            get
            {
                return WordList.Count(q => !q.IsRemember);
            }
        }

        public double RememberRate
        {
            get
            {
                double result = 0;
                if (MaxWordNum > 0)
                {
                    result = (double)RememberCount / (double)MaxWordNum;
                }
                return result;
            }
        }

        private ObservableCollection<WordItem> _topNoRememberList;

        public ObservableCollection<WordItem> TopNoRememberList
        {
            get { return _topNoRememberList; }
            set { this.Set(ref _topNoRememberList,value); }
        }


        private string _browserUrl;

        public string BrowserUrl
        {
            get { return _browserUrl; }
            set { this.Set(ref _browserUrl,value); }
        }

        private StorageFile _currentFile;

        public void SetCurrentFile(StorageFile file)
        {
            this._currentFile = file;
        }

        public WordFile CurrentWordFile
        {
            get
            {
                WordFile result = null;
                if (_currentFile != null)
                {
                    result = new WordFile(_currentFile.Name, _currentFile.Path, DateTime.Now, RememberCount, NoRememberCount);
                }
                return result;
            }
        }

        private string _currentFileName;

        public string CurrentFileName
        {
            get { return _currentFileName; }
            set { this.Set(ref _currentFileName,value); }
        }
        

        public bool IsStudying
        {
            get
            {
                return _isStudying;
            }

            set
            {
                this.Set(ref _isStudying,value);
            }
        }

        private bool _isStudying;

        private bool _canSynthesize;

        public bool CanSythesize
        {
            get { return _canSynthesize; }
            set { this.Set(ref _canSynthesize,value); }
        }

        private ObservableCollection<WordFile> _currentLearnedFileList;

        public ObservableCollection<WordFile> CurrentLearnedFileList
        {
            get { return _currentLearnedFileList; }
        }

        private DateTime _currentLearnedDay;

        private WordItem _currentEditingWord;

        public WordItem CurrentEditingWord
        {
            get { return _currentEditingWord; }
            set { this.Set(ref _currentEditingWord,value); }
        }
        

        public void ChangeCurrentLearnedDay(DateTime currentDate,ObservableCollection<WordFile> list)
        {
            _currentLearnedDay = currentDate;
            CurrentLearnedFileList.Clear();
            foreach (var i in list)
            {
                if (i.LastLearnDate.DayOfYear == currentDate.DayOfYear)
                {
                    CurrentLearnedFileList.Add(i);
                }
            }
        }

        public MainModel()
        {
            Settings = new SettingsModel();
            WordList = new ObservableCollection<WordItem>();
            IsHiddenAnswer = true;
            TopNoRememberList = new ObservableCollection<WordItem>();
            RecentlyFileList = new ObservableCollection<WordFile>();
            _currentLearnedFileList = new ObservableCollection<WordFile>();
            IsStudying = false;
            CanSythesize = false;
            CurrentFileName = "";
            BrowserUrl = "";
            CurrentWord = new WordItem("","",0,0);
            CurrentEditingWord = new WordItem("", "", 0, 0);
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        public async Task StartStudyAsync()
        {
            await LoadWordFileAsync();
            
            InitStudy();
            NextWord(false);
        }

        public void RepeatStudy()
        {
            InitStudy();
            NextWord(false);
        }

        public void InitStudy()
        {
            CurrentWordPosition = -1;
            IsStudying = true;
            CurrentWord = new WordItem("","",0,0);
            List<WordItem> items = WordList.OrderBy(q => Guid.NewGuid()).ToList();
            WordList.Clear();
            foreach (WordItem item in items)
            {
                WordList.Add(item);
            }
        }

        public bool NextWord(bool isRemember)
        {
            IsHiddenAnswer = true;
            if (CurrentWordPosition != -1)
            {
                CurrentWord.IsRemember = isRemember;
                if (isRemember)
                {
                    CurrentWord.RememberCount++;
                }
                else
                {
                    CurrentWord.NoRememberCount++;
                }
            }
            CurrentWordPosition++;
            if (CurrentWordPosition >= WordList.Count)
            {
                CurrentFileName = string.Empty;
                TopNoRememberList.Clear();
                var list = WordList.OrderBy(q => q.RememberRate).Take(5).ToList();
                foreach (var item in list)
                {
                    TopNoRememberList.Add(item);
                    UpdateLiveTileForText(item.Word,item.Mean,"正答率 = "+((int)(item.RememberRate*100))+"%");
                }

                if (RecentlyFileList.Count > 5)
                {
                    RecentlyFileList.RemoveAt(RecentlyFileList.Count - 1);
                }
                RecentlyFileList.Insert(0, this.CurrentWordFile);
                
                var temp = new List<WordFile>(RecentlyFileList.OrderByDescending(q=>q.LastLearnDate));
                RecentlyFileList.Clear();
                foreach (var item in temp)
                {
                    RecentlyFileList.Add(item);
                }
                

                IsStudying = false;
                return false;
            }
            else
            {
                CurrentWord = WordList.ElementAt(CurrentWordPosition);

                return true;
            }
            
                
        }

        public void ShowAnswer()
        {
            IsHiddenAnswer = false;
        }

        public void Synthesize()
        {
            if (Settings.IsSpeech)
            {
                Messenger.Default.Send<string>(CurrentWord.Word, "Synthesize");
            }
        }

        public void Synthesize(string word)
        {
            Messenger.Default.Send<string>(word, "Synthesize");
        }

        public void ChangeWeblioUrl()
        {
            BrowserUrl = "http://ejje.weblio.jp/content/"+CurrentWord.Word;
        }

        public void ChangeSynonymUrl()
        {
            BrowserUrl = "http://ejje.weblio.jp/english-thesaurus/content/" + CurrentWord.Word;
        }

        public void ChangeExampleUrl()
        {
            BrowserUrl = "http://ejje.weblio.jp/sentence/content/" + CurrentWord.Word;
        }

        public void ChangeImageSearchUrl()
        {
            BrowserUrl = "https://www.google.co.jp/search?tbm=isch&safe=high&q="+CurrentWord.Word;
        }

        public string GetShareResult()
        {
            return "WordShuffleを使って "+MaxWordNum+"単語中 "+RememberCount+"単語暗記しました";
        }

        public void UpdateLiveTileForText(string title, string subtitle, string body)
        {
            var bindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileText()
                    {
                        Text = title,
                        Style = TileTextStyle.Body
                    },

                    new TileText()
                    {
                        Text = subtitle,
                        Style = TileTextStyle.BodySubtle
                    },

                    new TileText()
                    {
                        Text = body,
                        Style = TileTextStyle.Caption
                    }
                }
            };
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = bindingContent
                    },

                    TileWide = new TileBinding()
                    {
                        Content = bindingContent
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = bindingContent
                    },
                    Branding = TileBranding.NameAndLogo
                },
            };
            var xml = content.GetXml();
            
            TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(xml));
        }

        public async Task SaveLearningResultAsync()
        {
            List<WordItem> items = new List<WordItem>();
            foreach(var item in WordList)
            {
                items.Add(item);
            }
            await SaveWordFileAsync(_currentFile,items);
        }

        public async Task SaveNoRememberAsync()
        {
            var fileType = _currentFile.FileType;
            string mainName = _currentFile.Name.Replace(fileType,"");
            if (mainName.Contains("_learn"))
            {
                var cols = mainName.Replace("_learn", ",").Split(',');
                var name = cols[0];
                var num = int.Parse(cols[1]);
                num++;
                mainName = name + "_learn" + num.ToString() + fileType;
            }
            else
            {
                mainName = mainName + "_learn1" + fileType;
            }

            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedFileName = mainName;
            picker.FileTypeChoices.Add("CSV", new string[] { ".csv" });

            StorageFile file = await picker.PickSaveFileAsync();
            if (file == null)
                throw new Exception("ファイルが選択されていません");

            List<WordItem> items = new List<WordItem>();
            foreach (var item in WordList)
            {
                if (!item.IsRemember)
                {
                    items.Add(item);
                }
            }
            await SaveWordFileAsync(file, items);
        }

        private async Task LoadWordFileAsync()
        {
            ApplicationData.Current.LocalSettings.Values["IsPicking"] = true;
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".csv");
            _currentFile = await picker.PickSingleFileAsync();

            ApplicationData.Current.LocalSettings.Values["IsPicking"] = false;

            if (_currentFile == null)
                throw new Exception("ファイルが選択されていません");

            CurrentFileName = _currentFile.Name;
            var buff = await FileIO.ReadBufferAsync(_currentFile);
            DataReader reader = DataReader.FromBuffer(buff);
            byte[] dataBuff = new byte[buff.Length];
            reader.ReadBytes(dataBuff);

            string dataStr = Portable.Text.Encoding.GetEncoding("Shift-JIS").GetString(dataBuff, 0, dataBuff.Length);

            IList<string> lines = dataStr.Split('\n').ToList();
            WordList.Clear();

            foreach (string line in lines)
            {
                string tempLine = line.Replace("\r", "");

                if (string.IsNullOrEmpty(tempLine))
                    continue;

                string[] str = tempLine.Split(',');
                WordItem item = null;
                if (str.Length == 4)
                {
                    int col2 = 0;
                    int.TryParse(str[2], out col2);
                    int col3 = 0;
                    int.TryParse(str[3],out col3);
                    item = new WordItem(str[0], str[1],col2, col3);
                }
                else
                {
                    item = new WordItem(str[0], str[1], 0, 0);
                }
                WordList.Add(item);
            }

            RaisePropertyChanged("MaxWordNum");
        }

        private async Task SaveWordFileAsync(StorageFile file,List<WordItem> wordList)
        {
            List<string> lines = new List<string>();
            foreach (WordItem item in wordList)
            {
                lines.Add(item.Word + "," + item.Mean + "," + item.RememberCount+","+item.NoRememberCount);
            }
            string text = string.Join("\n", lines);
            byte[] buff = Portable.Text.Encoding.GetEncoding("Shift-JIS").GetBytes(text);
            await FileIO.WriteBytesAsync(file, buff);

        }

        private async Task<string> LoadWebImageFromMeanAsync()
        {
            string searchWord = CurrentWord.Word;//検索ワード
            string market = "ja-JP";//検索をかける言語コード
            string adult = "Strict";   //エロコンテンツフィルタをかけるかどうかOff,Moderate,Strictの3種類
            int top = 5; //トップ何個の検索結果を返すか(デフォルト50)
            string format = "json"; //atom(xml)かjsonか

            //イメージフィルター
            string imageFilter = "Size:Small";
            ImageSearchObject contents = null;
            try
            {
                //文字列エンコード
                searchWord = Uri.EscapeDataString(searchWord);

                HttpClientHandler handler = new HttpClientHandler();
                //認証情報を追加
                handler.Credentials = new NetworkCredential("kEO5MTTzRJwAjTfMUtfBUd0gO5E1oh0GpF8Re2IRjuk=", "kEO5MTTzRJwAjTfMUtfBUd0gO5E1oh0GpF8Re2IRjuk=");

                HttpClient client = new HttpClient(handler);
                //Getリクエスト
                var resultStr = await client.GetStringAsync("https://api.datamarket.azure.com/Bing/Search/Image"
                    + "?Query=" + "'" + searchWord + "'" + "&Market=" + "'" + market + "'" + "&Adult=" + "'" + adult + "'" + "&$top=" + top + "&$format=" + format + "&ImageFilters='" + imageFilter + "'");

                //文字列デコード
                resultStr = Uri.UnescapeDataString(resultStr);

                //Jsonシリアライズ
                contents = JsonConvert.DeserializeObject<ImageSearchObject>(resultStr);
                Result result = contents.d.results.OrderBy(q => Guid.NewGuid()).First();
                return result.MediaUrl;
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }
    }
}
