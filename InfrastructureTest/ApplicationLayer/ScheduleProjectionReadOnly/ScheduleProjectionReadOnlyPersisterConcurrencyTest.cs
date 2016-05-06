using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ScheduleProjectionReadOnly
{
	[PrincipalAndStateTest]
	[Ignore]
	public class ScheduleProjectionReadOnlyPersisterConcurrencyTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TheService>();
		}

		public IScheduleProjectionReadOnlyPersister Persister;
		public WithUnitOfWork WithUnitOfWork;
		public TheService Service;

		[Test]
		public void ShouldNotRetryWhenMultipleWorkersUpdatesSameDay()
		{
			var date = "2016-05-02".Date();
			var dates = Enumerable.Range(0, 100).Select(i => date.AddDays(i)).ToArray();
			var personId = Guid.NewGuid();
			var scenarioid = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();
			var timestamp = "2016-05-01 08:00".Utc();
			
			dates.ForEach(d =>
			{
				Service.Update(d, scenarioid, personId, timestamp);
			});

			dates.ForEach(d =>
				20.Times(() =>
				{
					timestamp = timestamp.AddMinutes(1);
					var ts = timestamp;
					simulator.ProcessAsync(() =>
					{
						Service.Update(d, scenarioid, personId, ts);
					});
				})
				);
			simulator.WaitForAll();

			dates.ForEach(d =>
			{
				Assert.That(Service.Get(d, personId, scenarioid).Count(), Is.EqualTo(3), d.ToString());
			});
			simulator.RetryCount.Should().Be(0);
		}

		[Test]
		public void ShouldUpdateCorrectlyWhenMultipleWorkersAddTheSameDay()
		{
			var date = "2016-05-02".Date();
			var dates = Enumerable.Range(0, 100).Select(i => date.AddDays(i)).ToArray();
			var personId = Guid.NewGuid();
			var scenarioid = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();
			var timestamp = "2016-05-01 08:00".Utc();

			dates.ForEach(d =>
				20.Times(() =>
				{
					timestamp = timestamp.AddMinutes(1);
					var ts = timestamp;
					simulator.ProcessAsync(() =>
					{
						Service.Update(d, scenarioid, personId, ts);
					});
				})
				);
			simulator.WaitForAll();

			dates.ForEach(d =>
			{
				Assert.That(Service.Get(d, personId, scenarioid).Count(), Is.EqualTo(3), d.ToString());
			});
		}

		public class TheService
		{
			private readonly IScheduleProjectionReadOnlyPersister _persister;

			public TheService(IScheduleProjectionReadOnlyPersister persister)
			{
				_persister = persister;
			}

			[UnitOfWork]
			public virtual void Update(DateOnly date, Guid scenarioId, Guid personId, DateTime scheduleLoadedTimeStamp)
			{
				_persister.ClearDayForPerson(date, scenarioId, personId, scheduleLoadedTimeStamp);
				3.Times(() =>
				{
					_persister.AddProjectedLayer(new ScheduleProjectionReadOnlyModel
					{
						PersonId = personId,
						ScenarioId = scenarioId,
						BelongsToDate = date,
						ScheduleLoadedTime = scheduleLoadedTimeStamp,
						StartDateTime = "2016-05-04 08:00".Utc(),
						EndDateTime = "2016-05-04 08:00".Utc()
					});
				});
			}

			[UnitOfWork]
			public virtual IEnumerable<ScheduleProjectionReadOnlyModel> Get(DateOnly date, Guid personId, Guid scenarioId)
			{
				return _persister.ForPerson(date, personId, scenarioId);
			}
		}
	}
}