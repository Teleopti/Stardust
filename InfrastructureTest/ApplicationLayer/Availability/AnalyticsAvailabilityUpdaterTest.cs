using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Availability;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Availability
{
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	[Toggle(Toggles.ETL_EventbasedDate_39562)]
	public class AnalyticsAvailabilityUpdaterTest
	{
		public AnalyticsAvailabilityUpdater Target;
		public IPersonRepository PersonRepository;
		public WithUnitOfWork WithUnitOfWork;

		[Timeout(10000)]
		[Test]
		[Ignore("will be fixed soon")]
		public void ShouldNotHangWhenMultipleThreadsCallingMultipleDates()
		{
			var targetDate = new DateTime(2010, 01, 05);
			var cts = new CancellationTokenSource();
			var person = PersonFactory.CreatePerson(new Name("_", "_"));

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person);
			});

			var taskToRunInParallell = new Func<Task>(() =>
			{
				return Task.Factory.StartNew(() =>
				{
					while (!cts.IsCancellationRequested)
					{
						//we not commit here - the bug will only occur when date is not present in db
						Target.Handle(new ScheduleChangedEvent
						{
							StartDateTime = targetDate,
							EndDateTime = targetDate.AddDays(1),
							PersonId = person.Id.Value
						});
					}
				}, cts.Token);
			});
			var tasks = new[] {taskToRunInParallell(), taskToRunInParallell()};

			Thread.Sleep(500);
			cts.Cancel();
			Task.WaitAll(tasks);
		}
	}
}
