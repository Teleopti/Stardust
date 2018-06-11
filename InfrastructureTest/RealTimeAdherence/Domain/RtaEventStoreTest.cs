﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
	[Toggle(Toggles.RTA_StoreEvents_47721)]
	[Toggle(Toggles.RTA_RemoveApprovedOOA_47721)]
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreTest
	{
		public IEventPublisher Target;
		public IRtaEventStoreTestReader Events;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldStore()
		{
			Target.Publish(new PersonStateChangedEvent());

			var actal = WithUnitOfWork.Get(() => Events.LoadAll());

			actal.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldStoreWithProperties()
		{
			var personId = Guid.NewGuid();
			var stateGroupId = Guid.NewGuid();
			Target.Publish(new PersonStateChangedEvent
			{
				PersonId = personId,
				BelongsToDate = "2018-03-06".Date(),
				Timestamp = "2018-03-06 08:00".Utc(),
				StateName = "phone",
				StateGroupId = stateGroupId,
				ActivityName = "phone activity",
				ActivityColor = 255,
				RuleName = "Blah",
				RuleColor = -4,
				Adherence = null
			});

			var result = (PersonStateChangedEvent) WithUnitOfWork.Get(() => Events.LoadAll()).Single();
			result.PersonId.Should().Be(personId);
			result.BelongsToDate.Should().Be("2018-03-06".Date());
			result.Timestamp.Should().Be("2018-03-06 08:00".Utc());
			result.StateName.Should().Be("phone");
			result.StateGroupId.Should().Be(stateGroupId);
			result.ActivityName.Should().Be("phone activity");
			result.ActivityColor.Should().Be(255);
			result.RuleName.Should().Be("Blah");
			result.RuleColor.Should().Be(-4);
			result.Adherence.Should().Be(null);
		}

		[Test]
		public void ShouldWorkWithOpenUnitOfWork()
		{
			WithUnitOfWork.Do(() => Target.Publish(new PersonStateChangedEvent()));

			WithUnitOfWork.Get(() => Events.LoadAll())
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotRollbackWithOpenUnitOfWork()
		{
			Assert.Throws<Exception>(() =>
			{
				WithUnitOfWork.Do(() =>
				{
					Target.Publish(new PersonStateChangedEvent());
					throw new Exception();
				});
			});

			WithUnitOfWork.Get(() => Events.LoadAll())
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldStoreMultipleEvents()
		{
			var first = Guid.NewGuid();
			var second = Guid.NewGuid();

			Target.Publish(
				new PersonStateChangedEvent {PersonId = first},
				new PersonStateChangedEvent {PersonId = second}
			);

			WithUnitOfWork.Get(() => Events.LoadAll())
				.Cast<PersonStateChangedEvent>()
				.Select(x => x.PersonId)
				.Should().Have.SameSequenceAs(new[] {first, second});
		}

		[Test]
		public void ShouldStoreAnyTypeOfEvent()
		{
			var id = Guid.NewGuid();

			Target.Publish(new TestEvent {Id = id});

			WithUnitOfWork.Get(() => Events.LoadAll())
				.Cast<TestEvent>()
				.Single().Id.Should().Be(id);
		}

		[Test]
		public void ShouldStorePersonRuleChangedEvent()
		{
			Target.Publish(new PersonRuleChangedEvent());

			WithUnitOfWork.Get(() => Events.LoadAll())
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldStorePeriodApprovedAsInAdherenceEvent()
		{
			Target.Publish(new PeriodApprovedAsInAdherenceEvent());

			WithUnitOfWork.Get(() => Events.LoadAll())
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldStorePersonArrivalAfterLateForWorkEvent()
		{
			WithUnitOfWork.Do(() => Target.Publish(new PersonArrivedLateForWorkEvent()));

			WithUnitOfWork.Get(() => Events.LoadAll())
				.Should().Not.Be.Empty();
		}

		public class TestEvent : IRtaStoredEvent, IEvent
		{
			public Guid Id;
			public QueryData QueryData() => new QueryData();
		}
	}
}