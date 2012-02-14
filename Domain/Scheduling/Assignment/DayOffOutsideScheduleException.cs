using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    [Serializable]
    public class DayOffOutsideScheduleException :Exception
    {
        public DayOffOutsideScheduleException()
        {
        }

        public DayOffOutsideScheduleException(string message)
            : base(message)
        {
        }

        public DayOffOutsideScheduleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DayOffOutsideScheduleException(SerializationInfo info,
                                     StreamingContext context) : base(info, context)
        {
        }
    }
}
