using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using WordShuffle.Models;

namespace WordShuffle.Utils
{
    public class CsvObservableCollection<T>:ObservableCollection<T> where T :ICsvItem
    {
        private string _fileName;

        private StorageFolder _folder;

        private CoreDispatcher _dispatcher;

        

        public CsvObservableCollection(StorageFolder folder,string fileName,CoreDispatcher dispatcher)
        {
            this._folder = folder;
            this._fileName = fileName;
            this._dispatcher = dispatcher;
        }

        public async Task FetchAsync(Func<string,T> addRawCallBack)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal,()=>
            {
                this.Clear();
            });
            
            var lines = await FileIOEx.ReadFileAsync(_folder, _fileName, "Shift-JIS");
            foreach(var line in lines)
            {
                var item = addRawCallBack(line);
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.Add(item);
                });
            }
        }

        public async Task UnFetchAsync()
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.Clear();
            });
        }

        public async Task AddCsvRawAsync(T item)
        {
            await AddItemToFileAsync(_folder,_fileName, item);
        }

        public async Task RemoveCsvRawAsync(T item)
        {
            await RemoveItemToFileAsync(_folder, _fileName, item);
        }

        public async Task ClearCsvRawAsync()
        {
            await ClearItemToFileAsync(_folder, _fileName);
        }

        private async Task AddItemToFileAsync(StorageFolder folder, string fileName, ICsvItem item)
        {
            var lines = await FileIOEx.ReadFileAsync(folder, fileName, "Shift-JIS");
            var isExist = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string[] cols = lines[i].Split(',');
                if (cols[0] == item.CsvRawName)
                {
                    lines[i] = item.GetCsvRaw();
                    isExist = true;
                }
            }
            if (!isExist)
            {
                lines.Add(item.GetCsvRaw());
            }
            await FileIOEx.SaveFileAsync(folder, fileName, lines, "Shift-JIS");
        }

        private async Task RemoveItemToFileAsync(StorageFolder folder, string fileName, ICsvItem item)
        {
            var lines = await FileIOEx.ReadFileAsync(folder, fileName, "Shift-JIS");
            var existIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                string[] cols = lines[i].Split(',');
                if (cols[0] == item.CsvRawName)
                {
                    existIndex = i;
                    break;
                }
            }
            if (existIndex != -1)
            {
                lines.RemoveAt(existIndex);
            }
            await FileIOEx.SaveFileAsync(folder, fileName, lines, "Shift-JIS");
        }

        private async Task ClearItemToFileAsync(StorageFolder folder, string fileName)
        {
            await FileIOEx.SaveFileAsync(folder, fileName, new List<string>(), "Shift-JIS");
        }
    }
}
