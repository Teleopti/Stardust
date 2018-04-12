using System;
using Autofac;
using Autofac.Core.Registration;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	[DomainTest]
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class ScheduleChangedInDefaultScenarioNotificationTest
	{
		public ScheduleChangedInDefaultScenarioNotification Target;
		public FakeMessageSender Sender;

		public IComponentContext TempContainer;
		[Test]
		[ToggleOff(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
		public void MustNotUseNewHandler()
		{
			Assert.Throws<ComponentNotRegisteredException>(() =>
				TempContainer.Resolve<ScheduleChangedInDefaultScenarioNotificationNew>());
		}
		
		[Test]
		public void ShouldSendMessageInDefaultScenario()
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
		public void ShouldNotSendMessageInNonDefaultScenario()
		{
			var person = Guid.NewGuid();

			Target.Handle(new ProjectionChangedEvent
			{
				IsDefaultScenario = false,
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
				.Should().Have.Count.EqualTo(0);
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