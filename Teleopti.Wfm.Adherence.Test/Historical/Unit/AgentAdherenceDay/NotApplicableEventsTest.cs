using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class NotApplicableEventsTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;
		public FakeRtaEventStore Events;

		[Test]
		public void ShouldLoadWithEventsThatDoesntApply()
		{
			Now.Is("2018-10-22 15:00");
			var person = Guid.NewGuid();
			Events.Add(new UnknownTestEvent {PersonId = person}, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			History
				.ShiftStart(person, "2018-10-22 09:00", "2018-10-22 17:00")
				.ShiftEnd(person, "2018-10-22 09:00", "2018-10-22 17:00")
				.AdherenceDayStart(person, "2018-10-22 09:00", null, null, null, null, null, null)
				;

			Assert.DoesNotThrow(() => { Target.LoadUntilNow(person, "2018-10-22".Date()); });
		}
	}

	public class UnknownTestEvent : IEvent, IRtaStoredEvent
	{
		public Guid PersonId;

		public QueryData QueryData()
		{
			return new QueryData
			{
				PersonId = PersonId,
				BelongsToDate = "2018-10-22".Date(),
				StartTime = "2018-10-22 09:00".Utc(),
				EndTime = "2018-10-22 09:00".Utc()
			};
		}
	}
}