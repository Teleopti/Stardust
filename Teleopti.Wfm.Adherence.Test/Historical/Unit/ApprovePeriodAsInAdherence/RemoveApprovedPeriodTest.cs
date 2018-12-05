using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ApprovePeriodAsInAdherence
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

			var result = Target.LoadUntilNow(person, "2018-03-09".Date());

			result.ApprovedPeriods().Should().Be.Empty();
		}

		[Test]
		public void ShouldReApprove()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00")
				.WithRemovedApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00")
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00");

			var result = Target.LoadUntilNow(person, "2018-03-09".Date());

			result.ApprovedPeriods().Single().StartTime.Should().Be("2018-03-09 08:00:00".Utc());
			result.ApprovedPeriods().Single().EndTime.Should().Be("2018-03-09 08:05:00".Utc());
		}
		
		[Test]
		public void ShouldOnlyRemoveOneIfDuplicateApprovedPeriods()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00")
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00")
				.WithRemovedApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00");

			var result = Target.LoadUntilNow(person, "2018-03-09".Date());

			result.ApprovedPeriods().Single().StartTime.Should().Be("2018-03-09 08:00:00".Utc());
			result.ApprovedPeriods().Single().EndTime.Should().Be("2018-03-09 08:05:00".Utc());
		}
		
		[Test]
		public void ShouldOnlyRemoveCorrespondingApprovedPeriod()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 09:05:00")
				.WithApprovedPeriod("2018-03-09 09:00:00", "2018-03-09 09:05:00")
				.WithRemovedApprovedPeriod("2018-03-09 09:00:00", "2018-03-09 09:05:00")
				;

			var result = Target.LoadUntilNow(person, "2018-03-09".Date());

			result.ApprovedPeriods().Single().StartTime.Should().Be("2018-03-09 08:00:00".Utc());
			result.ApprovedPeriods().Single().EndTime.Should().Be("2018-03-09 09:05:00".Utc());
		}

		[Test]
		public void ShouldOnlyRemoveCorrespondingApprovedPeriod_WithWeirdOrder()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-03-09 15:00:00", "2018-03-09 17:00:00")
				.WithApprovedPeriod("2018-03-09 09:00:00", "2018-03-09 09:05:00")
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 09:05:00")
				.WithRemovedApprovedPeriod("2018-03-09 09:00:00", "2018-03-09 09:05:00")
				;

			var result = Target.LoadUntilNow(person, "2018-03-09".Date())
				.ApprovedPeriods()
				.OrderBy(x => x.StartTime);

			result.Select(x => x.StartTime).Should().Have.SameSequenceAs(new[] {"2018-03-09 08:00:00".Utc(), "2018-03-09 15:00:00".Utc()});
			result.Select(x => x.EndTime).Should().Have.SameSequenceAs(new[] {"2018-03-09 09:05:00".Utc(), "2018-03-09 17:00:00".Utc()});
		}

		[Test]
		public void ShouldNotRemoveTwoApprovedPeriods()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 09:05:00")
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 09:05:00")
				;

			var result = Target.LoadUntilNow(person, "2018-03-09".Date())
				.ApprovedPeriods();

			result.Select(x => x.StartTime).Should().Have.SameSequenceAs(new[] {"2018-03-09 08:00:00".Utc(), "2018-03-09 08:00:00".Utc()});
			result.Select(x => x.EndTime).Should().Have.SameSequenceAs(new[] {"2018-03-09 09:05:00".Utc(), "2018-03-09 09:05:00".Utc()});
		}
		
		[Test]
		public void ShouldRemoveApprovedPeriodWithDuplicateRemovals()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00")
				.WithRemovedApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00")
				.WithRemovedApprovedPeriod("2018-03-09 08:00:00", "2018-03-09 08:05:00");

			var result = Target.LoadUntilNow(person, "2018-03-09".Date());

			result.ApprovedPeriods().Should().Be.Empty();
		}
	}
}