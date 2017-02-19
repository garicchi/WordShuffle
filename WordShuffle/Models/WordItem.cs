using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace WordShuffle.Models
{
    public class WordItem:ObservableObject,ICsvItem
    {
        public WordItem(string word,string mean,int rememberCount,int noRememberCount)
        {
            this.Word = word;
            this.Mean = mean;
            this.RememberCount = rememberCount;
            this.NoRememberCount = NoRememberCount;
            this.IsRemember = false;
        }
        private string _word;

        public string Word
        {
            get { return _word; }
            set { Set(ref _word,value); }
        }

        private string _mean;

        public string Mean
        {
            get { return _mean; }
            set { Set(ref _mean, value); }
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

        public double RememberRate
        {
            get {
                double result = 0;
                if (RememberCount == 0 && NoRememberCount == 0)
                {
                    return result;
                }
                else
                {
                    return (double)RememberCount / (double)(RememberCount + NoRememberCount);
                }
            }
                
        }

        private bool _isRemember;

        public bool IsRemember
        {
            get { return _isRemember; }
            set { Set(ref _isRemember, value); }
        }

        public string CsvRawName
        {
            get
            {
                return this.Word;
            }
        }

        public string GetCsvRaw()
        {
            return Word + "," + Mean + "," + RememberCount + "," + NoRememberCount;
        }

        public void SetCsvRaw(string rawStr)
        {
            string[] cols = rawStr.Split(',');
            this.Word = cols[0];
            this.Mean = cols[1];
            this.RememberCount = int.Parse(cols[2]);
            this.NoRememberCount = int.Parse(cols[3]);
        }
    }
}
