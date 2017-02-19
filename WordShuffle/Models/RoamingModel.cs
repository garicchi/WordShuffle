using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordShuffle.Models
{
    public class RoamingModel:ObservableObject
    {
        private int _continueDays;

        public int ContinueDays
        {
            get { return _continueDays; }
            set { this.Set(ref _continueDays,value); }
        }
        private DateTime _prevLearnDay;

        public DateTime PrevLearnDay
        {
            get { return _prevLearnDay; }
            set { this.Set(ref _prevLearnDay,value); }
        }

        private long _learnCount;

        public long LearnCount
        {
            get { return _learnCount; }
            set { this.Set(ref _learnCount,value); }
        }
        

        public RoamingModel()
        {
            ContinueDays = 0;
            LearnCount = 0;
            PrevLearnDay = DateTime.Now;
        }
    }
}
