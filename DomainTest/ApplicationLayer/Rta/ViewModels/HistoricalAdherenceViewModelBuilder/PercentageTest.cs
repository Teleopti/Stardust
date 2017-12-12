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
		public FakeHistoricalAdherenceReadModelPersister ReadModel;
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
				;
			ReadModel.AddIn(person, "2017-12-08 08:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(100);
		}

		[Test]
		public void ShouldCalculateWhenInBeforeShift()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddIn(person, "2017-12-08 07:30".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(100);
		}

		[Test]
		public void ShouldCalculateWhenInMultipleTimes()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddIn(person, "2017-12-08 07:30".Utc());
			ReadModel.AddIn(person, "2017-12-08 08:30".Utc());
			ReadModel.AddIn(person, "2017-12-08 09:30".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(100);
		}
		
		[Test]
		public void ShouldCalculateWhenOutMultipleTimes()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddOut(person, "2017-12-08 07:30".Utc());
			ReadModel.AddOut(person, "2017-12-08 08:30".Utc());
			ReadModel.AddIn(person, "2017-12-08 12:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(50);
		}
		
		[Test]
		public void ShouldCalculateWhenInInTheMorning()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddIn(person, "2017-12-08 08:00".Utc());
			ReadModel.AddOut(person, "2017-12-08 12:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(50);
		}

		[Test]
		public void ShouldCalculateWhenOutInTheAfternoon()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddOut(person, "2017-12-08 08:00".Utc());
			ReadModel.AddIn(person, "2017-12-08 14:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(25);
		}

		[Test]
		public void ShouldCalculateWhenInIn2Periods()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddOut(person, "2017-12-08 08:00".Utc());
			ReadModel.AddIn(person, "2017-12-08 10:00".Utc());
			ReadModel.AddOut(person, "2017-12-08 12:00".Utc());
			ReadModel.AddIn(person, "2017-12-08 14:00".Utc());
			ReadModel.AddOut(person, "2017-12-08 16:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(50);
		}

		[Test]
		public void ShouldCalculateWhenOutInThePast()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddOut(person, "2017-12-01 00:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(0);
		}

		[Test]
		public void ShouldCalculateWhenInInThePast()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddIn(person, "2017-12-01 00:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(100);
		}

		[Test]
		public void ShouldNotCalculateWhenNoShift()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				;

			var viewModel = Target.Build(person);
			viewModel.AdherencePercentage.Should().Be(null);
		}

		[Test]
		public void ShouldCalculateUpUntilNow()
		{
			Now.Is("2017-12-08 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddIn(person, "2017-12-08 08:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(50);
		}

		[Test]
		public void ShouldExcludeNeutralInTheMorning()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddNeutral(person, "2017-12-08 08:00".Utc());
			ReadModel.AddIn(person, "2017-12-08 12:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(100);
		}

		[Test]
		public void ShouldExcludeNeutralInTheAfternoon()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddOut(person, "2017-12-08 08:00".Utc());
			ReadModel.AddIn(person, "2017-12-08 10:00".Utc());
			ReadModel.AddNeutral(person, "2017-12-08 12:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(50);
		}
		
		[Test]
		public void ShouldExcludeNeutralWhenNeutralMultipleTimes()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			ReadModel.AddNeutral(person, "2017-12-08 07:30".Utc());
			ReadModel.AddNeutral(person, "2017-12-08 08:30".Utc());
			ReadModel.AddIn(person, "2017-12-08 14:00".Utc());

			var viewModel = Target.Build(person);

			viewModel.AdherencePercentage.Should().Be(100);
		}
		
	}
}