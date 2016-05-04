using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Rta.Persisters;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ScheduleProjectionReadOnly
{
	[PrincipalAndStateTest]
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
		[Ignore]
		public void ShouldNotRetryWhenMultipleWorkersUpdatesSameModel([Range(0, 0)] int ladfkja)
		{
			var date = "2016-05-02".Date();
			var dates = Enumerable.Range(0, 20).Select(i => date.AddDays(i)).ToArray();
			var personId = Guid.NewGuid();
			var scenarioid = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();
			var time = "2016-05-01 08:00".Utc();

			dates.ForEach(d => simulator.ProcessAsync(() => Service.AddState(d, scenarioid, personId, time = time.AddMinutes(1))));
			simulator.WaitForAll();

			dates.ForEach(d => 20.Times(() => simulator.ProcessAsync(() => Service.AddState(d, scenarioid, personId, time = time.AddMinutes(1)))));
			simulator.WaitForAll();

			simulator.RetryCount.Should().Be(0);
			dates.ForEach(d => Service.Get(d, personId, scenarioid).Should().Have.Count.EqualTo(3));
		}

		[Test]
		[Ignore]
		public void ShouldCalculateCorrectlyWhenMultipleWorkersAddTheSameModel([Range(0, 0)] int ladfkja)
		{
			var date = "2016-05-02".Date();
			var dates = Enumerable.Range(0, 20).Select(i => date.AddDays(i)).ToArray();
			var personId = Guid.NewGuid();
			var scenarioid = Guid.NewGuid();
			var simulator = new RetryingQueueSimulator();
			var time = "2016-05-01 08:00".Utc();

			dates.ForEach(d => 20.Times(() => simulator.ProcessAsync(() => Service.AddState(d, scenarioid, personId, time = time.AddMinutes(1)))));
			simulator.WaitForAll();

			dates.ForEach(d => Service.Get(d, personId, scenarioid).Should().Have.Count.EqualTo(3));
		}

		public class TheService
		{
			private readonly IScheduleProjectionReadOnlyPersister _persister;

			public TheService(IScheduleProjectionReadOnlyPersister persister)
			{
				_persister = persister;
			}

			[UnitOfWork]
			public virtual void AddState(DateOnly date, Guid scenarioId, Guid personId, DateTime scheduleLoadedTimeStamp)
			{
				Enumerable.Range(-1, 2)
					.Select(date.AddDays)
					.ForEach(d =>
					{
						_persister.ClearDayForPerson(d, scenarioId, personId, scheduleLoadedTimeStamp);
						3.Times(() =>
						{
							_persister.AddProjectedLayer(new ScheduleProjectionReadOnlyModel
							{
								PersonId = personId,
								ScenarioId = scenarioId,
								BelongsToDate = d,
								ScheduleLoadedTime = scheduleLoadedTimeStamp,
								StartDateTime = "2016-05-04 08:00".Utc(),
								EndDateTime = "2016-05-04 08:00".Utc()
							});
						});
						//_persister.GetNextActivityStartTime(scheduleLoadedTimeStamp, personId);
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