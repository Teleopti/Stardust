using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States.Events;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[DatabaseTest]
	public class RtaEventStoreTest
	{
		public IEventPublisher Target;
		public IRtaEventStoreTester Events;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldStore()
		{
			Target.Publish(new PersonStateChangedEvent());

			var actual = WithUnitOfWork.Get(() => Events.LoadAllForTest());

			actual.Should().Not.Be.Empty();
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

			var result = (PersonStateChangedEvent) WithUnitOfWork.Get(() => Events.LoadAllForTest()).Single();
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

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
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

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
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

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Cast<PersonStateChangedEvent>()
				.Select(x => x.PersonId)
				.Should().Have.SameSequenceAs(new[] {first, second});
		}

		[Test]
		public void ShouldStorePersonRuleChangedEvent()
		{
			Target.Publish(new PersonRuleChangedEvent());

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldStorePeriodApprovedAsInAdherenceEvent()
		{
			Target.Publish(new PeriodApprovedAsInAdherenceEvent());

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldStorePersonArrivalAfterLateForWorkEvent()
		{
			WithUnitOfWork.Do(() => Target.Publish(new PersonArrivedLateForWorkEvent()));

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldStoreApprovedPeriodRemovedEvent()
		{
			WithUnitOfWork.Do(() => Target.Publish(new ApprovedPeriodRemovedEvent()));

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldStorePersonAdherenceDayStartEvent()
		{
			WithUnitOfWork.Do(() => Target.Publish(new PersonAdherenceDayStartEvent()));

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldStorePersonShiftEndEvent()
		{
			WithUnitOfWork.Do(() => Target.Publish(new PersonShiftEndEvent()));

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldStorePersonShiftStartEvent()
		{
			WithUnitOfWork.Do(() => Target.Publish(new PersonShiftStartEvent()));

			WithUnitOfWork.Get(() => Events.LoadAllForTest())
				.Single().Should().Not.Be.Null();
		}
	}
}