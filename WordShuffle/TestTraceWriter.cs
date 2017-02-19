using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace WordShuffle
{
    public class TestTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter
        {
            get
            {
                return TraceLevel.Error;
            }
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            Debug.WriteLine(level+" " + message+" " + ex);
        }
    }
}
