using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class ApprovedPeriodsTest
	{
		public Adherence.Historical.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeUserTimeZone TimeZone;

		[Test]
		public void ShouldGetForAgent()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-01-29 08:05:00", "2018-01-29 08:15:00");

			var viemModel = Target.Build(person, "2018-01-29".Date());

			viemModel.ApprovedPeriods.Single().StartTime.Should().Be("2018-01-29T08:05:00");
			viemModel.ApprovedPeriods.Single().EndTime.Should().Be("2018-01-29T08:15:00");
		}

		[Test]
		public void ShouldGetMultiplePeriodsForAgent()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-01-30 08:05:00", "2018-01-30 08:15:00")
				.WithApprovedPeriod("2018-01-30 10:00:00", "2018-01-30 10:30:00");

			var viewModel = Target.Build(person, "2018-01-30".Date());

			viewModel.ApprovedPeriods.First().StartTime.Should().Be("2018-01-30T08:05:00");
			viewModel.ApprovedPeriods.First().EndTime.Should().Be("2018-01-30T08:15:00");
			viewModel.ApprovedPeriods.Last().StartTime.Should().Be("2018-01-30T10:00:00");
			viewModel.ApprovedPeriods.Last().EndTime.Should().Be("2018-01-30T10:30:00");
		}
		
		[Test]
		public void ShouldGetForAgentInStockholm()
		{
			TimeZone.IsSweden();
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-01-31 08:05:00", "2018-01-31 08:15:00");

			var viewModel = Target.Build(person, "2018-01-31".Date());

			viewModel.ApprovedPeriods.First().StartTime.Should().Be("2018-01-31T09:05:00");
			viewModel.ApprovedPeriods.First().EndTime.Should().Be("2018-01-31T09:15:00");
		}
	}
}