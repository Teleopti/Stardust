using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Availability;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Concurrency
{
	[Category("BucketB")]
	[MultiDatabaseTestAttribute]
	[Toggle(Toggles.ETL_EventbasedDate_39562)]
	public class AnalyticsAvailabilityUpdaterTest
	{
		public AnalyticsAvailabilityUpdater Target;
		public IPersonRepository PersonRepository;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		[Timeout(8000)]
		public void ShouldNotHangWhenMultipleThreadsCallingMultipleDates()
		{
			var targetDate = new DateTime(2010, 01, 05);
			var person = new Person().InTimeZone(TimeZoneInfo.Utc);

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person);
			});

			var taskToRunInParallell = new Func<Task>(() =>
			{
				return Task.Factory.StartNew(() =>
				{
					Target.Handle(new ScheduleChangedEvent
					{
						StartDateTime = targetDate,
						EndDateTime = targetDate.AddDays(1),
						PersonId = person.Id.Value
					});
				});
			});
			var tasks = new[] {taskToRunInParallell(), taskToRunInParallell()};

			Task.WaitAll(tasks);

			WithUnitOfWork.Do(() =>
			{
				PersonRepository.HardRemove(person);
			});
		}
	}
}
