using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    /// <summary>
    /// Class to save timespan values
    /// </summary>
    /// <remarks>
    /// Created by: Rob Wright
    /// Created date: 2014-09-25
    /// </remarks>
     [Serializable]
    public class TimeSpanSetting : SettingValue
    {
         private TimeSpan _timeSpanValue = new TimeSpan();

        public TimeSpanSetting(TimeSpan timeSpan)
        {
            _timeSpanValue = timeSpan;
        }

        public TimeSpanSetting()
        {
        }

        public TimeSpan TimeSpanValue
         {
             get { return _timeSpanValue; }
             set { _timeSpanValue = value; }
         }

         
    }
}




