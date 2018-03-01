using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class PercentageTest
	{
		public Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldBuildAdherencePercentage()
		{
			Now.Is("2017-12-08 18:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 17:00")
				.WithAdherenceIn("2017-12-08 08:00")
				;

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(100);
		}
	}
}