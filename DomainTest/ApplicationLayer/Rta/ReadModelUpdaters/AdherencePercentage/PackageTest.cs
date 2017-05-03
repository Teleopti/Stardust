using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AdherencePercentage
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.HangFire_EventPackages_43924)]
	public class PackageTest
	{
		public FakeAdherencePercentageReadModelPersister Persister;
		public AdherencePercentageReadModelUpdaterWithPackages Target;

		[Test]
		public void ShouldSubscribeToEvents()
		{
			var subscriptionsRegistrator = new SubscriptionsRegistrator();

			Target.Subscribe(subscriptionsRegistrator);

			subscriptionsRegistrator.Has(typeof(PersonInAdherenceEvent)).Should().Be(true);
			subscriptionsRegistrator.Has(typeof(PersonOutOfAdherenceEvent)).Should().Be(true);
			subscriptionsRegistrator.Has(typeof(PersonNeutralAdherenceEvent)).Should().Be(true);
			subscriptionsRegistrator.Has(typeof(PersonShiftStartEvent)).Should().Be(true);
			subscriptionsRegistrator.Has(typeof(PersonShiftEndEvent)).Should().Be(true);
			subscriptionsRegistrator.Has(typeof(PersonDeletedEvent)).Should().Be(true);
		}

		[Test]
		public void ShouldPersist()
		{
			Target.Handle(new []{ new PersonInAdherenceEvent
				{
					PersonId = Guid.NewGuid(),
					Timestamp =  "2017-05-02 8:00".Utc()
				}, new PersonInAdherenceEvent
				{
					PersonId = Guid.NewGuid(),
					Timestamp =  "2017-05-02 8:00".Utc()
				}
			});

			Persister.PersistedModels.Count().Should().Be(2);
		}


		[Test]
		public void ShouldUpdateTimeInAdherenceWhenPersonOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var personId2 = Guid.NewGuid();

			Target.Handle(new[] {
				new PersonInAdherenceEvent
				{
					PersonId = personId,
					Timestamp = "2017-05-02 8:00".Utc()
				},
				new PersonInAdherenceEvent
				{
					PersonId = personId2,
					Timestamp = "2017-05-02 8:00".Utc()
				}});
			Target.Handle(new[] {
				new PersonOutOfAdherenceEvent
				{
					PersonId = personId,
					Timestamp = "2017-05-02 8:10".Utc()
				},
				new PersonOutOfAdherenceEvent
				{
					PersonId = personId2,
					Timestamp = "2017-05-02 8:20".Utc()
				}});

			Persister.PersistedModels.Single(x => x.PersonId == personId).TimeInAdherence.Should().Be("10".Minutes());
			Persister.PersistedModels.Single(x => x.PersonId == personId2).TimeInAdherence.Should().Be("20".Minutes());
			
		}

		[Test]
		public void ShouldPersistShiftStartTime()
		{
			Target.Handle(new[]
				{
					new PersonShiftStartEvent
					{
						PersonId = Guid.NewGuid(),
						ShiftStartTime = "2017-05-02 08:00".Utc()
					}
				}
			);

			Persister.PersistedModel.ShiftStartTime.Should().Be("2017-05-02 08:00".Utc());
		}

		[Test]
		public void ShouldPersistShiftEndTime()
		{
			Target.Handle(new[]
			{
				new PersonShiftStartEvent
				{
					PersonId = Guid.NewGuid(),
					ShiftEndTime = "2017-05-02 17:00".Utc()
				}
			});

			Persister.PersistedModel.ShiftEndTime.Should().Be("2017-05-02 17:00".Utc());
		}


		[Test]
		public void ShouldUpdateAdherenceTimesWhenShiftEnds()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonInAdherenceEvent
				{
					PersonId = personId,
					Timestamp = "2014-05-02 15:00".Utc()
				}
			});
			Target.Handle(new[]
			{
				new PersonOutOfAdherenceEvent
				{
					PersonId = personId,
					Timestamp = "2014-05-02 16:00".Utc()
				}
			});
			Target.Handle(new[]
			{
				new PersonShiftEndEvent
				{
					PersonId = personId,
					ShiftEndTime = "2014-05-02 17:00".Utc()
				}
			});

			Persister.PersistedModel.TimeOutOfAdherence.Should().Be("60".Minutes());
			Persister.PersistedModel.TimeInAdherence.Should().Be("60".Minutes());
		}

		[Test]
		public void ShouldHandleNeutralAdherenceEvent()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new[]
			{
				new PersonNeutralAdherenceEvent
				{
					PersonId = personId
				}
			});

			Persister.PersistedModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDeletePerson()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new[]
			{
				new PersonOutOfAdherenceEvent {PersonId = personId}
			});

			Target.Handle(new[]
			{
				new PersonDeletedEvent {PersonId = personId}
			});

			Persister.PersistedModel.Should().Be.Null();
		}

	}
}