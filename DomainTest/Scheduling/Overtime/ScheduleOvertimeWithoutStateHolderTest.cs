using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Overtime
{
	[DomainTest]
	[TestFixture]
	public class ScheduleOvertimeWithoutStateHolderTest
	{
		public ScheduleOvertimeWithoutStateHolder Target;
		public MutableNow Now;
		

		[Test]
		public void ShouldDoNothingIfNoScheduleDays()
		{
			Target.Execute(new List<IScheduleDay>());
		}
		
	}
}
