using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordShuffle.Models
{
    public class WordFile:ObservableObject,ICsvItem
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { this.Set(ref _name,value); }
        }
        private string _path;

        public string Path
        {
            get { return _path; }
            set { this.Set(ref _path,value); }
        }

        private DateTime _lastLearnDate;

        public DateTime LastLearnDate
        {
            get { return _lastLearnDate; }
            set { this.Set(ref _lastLearnDate,value); }
        }

        private int _rememberCount;

        public int RememberCount
        {
            get { return _rememberCount; }
            set { this.Set(ref _rememberCount,value); }
        }

        private int _noRememberCount;

        public int NoRememberCount
        {
            get { return _noRememberCount; }
            set { this.Set(ref _noRememberCount,value); }
        }

        public double RemenberRate
        {
            get
            {
                if (RememberCount == 0 && NoRememberCount == 0)
                {
                    return 0;
                }
                else
                {
                    return (double)RememberCount / (double)(RememberCount + NoRememberCount);
                    
                }
            }
        }

        public string CsvRawName
        {
            get
            {
                return this.Name;
            }
        }

        public string GetCsvRaw()
        {
            return Name + "," + Path + "," + LastLearnDate + "," + RememberCount + "," + NoRememberCount;
        }

        public void SetCsvRaw(string rawStr)
        {
            string[] cols = rawStr.Split(',');
            this.Name = cols[0];
            this.Path = cols[1];
            this.LastLearnDate = DateTime.Parse(cols[2]);
            this.RememberCount = int.Parse(cols[3]);
            this.NoRememberCount = int.Parse(cols[4]);
        }

        public WordFile(string name,string path,DateTime lastLearnDate,int rememberCount,int noRememberCount)
        {
            this.Name = name;
            this.Path = path;
            this.LastLearnDate = lastLearnDate;
            this.RememberCount = rememberCount;
            this.NoRememberCount = noRememberCount;
        }

        
    }
}
