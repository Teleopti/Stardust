using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Availability;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Concurrency
{
	[Category("BucketB")]
	[MultiDatabaseTest]
	public class AnalyticsAvailabilityUpdaterTest
	{
		public AnalyticsAvailabilityUpdater Target;
		public IPersonRepository PersonRepository;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		[Timeout(15000)]
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
