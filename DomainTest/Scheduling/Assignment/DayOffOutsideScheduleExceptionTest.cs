using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class DayOffOutsideScheduleExceptionTest : ExceptionTest<DayOffOutsideScheduleException>
    {
        protected override DayOffOutsideScheduleException CreateTestInstance(string message, Exception innerException)
        {
            return new DayOffOutsideScheduleException(message, innerException);
        }

        protected override DayOffOutsideScheduleException CreateTestInstance(string message)
        {
            return new DayOffOutsideScheduleException(message);
        }
    }

    
}
