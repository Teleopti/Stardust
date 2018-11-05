﻿using System;
using System.Linq;
using NUnit.Framework;
//using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreLoadLastBeforeTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreReader Events;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldReadFromYesterday()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
				{
					PersonId = personId,
					Timestamp = "2018-03-06 08:00".Utc()
				},
				new PersonStateChangedEvent
				{
					PersonId = personId,
					Timestamp = "2018-03-07 08:00".Utc()
				});

			var actual = WithUnitOfWork.Get(() => Events.LoadLastAdherenceEventBefore(personId, "2018-03-07 08:00".Utc(), DeadLockVictim.No)) as PersonStateChangedEvent;

			actual.Timestamp.Should().Be("2018-03-06 08:00".Utc());
		}
		

		[Test]
		public void ShouldReadLatestFromYesterday()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(new PersonStateChangedEvent
				{
					PersonId = personId,
					Timestamp = "2018-03-06 07:59".Utc()
				},
				new PersonStateChangedEvent
				{
					PersonId = personId,
					Timestamp = "2018-03-06 08:00".Utc()
				});

			var actual = WithUnitOfWork.Get(() => Events.LoadLastAdherenceEventBefore(personId, "2018-03-07 08:00".Utc(), DeadLockVictim.No)) as PersonStateChangedEvent;

			actual.Timestamp.Should().Be("2018-03-06 08:00".Utc());	
		}
		
		[Test]
		public void ShouldOnlyReturnAdhereceEvents()
		{
			var personId = Guid.NewGuid();
			Publisher.Publish(
				new PeriodApprovedAsInAdherenceEvent
				{
					PersonId = personId,
					StartTime = "2018-03-06 09:00".Utc(),
					EndTime = "2018-03-06 10:00".Utc()
				},
				new PersonRuleChangedEvent()
				{
					PersonId = personId,
					Timestamp = "2018-03-06 08:00".Utc()
				});

			var actual = WithUnitOfWork.Get(() => Events.LoadLastAdherenceEventBefore(personId, "2018-03-07 08:00".Utc(), DeadLockVictim.No) as PersonRuleChangedEvent);

			actual.Timestamp.Should().Be("2018-03-06 08:00".Utc());
		}

		[Test]
		public void ShouldReturnNullIfNoEarlierEvent()
		{
			WithUnitOfWork.Get(() => Events.LoadLastAdherenceEventBefore(Guid.NewGuid(), DateTime.MinValue, DeadLockVictim.No))
				.Should().Be.Null();
		}
	}
}