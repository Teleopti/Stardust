﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class RecordedAdherenceTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldCloseOutOfAdherenceOnFirstChange()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-08 08:00", Adherence.Out);
			Database.WithHistoricalStateChange("2017-12-08 10:00", Adherence.In);
			Database.WithHistoricalStateChange("2017-12-08 12:00", Adherence.Neutral);

			var result = Target.Load(person);

			result.RecordedOutOfAdherences().First().StartTime.Should().Be("2017-12-08 08:00".Utc());
			result.RecordedOutOfAdherences().First().EndTime.Should().Be("2017-12-08 10:00".Utc());
		}

		[Test]
		public void ShouldCloseOutOfAdherencesFirstChange()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				.WithHistoricalStateChange("2017-12-08 08:00", Adherence.Out)
				.WithHistoricalStateChange("2017-12-08 09:00", Adherence.In)
				.WithHistoricalStateChange("2017-12-08 10:00", Adherence.Neutral)
				.WithHistoricalStateChange("2017-12-08 11:00", Adherence.Out)
				.WithHistoricalStateChange("2017-12-08 12:00", Adherence.Neutral)
				.WithHistoricalStateChange("2017-12-08 13:00", Adherence.In)
				;

			var result = Target.Load(person);

			result.RecordedOutOfAdherences().First().StartTime.Should().Be("2017-12-08 08:00".Utc());
			result.RecordedOutOfAdherences().First().EndTime.Should().Be("2017-12-08 09:00".Utc());
			result.RecordedOutOfAdherences().Second().StartTime.Should().Be("2017-12-08 11:00".Utc());
			result.RecordedOutOfAdherences().Second().EndTime.Should().Be("2017-12-08 12:00".Utc());
		}

		[Test, Ignore("Later")]
		public void ShouldCloseOutOfAdherenceEndingInTheFuture()
		{
			Now.Is("2017-12-08 20:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				.WithHistoricalStateChange("2017-12-08 08:00", Adherence.Out)
				.WithHistoricalStateChange("2017-12-10 12:00", Adherence.In);

			var result = Target.Load(person);

			result.RecordedOutOfAdherences().First().StartTime.Should().Be("2017-12-08 08:00".Utc());
			result.RecordedOutOfAdherences().First().EndTime.Should().Be("2017-12-10 12:00".Utc());
		}
	}
}