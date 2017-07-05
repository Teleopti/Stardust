using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.PersonScheduleDayReadModel
{
	[PrincipalAndStateTest]
	[Explicit]
	[Category("LongRunning")]
	public class PersonScheduleDayReadModelUpdaterHangfireConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FakeMessageSender>();
		}

		public PersonScheduleDayReadModelUpdaterHangfire Handler;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonScheduleDayReadModelFinder Finder;

		[Test]
		public void ShouldNotRetryWhenMultipleWorkersUpdatesSameDay()
		{
			var date = "2016-05-02".Date();
			var dates = Enumerable.Range(0, 100).Select(i => date.AddDays(i)).ToArray();
			var personId = SetupFixtureForAssembly.loggedOnPerson.Id.GetValueOrDefault();
			var scenarioId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();
			var timestamp = "2016-05-01 08:00".Utc();

			// Fill the table initially to get only updates
			dates.ForEach(d =>
			{
				var ts = timestamp = timestamp.AddMinutes(1);
				Handler.Handle(new ProjectionChangedEvent
				{
					PersonId = personId,
					LogOnBusinessUnitId = businessUnitId,
					ScenarioId = scenarioId,
					IsInitialLoad = false,
					IsDefaultScenario = true,
					ScheduleDays = new List<ProjectionChangedEventScheduleDay>
								{
									new ProjectionChangedEventScheduleDay
									{
										Date = d.Date,
										Version = 0
									}
								},
					Timestamp = ts,
					ScheduleLoadTimestamp = ts
				});
			});
			dates.ForEach(d =>
				20.Times(v =>
				{
					var ts = timestamp = timestamp.AddMinutes(1);
					simulator.ProcessAsync(() =>
					{
						Handler.Handle(new ProjectionChangedEvent
						{
							PersonId = personId,
							LogOnBusinessUnitId = businessUnitId,
							ScenarioId = scenarioId,
							IsInitialLoad = false,
							IsDefaultScenario = true,
							ScheduleDays = new List<ProjectionChangedEventScheduleDay>
								{
									new ProjectionChangedEventScheduleDay
									{
										Date = d.Date,
										Version = v
									}
								},
							Timestamp = ts,
							ScheduleLoadTimestamp = ts
						});

					});
				})
				);
			simulator.WaitForAll();

			simulator.RetryCount.Should().Be(0);

			var readModels = WithUnitOfWork.Get(() => Finder.ForPerson(dates.First(), dates.Last(), personId));
			readModels.Count().Should().Be.EqualTo(dates.Length);
		}
	}
}