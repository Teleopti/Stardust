using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ApprovePeriodAsInAdherence
{
	[DomainTest]
	public class ApprovedPeriodTest
	{
		public FakeDatabase Database;
		public IAgentAdherenceDayLoader Target;

		[Test]
		public void ShouldApprove()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-02-07 08:00:00", "2018-02-07 08:00:00");

			var result = Target.LoadUntilNow(person, "2018-02-07".Date());

			result.ApprovedPeriods().Single().StartTime.Should().Be("2018-02-07 08:00:00".Utc());
		}
	}
}