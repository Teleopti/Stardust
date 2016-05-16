using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ScheduleProjectionReadOnly
{
	[DatabaseTest]
	[Toggle(Toggles.RTA_ScheduleProjectionReadOnlyHangfire_35703)]
	public class ScheduleProjectionReadOnlyUpdaterTest
	{
		public ScheduleProjectionReadOnlyUpdater Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldNotPersistOldSchedule()
		{
			var person = Guid.NewGuid();
			Target.Handle(new ProjectionChangedEvent
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

			Target.Handle(new ProjectionChangedEvent
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

			WithUnitOfWork.Get(() => Persister.ForPerson("2016-04-29".Date(), person, Guid.Empty))
				.Should().Have.Count.EqualTo(2);
		}
	}
}