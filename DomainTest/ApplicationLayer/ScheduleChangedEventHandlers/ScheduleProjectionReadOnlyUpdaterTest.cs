﻿using System;
using System.Linq;
using Autofac;
using Autofac.Core.Registration;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[Toggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	[DomainTest]
	public class ScheduleProjectionReadOnlyUpdaterTest
	{
		public FakeScheduleProjectionReadOnlyPersister Persister;
		public FakeEventPublisher EventPublisher;
		public IComponentContext TempContainer;
		
		[Test]
		public void MustNotUseOldHandler()
		{
			Assert.Throws<ComponentNotRegisteredException>(() => TempContainer.Resolve<ScheduleProjectionReadOnlyUpdater>());
		}
		
		[Test]
		public void ShouldPersistScheduleProjection()
		{
			var target = new ScheduleReadModelWrapperHandler(null, null, TempContainer.Resolve<ScheduleProjectionReadOnlyChecker>(), null);
			var person = Guid.NewGuid();

			target.Handle(new ProjectionChangedEventNew
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
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	[ToggleOff(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	[DomainTest]
	public class ScheduleProjectionReadOnlyUpdaterTestOLD
	{
		public ScheduleProjectionReadOnlyUpdater Target;
		public FakeScheduleProjectionReadOnlyPersister Persister;
		public FakeEventPublisher EventPublisher;
		public IComponentContext TempContainer;
		
		[Test]
		public void MustNotUseNewHandler()
		{
			Assert.Throws<ComponentNotRegisteredException>(() => TempContainer.Resolve<ScheduleReadModelWrapperHandler>());
		}
		
		[Test]
		public void ShouldPersistScheduleProjectionOld()
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