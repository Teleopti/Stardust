using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class PeriodTest
	{
		public Domain.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldBuildPeriod()
		{
			Now.Is("2018-01-02 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name")
				.WithAssignment(person, "2018-01-02");

			var viewModel = Target.Build(person);

			viewModel.Period.StartDate.Should().Be("20171227");
			viewModel.Period.EndDate.Should().Be("20180102");
		}
	}
}