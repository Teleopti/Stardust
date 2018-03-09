using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class RemoveApprovedPeriodTest
	{
		public FakeDatabase Database;
		public IAgentAdherenceDayLoader Target;

		[Test]
		public void ShouldRemove()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00")
				.WithRemovedApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00");

			var result = Target.Load(person, "2018-03-09".Date());

			result.ApprovedPeriods().Should().Be.Empty();
		}
	}
}