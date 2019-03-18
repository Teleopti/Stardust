using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.AdjustedPeriodsViewModelBuilder
{
	[DomainTest]
	public class AdjustedPeriodTest
	{
		public Adherence.Historical.AdjustedPeriodsViewModelBuilder Target;
		public FakeRtaHistory History;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		[Test]
		public void ShouldGetEmptyModel()
		{
			Target.Build()
				.Should().Have.SameValuesAs(Enumerable.Empty<AdjustedPeriodViewModel>());
		}

		[Test]
		public void ShouldBuild()
		{
			History.AdjustedAdherenceToNeutral("2019-03-05 12:00", "2019-03-05 14:00");

			var data = Target.Build();

			data.Single().StartTime.Should().Be("2019-03-05 12:00");
			data.Single().EndTime.Should().Be("2019-03-05 14:00");
		}

		[Test]
		public void ShouldBuildTwo()
		{
			History
				.AdjustedAdherenceToNeutral("2019-03-05 12:00", "2019-03-05 14:00")
				.AdjustedAdherenceToNeutral("2019-03-05 15:00", "2019-03-05 16:00");

			var data = Target.Build();

			data.First().StartTime.Should().Be("2019-03-05 12:00");
			data.First().EndTime.Should().Be("2019-03-05 14:00");
			data.Second().StartTime.Should().Be("2019-03-05 15:00");
			data.Second().EndTime.Should().Be("2019-03-05 16:00");
		}

		[Test]
		public void ShouldBuildOnDifferentDay()
		{
			History
				.AdjustedAdherenceToNeutral("2019-03-04 12:00", "2019-03-04 14:00");

			var data = Target.Build();

			data.Single().StartTime.Should().Be("2019-03-04 12:00");
			data.Single().EndTime.Should().Be("2019-03-04 14:00");
		}

		[Test]
		public void ShouldBuildWithSwedenTimeZone()
		{
			TimeZone.IsSweden();
			Now.Is("2019-03-06 15:00");
			History.AdjustedAdherenceToNeutral("2019-03-05 12:00", "2019-03-05 14:00");

			var data = Target.Build();

			data.Single().StartTime.Should().Be("2019-03-05 13:00");
			data.Single().EndTime.Should().Be("2019-03-05 15:00");
		}

		[Test]
		public void ShouldRemoveAdjustment()
		{
			History
				.AdjustedAdherenceToNeutral("2019-03-18 10:00", "2019-03-18 11:00")
				.AdjustedAdherenceToNeutral("2019-03-18 12:00", "2019-03-18 14:00")
				.CanceledAdjustment("2019-03-18 12:00", "2019-03-18 14:00");

			var data = Target.Build();

			data.Single().StartTime.Should().Be("2019-03-18 10:00");
			data.Single().EndTime.Should().Be("2019-03-18 11:00");
		}

		[Test]
		public void ShouldRemoveTwoAdjustments()
		{
			History
				.AdjustedAdherenceToNeutral("2019-03-18 10:00", "2019-03-18 11:00")
				.AdjustedAdherenceToNeutral("2019-03-18 12:00", "2019-03-18 14:00")
				.AdjustedAdherenceToNeutral("2019-03-18 15:00", "2019-03-18 16:00")
				.CanceledAdjustment("2019-03-18 12:00", "2019-03-18 14:00")
				.CanceledAdjustment("2019-03-18 15:00", "2019-03-18 16:00");

			var data = Target.Build();

			data.Single().StartTime.Should().Be("2019-03-18 10:00");
			data.Single().EndTime.Should().Be("2019-03-18 11:00");
		}
		
		
		[Test]
		public void ShouldRemoveFirstAdjustment()
		{
			History
				.AdjustedAdherenceToNeutral("2019-03-18 10:00", "2019-03-18 11:00")
				.AdjustedAdherenceToNeutral("2019-03-18 12:00", "2019-03-18 14:00")
				.CanceledAdjustment("2019-03-18 10:00", "2019-03-18 11:00");

			var data = Target.Build();

			data.Single().StartTime.Should().Be("2019-03-18 12:00");
			data.Single().EndTime.Should().Be("2019-03-18 14:00");
		}		
		
		[Test]
		public void ShouldRemoveOnlyOneAdjustment()
		{
			History
				.AdjustedAdherenceToNeutral("2019-03-18 10:00", "2019-03-18 11:00")
				.AdjustedAdherenceToNeutral("2019-03-18 10:00", "2019-03-18 11:00")
				.CanceledAdjustment("2019-03-18 10:00", "2019-03-18 11:00");

			var data = Target.Build();

			data.Single().StartTime.Should().Be("2019-03-18 10:00");
			data.Single().EndTime.Should().Be("2019-03-18 11:00");
		}
	}
}