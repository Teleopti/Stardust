using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class ApprovedPeriodPercentageTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldRecalculatePercentage()
		{
			Now.Is("2018-02-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2018-02-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2018-02-08 08:00", "2018-02-08 18:00")
				.WithAdherenceOut("2018-02-08 08:00")
				.WithAdherenceIn("2018-02-08 09:00")
				.WithApprovedPeriod("2018-02-08 08:00", "2018-02-08 09:00")
				;

			var result = Target.Load(person, "2018-02-08".Date());

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldApproveNeutralPeriodAsInAdherence_Fokklokiklilogogoblipblopblapp_For()
		{
			Now.Is("2017-12-08 21:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 10:00", "2017-12-08 20:00")
				.WithAdherenceOut("2017-12-08 10:00")
				.WithAdherenceNeutral("2017-12-08 11:00")
				.WithApprovedPeriod("2017-12-08 11:00", "2017-12-08 20:00")
				;

			var result = Target.Load(person);

			result.Percentage().Should().Be(90);
		}

		[Test]
		public void ShouldApproveNeutralPeriodAsInAdherence_Fokklokiklilogogoblipblopblapp()
		{
			Now.Is("2017-12-08 21:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 10:00", "2017-12-08 18:00")
				.WithAdherenceNeutral("2017-12-08 10:00")
				.WithAdherenceIn("2017-12-08 12:00")
				.WithAdherenceOut("2017-12-08 14:00")
				.WithApprovedPeriod("2017-12-08 10:00", "2017-12-08 12:00")
				;

			var result = Target.Load(person);

			result.Percentage().Should().Be(50);
		}
	}
}