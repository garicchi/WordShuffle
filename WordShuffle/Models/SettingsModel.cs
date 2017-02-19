using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordShuffle.Models
{
    public class SettingsModel : ObservableObject
    {
        private bool _isSpeech;

        public bool IsSpeech
        {
            get
            {
                return _isSpeech;
            }

            set
            {
                this.Set(ref _isSpeech,value);
            }
        }

        private StudyRangeType _notifyStudyRange;

        public StudyRangeType NotifyStudyRange
        {
            get { return _notifyStudyRange; }
            set { this.Set(ref _notifyStudyRange,value); }
        }
        

        public SettingsModel()
        {
            IsSpeech = true;
            NotifyStudyRange = StudyRangeType.OneWeek;
        }

        
    }
}
