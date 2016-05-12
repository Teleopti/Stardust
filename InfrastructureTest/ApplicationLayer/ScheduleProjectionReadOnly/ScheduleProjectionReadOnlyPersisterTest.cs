using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ScheduleProjectionReadOnly
{
	[UnitOfWorkWithLoginTest]
	public class ScheduleProjectionReadOnlyPersisterTest
	{
		public IScheduleProjectionReadOnlyPersister Persister;

		[Test]
		public void ShouldAddLayer()
		{
			var scenario = Guid.NewGuid();
			var person = Guid.NewGuid();

			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = "2016-04-29".Date(),
					ScenarioId = scenario,
					PersonId = person,
					StartDateTime = "2016-04-29 8:00".Utc(),
					EndDateTime = "2016-04-29 17:00".Utc()
				});

			Persister.ForPerson("2016-04-29".Date(), person, scenario).Should().Have.Count.EqualTo(1);
		}


		[Test]
		public void ShouldPersistWithProperties()
		{
			var scenario = Guid.NewGuid();
			var person = Guid.NewGuid();
			var payloadId = Guid.NewGuid();

			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = "2016-04-29".Date(),
					ScenarioId = scenario,
					PersonId = person,
					StartDateTime = "2016-04-29 8:00".Utc(),
					EndDateTime = "2016-04-29 17:00".Utc(),
					PayloadId = payloadId,
					WorkTime = "4".Hours(),
					ContractTime = "3".Hours(),
					Name = "Phone",
					ShortName = "Ph",
					DisplayColor = 182,
				});

			var model = Persister.ForPerson("2016-04-29".Date(), person, scenario).Single();
			model.BelongsToDate.Should().Be("2016-04-29".Date());
			model.ScenarioId.Should().Be(scenario);
			model.PersonId.Should().Be(person);
			model.StartDateTime.Should().Be("2016-04-29 8:00".Utc());
			model.EndDateTime.Should().Be("2016-04-29 17:00".Utc());
			model.PayloadId.Should().Be(payloadId);
			model.WorkTime.Should().Be("4".Hours());
			model.ContractTime.Should().Be("3".Hours());
			model.Name.Should().Be("Phone");
			model.ShortName.Should().Be("Ph");
			model.DisplayColor.Should().Be(182);
		}
	}
}