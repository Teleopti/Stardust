using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    [Serializable]
    public class DateDateTimePeriodDictionary : Dictionary<DateOnly, DateTimePeriod>
    {
        protected DateDateTimePeriodDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        public DateDateTimePeriodDictionary()
        {

        }
    }
}