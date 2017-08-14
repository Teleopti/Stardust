using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	[DomainTest]
	public class ScheduleChangedInDefaultScenarioNotificationTest
	{
		public ScheduleChangedInDefaultScenarioNotification Target;
		public FakeMessageSender Sender;

		[Test]
		public void ShouldSendMessage()
		{
			var person = Guid.NewGuid();

			Target.Handle(new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				PersonId = person,
				ScheduleDays = new[]
				{
					new ProjectionChangedEventScheduleDay
					{
						Date = "2016-06-03".Utc(),
						Shift = new ProjectionChangedEventShift
						{
							StartDateTime = "2016-06-03 08:00".Utc(),
							EndDateTime = "2016-06-03 17:00".Utc()
						}
					}
				}
			});

			Sender.NotificationsOfDomainType<IScheduleChangedInDefaultScenario>()
				.Should().Have.Count.EqualTo(1);
		}


		[Test]
		public void ShouldNotWhenNoSchedule()
		{
			var person = Guid.NewGuid();

			Target.Handle(new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				PersonId = person,
				ScheduleDays = new ProjectionChangedEventScheduleDay[] { }
			});

			Sender.AllNotifications.Should().Be.Empty();
		}
	}
}