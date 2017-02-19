using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using WordShuffle.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Storage;

namespace WordShuffle.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private MainModel _mainModel;

        public MainModel MainModel
        {
            get { return _mainModel; }
            set { this.Set(ref _mainModel, value); }
        }

        private TempModel _tempModel;

        public TempModel TempModel
        {
            get { return _tempModel; }
            set { this.Set(ref _tempModel, value); }
        }

        private RoamingModel _roamingModel;

        public RoamingModel RoamingModel
        {
            get { return _roamingModel; }
            set { this.Set(ref _roamingModel,value); }
        }
        

        public RelayCommand AnswerCommand { get; set; }

        public RelayCommand NextRememberCommand { get; set; }

        public RelayCommand NextNoRememberCommand { get; set; }

        public RelayCommand StartStudyCommand { get; set; }

        public RelayCommand RepeatStudyCommand { get; set; }

        public RelayCommand SynthesizeCommand { get; set; }

        public RelayCommand<WordItem> SynthesizeWithWordCommand { get; set; }

        public RelayCommand SearchWeblioCommand { get; set; }

        public RelayCommand SearchSynonymCommand { get; set; }

        public RelayCommand SearchExampleCommand { get; set; }

        public RelayCommand SearchImageCommand { get; set; }

        public RelayCommand SaveLearningResultCommand { get; set; }

        public RelayCommand SaveNoRememberCommand { get; set; }

        public RelayCommand AddFavoriteCommand { get; set; }

        public RelayCommand<WordItem> AddFavoriteWithItemCommand { get; set; }

        public RelayCommand<WordItem> RemoveFavoriteWithItemCommand { get; set; }

        public RelayCommand ClearFavoriteCommand { get; set; }

        public RelayCommand FetchFavoriteCommand { get; set; }

        public RelayCommand UnFetchFavoriteCommand { get; set; }

        public RelayCommand FetchNoRememberCommand { get; set; }

        public RelayCommand UnFetchNoRememberCommand { get; set; }

        public RelayCommand FetchLearnedFileCommand { get; set; }

        public RelayCommand UnFetchLearnedFileCommand { get; set; }

        public RelayCommand ShareResultCommand { get; set; }

        public RelayCommand PickFileCommand { get; set; }

        public RelayCommand<DateTime> ChangeCurrentLearnedDayCommand { get; set; }

        public RelayCommand ExportFavoriteCommand { get; set; }

        public RelayCommand ExportTopNoRememberCommand { get; set; }

        public RelayCommand ClearTopNoRememberCommand { get; set; }

        public RelayCommand AddEditingWordCommand { get; set; }
        public RelayCommand<string> RemoveEditingWordCommand { get; set; }

        public RelayCommand<int> EditEditingWordCommand { get; set; }

        public RelayCommand ExportEditingWordCommand { get; set; }
        public RelayCommand FetchEdittingWordCommand { get; set; }

        public RelayCommand UnFetchEdittingWordCommand { get; set; }


        public MainViewModel()
        {
            MainModel = new MainModel();
            TempModel = new TempModel();
            RoamingModel = new RoamingModel();

            AnswerCommand = new RelayCommand(() =>
            {
                MainModel.ShowAnswer();
            });

            NextRememberCommand = new RelayCommand(() =>
            {
                var canContinue = MainModel.NextWord(true);
                if (!canContinue)
                {
                    Messenger.Default.Send<bool>(false, "GoResult");
                }
                else
                {
                    MainModel.Synthesize();
                }
            });

            NextNoRememberCommand = new RelayCommand(() =>
            {
                var canContinue = MainModel.NextWord(false);
                if (!canContinue)
                {
                    Messenger.Default.Send<bool>(false, "GoResult");
                }
                else
                {
                    MainModel.Synthesize();
                }
            });

            PickFileCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await MainModel.StartStudyAsync();
                }, () =>
                {
                    Messenger.Default.Send<bool>(true, "FileLoadResult");
                }, (e) =>
                {
                    Messenger.Default.Send<bool>(false, "FileLoadResult");
                });

            });

            StartStudyCommand = new RelayCommand(() =>
            {
                if (RoamingModel.PrevLearnDay.DayOfYear == (DateTime.Now - TimeSpan.FromDays(1)).DayOfYear)
                {
                    RoamingModel.ContinueDays++;
                    RoamingModel.PrevLearnDay = DateTime.Now;
                } else if (RoamingModel.PrevLearnDay.DayOfYear != DateTime.Now.DayOfYear)
                {
                    RoamingModel.ContinueDays = 0;
                }
                RoamingModel.LearnCount++;
                MainModel.Synthesize();
            });

            RepeatStudyCommand = new RelayCommand(() =>
            {
                MainModel.RepeatStudy();
            });

            SynthesizeCommand = new RelayCommand(() =>
            {
                MainModel.Synthesize();
            });

            SearchWeblioCommand = new RelayCommand(() =>
            {
                MainModel.ChangeWeblioUrl();
                Messenger.Default.Send<string>(MainModel.BrowserUrl, "ShowBrowser");
            });

            SearchSynonymCommand = new RelayCommand(() =>
            {
                MainModel.ChangeSynonymUrl();
                Messenger.Default.Send<string>(MainModel.BrowserUrl, "ShowBrowser");
            });

            SearchExampleCommand = new RelayCommand(() =>
            {
                MainModel.ChangeExampleUrl();
                Messenger.Default.Send<string>(MainModel.BrowserUrl, "ShowBrowser");
            });

            SearchImageCommand = new RelayCommand(() =>
            {
                MainModel.ChangeImageSearchUrl();
                Messenger.Default.Send<string>(MainModel.BrowserUrl, "ShowBrowser");
            });

            SaveLearningResultCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    Messenger.Default.Send<bool>(true, "FileSaving");
                    await MainModel.SaveLearningResultAsync();
                    await TempModel.LearnedFileList.AddCsvRawAsync(MainModel.CurrentWordFile);
                    await TempModel.TopNoRememberList.FetchAsync((str) =>
                    {
                        string[] cols = str.Split(',');
                        return new WordItem(cols[0], cols[1], int.Parse(cols[2]), int.Parse(cols[3]));
                    });

                    if (TempModel.TopNoRememberList.Count + MainModel.TopNoRememberList.Count > 1000)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            await TempModel.TopNoRememberList.RemoveCsvRawAsync(TempModel.TopNoRememberList.ElementAt(i));
                        }
                    }
                    foreach (var item in MainModel.TopNoRememberList)
                    {
                        await TempModel.TopNoRememberList.AddCsvRawAsync(item);
                    }

                    await TempModel.TopNoRememberList.UnFetchAsync();


                }, () =>
                {
                    Messenger.Default.Send<bool>(false, "FileSaving");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            SaveNoRememberCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    Messenger.Default.Send<bool>(true, "FileSaving");
                    await MainModel.SaveNoRememberAsync();
                }, () =>
                {
                    Messenger.Default.Send<string>("ファイルを出力しました", "Success");
                    Messenger.Default.Send<bool>(false, "FileSaving");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            AddFavoriteCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.FavoriteList.AddCsvRawAsync(MainModel.CurrentWord);
                }, () =>
                 {
                     Messenger.Default.Send<string>("お気に入り登録完了しました", "Success");
                 }, (e) =>
                 {
                     Messenger.Default.Send<string>(e.Message, "Error");
                 });

            });

            AddFavoriteWithItemCommand = new RelayCommand<WordItem>(async (word) =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.FavoriteList.AddCsvRawAsync(word);
                }, () =>
                {
                    Messenger.Default.Send<string>("お気に入り登録完了しました", "Success");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });

            });

            RemoveFavoriteWithItemCommand = new RelayCommand<WordItem>(async (word) =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.FavoriteList.RemoveCsvRawAsync(word);
                }, () =>
                {
                    Messenger.Default.Send<string>("お気に入りに登録を解除しました", "Success");
                    FetchFavoriteCommand.Execute(null);
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            ClearFavoriteCommand = new RelayCommand(async () =>
            {
                var dialog = new MessageDialog("現在お気に入りに登録してる単語をすべてクリアしても大丈夫ですか？", "確認");
                dialog.Commands.Add(new UICommand("OK", async (arg) =>
                {
                    await ExecuteWithProgressAsync(async () =>
                    {
                        await TempModel.FavoriteList.ClearCsvRawAsync();
                    }, () =>
                    {
                        Messenger.Default.Send<string>("お気に入りに登録をすべて解除しました", "Success");
                        FetchFavoriteCommand.Execute(null);
                    }, (e) =>
                    {
                        Messenger.Default.Send<string>(e.Message, "Error");
                    });
                }));
                dialog.Commands.Add(new UICommand("Cancel", arg => { }));

                await dialog.ShowAsync();
            });

            FetchFavoriteCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.FavoriteList.FetchAsync((str) =>
                    {
                        var item = new WordItem("", "", 0, 0);
                        item.SetCsvRaw(str);
                        return item;
                    });
                }, () =>
                {
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });

            });

            UnFetchFavoriteCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.FavoriteList.UnFetchAsync();
                }, () =>
                {
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            FetchNoRememberCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.TopNoRememberList.FetchAsync((str) =>
                    {
                        var item = new WordItem("", "", 0, 0);
                        item.SetCsvRaw(str);
                        return item;
                    });
                    var list = TempModel.TopNoRememberList.OrderBy(q => q.RememberRate).ToList();
                    TempModel.TopNoRememberList.Clear();
                    foreach (var item in list)
                    {
                        TempModel.TopNoRememberList.Add(item);
                    }
                }, () =>
                {
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            UnFetchNoRememberCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.TopNoRememberList.UnFetchAsync();
                }, () =>
                {
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            FetchLearnedFileCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.LearnedFileList.FetchAsync((str) =>
                    {
                        var item = new WordFile("", "", DateTime.Now, 0, 0);
                        item.SetCsvRaw(str);
                        return item;
                    });
                    await TempModel.UpdateLearnedDayDicAsync();
                    await TempModel.UpdateForgetFileListAsync();
                }, () =>
                {
                    Messenger.Default.Send<bool>(true, "LearnFileFetched");
                }, (e) =>
                {
                    Messenger.Default.Send<bool>(false, "LearnFileFetched");
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            UnFetchLearnedFileCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.LearnedFileList.UnFetchAsync();
                    TempModel.LearnedFileDayDic.Clear();
                }, () =>
                {
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            SynthesizeWithWordCommand = new RelayCommand<WordItem>((item) =>
            {
                MainModel.Synthesize(item.Word);
            });

            ShareResultCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send<string>(MainModel.GetShareResult(), "ShareResult");
            });

            ChangeCurrentLearnedDayCommand = new RelayCommand<DateTime>((date) =>
            {
                MainModel.ChangeCurrentLearnedDay(date, TempModel.LearnedFileList);
            });

            ClearTopNoRememberCommand = new RelayCommand(async () =>
            {
                var dialog = new MessageDialog("現在記録されている忘れている単語をすべて削除してもいいですか？", "確認");
                dialog.Commands.Add(new UICommand("OK", async (arg) =>
                {
                    await ExecuteWithProgressAsync(async () =>
                    {
                        await TempModel.TopNoRememberList.ClearCsvRawAsync();
                    }, () =>
                    {
                        Messenger.Default.Send<string>("忘れている単語をすべて解除しました", "Success");
                        FetchNoRememberCommand.Execute(null);
                    }, (e) =>
                    {
                        Messenger.Default.Send<string>(e.Message, "Error");
                    });
                }));
                dialog.Commands.Add(new UICommand("Cancel", arg => { }));

                await dialog.ShowAsync();
            });

            ExportFavoriteCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.SaveFavoriteAsync();
                }, () =>
                {
                    Messenger.Default.Send<string>("ファイルを出力しました", "Success");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            ExportTopNoRememberCommand = new RelayCommand(async () =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.SaveTopNoRememberAsync();
                }, () =>
                {
                    Messenger.Default.Send<string>("ファイルを出力しました", "Success");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            AddEditingWordCommand = new RelayCommand(async() =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.EditingWordList.AddCsvRawAsync(MainModel.CurrentEditingWord);
                    await TempModel.EditingWordList.FetchAsync((str) =>
                    {
                        string[] cols = str.Split(',');
                        return new WordItem(cols[0], cols[1], int.Parse(cols[2]), int.Parse(cols[3]));
                    });
                    MainModel.CurrentEditingWord.Word = string.Empty;
                    MainModel.CurrentEditingWord.Mean = string.Empty;
                }, () =>
                {
                    //Messenger.Default.Send<string>("単語登録完了しました", "Success");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            EditEditingWordCommand = new RelayCommand<int>(async(index)=>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    var word = TempModel.EditingWordList.ElementAt(index);
                    await TempModel.EditingWordList.RemoveCsvRawAsync(word);
                    await TempModel.EditingWordList.AddCsvRawAsync(MainModel.CurrentEditingWord);
                    await TempModel.EditingWordList.FetchAsync((str) =>
                    {
                        string[] cols = str.Split(',');
                        return new WordItem(cols[0], cols[1], int.Parse(cols[2]), int.Parse(cols[3]));
                    });
                }, () =>
                {
                    //Messenger.Default.Send<string>("単語登録完了しました", "Success");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            RemoveEditingWordCommand = new RelayCommand<string>(async(word)=>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.EditingWordList.RemoveCsvRawAsync(new WordItem(word,"",0,0));
                    await TempModel.EditingWordList.FetchAsync((str) =>
                    {
                        string[] cols = str.Split(',');
                        return new WordItem(cols[0], cols[1], int.Parse(cols[2]), int.Parse(cols[3]));
                    });
                }, () =>
                {
                    Messenger.Default.Send<string>("単語削除完了しました", "Success");
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            ExportEditingWordCommand = new RelayCommand(async()=>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.SaveEditingWordAsync();
                }, async() =>
                {
                    var dialog = new MessageDialog("現在登録している単語をすべて削除しますか？", "正常にエクスポートできました");
                    dialog.Commands.Add(new UICommand("OK", async (arg) =>
                    {
                        await ExecuteWithProgressAsync(async () =>
                        {
                            await TempModel.EditingWordList.ClearCsvRawAsync();
                        }, () =>
                        {
                            FetchEdittingWordCommand.Execute(null);
                        }, (e) =>
                        {
                            Messenger.Default.Send<string>(e.Message, "Error");
                        });
                    }));
                    dialog.Commands.Add(new UICommand("Cancel", arg => { }));

                    await dialog.ShowAsync();
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            FetchEdittingWordCommand = new RelayCommand(async() =>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.EditingWordList.FetchAsync((str) =>
                    {
                        var item = new WordItem("", "", 0, 0);
                        item.SetCsvRaw(str);
                        return item;
                    });
                }, () =>
                {
                    Messenger.Default.Send<bool>(true, "LearnFileFetched");
                }, (e) =>
                {
                    Messenger.Default.Send<bool>(false, "LearnFileFetched");
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });

            UnFetchEdittingWordCommand = new RelayCommand(async()=>
            {
                await ExecuteWithProgressAsync(async () =>
                {
                    await TempModel.EditingWordList.UnFetchAsync();
                }, () =>
                {
                }, (e) =>
                {
                    Messenger.Default.Send<string>(e.Message, "Error");
                });
            });
        
        }

        private async Task ExecuteWithProgressAsync(Func<Task> processCallBack, Action successCallback, Action<Exception> errorCallBack)
        {
            await App.AppDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    Messenger.Default.Send<bool>(true, "Progress");

                    await processCallBack();
                    successCallback();
                }
                catch (Exception e)
                {
                    errorCallBack(e);
                }
                finally
                {
                    Messenger.Default.Send<bool>(false, "Progress");
                }
            });
        }

    }
}
