using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ScheduleProjectionReadOnly
{
	[DatabaseTest]
	public class ScheduleProjectionReadOnlyCheckerTest
	{
		public ScheduleProjectionReadOnlyChecker Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldNotPersistOldSchedule()
		{
			var person = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Target.Execute(new ProjectionChangedEvent
				{
					PersonId = person,
					ScheduleDays = new[]
					{
						new ProjectionChangedEventScheduleDay
						{
							Date = "2016-04-29".Utc(),
							Version = 2,
							Shift = new ProjectionChangedEventShift
							{
								Layers = new[]
								{
									new ProjectionChangedEventLayer
									{
										Name = "Phone",
										StartDateTime = "2016-04-29 08:00".Utc(),
										EndDateTime = "2016-04-29 12:00".Utc()
									},
									new ProjectionChangedEventLayer
									{
										Name = "Admin",
										StartDateTime = "2016-04-29 12:00".Utc(),
										EndDateTime = "2016-04-29 17:00".Utc()
									}
								}
							}
						}
					}
				});

				Target.Execute(new ProjectionChangedEvent
				{
					PersonId = person,
					ScheduleDays = new[]
					{
						new ProjectionChangedEventScheduleDay
						{
							Date = "2016-04-29".Utc(),
							Version = 1,
							Shift = new ProjectionChangedEventShift
							{
								Layers = new[]
								{
									new ProjectionChangedEventLayer
									{
										Name = "Phone",
										StartDateTime = "2016-04-29 08:00".Utc(),
										EndDateTime = "2016-04-29 17:00".Utc()
									}
								}
							}
						}
					}
				});
			});
			

			WithUnitOfWork.Get(() => Persister.ForPerson("2016-04-29".Date(), person, Guid.Empty))
				.Should().Have.Count.EqualTo(2);
		}
	}
}