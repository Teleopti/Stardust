using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class PercentageTest
	{
		public Domain.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder Target;
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