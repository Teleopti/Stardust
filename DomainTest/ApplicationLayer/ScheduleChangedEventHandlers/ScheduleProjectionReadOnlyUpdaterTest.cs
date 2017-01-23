﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	[DomainTest]
	public class ScheduleProjectionReadOnlyUpdaterTest
	{
		public ScheduleProjectionReadOnlyUpdater Target;
		public FakeScheduleProjectionReadOnlyPersister Persister;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldPersistScheduleProjection()
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
						Date = "2016-04-29".Utc(),
						Shift = new ProjectionChangedEventShift
						{
							StartDateTime = "2016-04-29 08:00".Utc(),
							EndDateTime = "2016-04-29 17:00".Utc(),
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

			var layer = Persister.ForPerson("2016-04-29".Date(), person, Guid.Empty).Single();
			layer.Name.Should().Be("Phone");
			layer.StartDateTime.Should().Be("2016-04-29 08:00".Utc());
			layer.EndDateTime.Should().Be("2016-04-29 17:00".Utc());
		}
		
	}
}