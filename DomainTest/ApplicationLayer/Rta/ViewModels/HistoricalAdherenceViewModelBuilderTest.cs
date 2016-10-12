using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class HistoricalAdherenceViewModelBuilderTest
	{
		public HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeHistoricalAdherenceReadModelPersister ReadModel;
		public MutableNow Now;
		
		[Test]
		public void ShouldGetPerson()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			var name = RandomName.Make();
			Database.WithAgent(person, name);

			var historicalData = Target.Build(person);

			historicalData.PersonId.Should().Be(person);
			historicalData.AgentName.Should().Be(name);
		}

		[Test]
		public void ShouldGetHistoricalDataForAgent()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			ReadModel.Has(new HistoricalAdherenceReadModel
			{
				PersonId = person,
				Date = "2016-10-10".Date(),
				OutOfAdherences = new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						EndTime = "2016-10-10 08:15".Utc(),
						StartTime = "2016-10-10 08:05".Utc()
					}
				}
			});

			var historicalData = Target.Build(person);

			historicalData.OutOfAdherences.Single().StartTime.Should().Be("2016-10-10T08:05:00");
			historicalData.OutOfAdherences.Single().EndTime.Should().Be("2016-10-10T08:15:00");
		}

		[Test]
		public void ShouldGetForCorrectDate()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			var name = RandomName.Make();

			Database
				.WithAgent(person, "name");

			ReadModel
				.Has(new HistoricalAdherenceReadModel
				{
					PersonId = person,
					Date = "2016-10-09".Date()
				})
				.Has(new HistoricalAdherenceReadModel
				{
					PersonId = person,
					Date = "2016-10-10".Date(),
					OutOfAdherences = new[] {new HistoricalOutOfAdherenceReadModel() }
				});

			var historicalData = Target.Build(person);

			historicalData.PersonId.Should().Be(person);
			historicalData.Now.Should().Be("2016-10-10T15:00:00");
			historicalData.OutOfAdherences.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetSchedule()
		{
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name")
				.WithAssignment(person, "2016-10-11")
				.WithActivity(null, ColorTranslator.FromHtml("#80FF80"))
				.WithAssignedActivity("2016-10-11 09:00", "2016-10-11 17:00");

			var historicalData = Target.Build(person);

			historicalData.Schedules.Single().Color.Should().Be("#80FF80");
			historicalData.Schedules.Single().StartTime.Should().Be("2016-10-11T09:00:00");
			historicalData.Schedules.Single().EndTime.Should().Be("2016-10-11T17:00:00");
		}
	}
}