using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordShuffle.Models
{
    public interface ICsvItem
    {
        string CsvRawName { get; }
        string GetCsvRaw();
        void SetCsvRaw(string rawStr);
    }
}
