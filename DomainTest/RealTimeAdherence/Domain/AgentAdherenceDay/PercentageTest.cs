﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class PercentageTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldBuildAdherencePercentage()
		{
			Now.Is("2017-12-31 00:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 17:00")
				.WithHistoricalStateChange("2017-12-08 08:00", Adherence.In);

			var result = Target.LoadUntilNow(person, "2017-12-08".Date());

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldCalculateWhenInBeforeShift()
		{
			Now.Is("2017-12-31 00:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				.WithHistoricalStateChange("2017-12-08 07:30", Adherence.In);

			var result = Target.LoadUntilNow(person, "2017-12-08".Date());

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldCalculateWhenInMultipleTimes()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				.WithHistoricalStateChange("2017-12-08 07:30", Adherence.In)
				.WithHistoricalStateChange("2017-12-08 08:30", Adherence.In)
				.WithHistoricalStateChange("2017-12-08 09:30", Adherence.In);

			var result = Target.LoadUntilNow(person, "2017-12-08".Date());

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldCalculateWhenOutMultipleTimes()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				.WithHistoricalStateChange("2017-12-08 07:30", Adherence.Out)
				.WithHistoricalStateChange("2017-12-08 08:30", Adherence.Out)
				.WithHistoricalStateChange("2017-12-08 12:00", Adherence.In);

			var result = Target.Load(person);

			result.Percentage().Should().Be(50);
		}

		[Test]
		public void ShouldCalculateWhenInInTheMorning()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-08 08:00", Adherence.In);
			Database.WithHistoricalStateChange("2017-12-08 12:00", Adherence.Out);

			var result = Target.Load(person);

			result.Percentage().Should().Be(50);
		}

		[Test]
		public void ShouldCalculateWhenOutInTheAfternoon()
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
			Database.WithHistoricalStateChange("2017-12-08 14:00", Adherence.In);

			var result = Target.Load(person);

			result.Percentage().Should().Be(25);
		}

		[Test]
		public void ShouldCalculateWhenInIn2Periods()
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
			Database.WithHistoricalStateChange("2017-12-08 12:00", Adherence.Out);
			Database.WithHistoricalStateChange("2017-12-08 14:00", Adherence.In);
			Database.WithHistoricalStateChange("2017-12-08 16:00", Adherence.Out);

			var result = Target.Load(person);

			result.Percentage().Should().Be(50);
		}

		[Test]
		public void ShouldCalculateWhenOutInThePast()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-01 00:00", Adherence.Out);

			var result = Target.Load(person);

			result.Percentage().Should().Be(0);
		}

		[Test]
		public void ShouldCalculateWhenInInThePast()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-01 00:00", Adherence.In);

			var result = Target.Load(person);

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldCalculateWhenOutBeforeShift()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-08 07:30", Adherence.In);
			Database.WithHistoricalStateChange("2017-12-08 07:31", Adherence.Out);

			var result = Target.Load(person);

			result.Percentage().Should().Be(0);
		}

		[Test]
		public void ShouldCalculateWhenInAfterShift()
		{
			Now.Is("2017-12-08 18:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-08 08:00", Adherence.Out);
			Database.WithHistoricalStateChange("2017-12-08 16:30", Adherence.In);

			var result = Target.Load(person);

			result.Percentage().Should().Be(0);
		}

		[Test]
		public void ShouldNotCalculateWhenNoShift()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				;

			var result = Target.Load(person);
			result.Percentage().Should().Be(null);
		}

		[Test]
		public void ShouldCalculateUpUntilNow()
		{
			Now.Is("2017-12-08 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				.WithHistoricalStateChange("2017-12-08 08:00", Adherence.In);

			var result = Target.Load(person);

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldExcludeNeutralInTheMorning()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-08 08:00", Adherence.Neutral);
			Database.WithHistoricalStateChange("2017-12-08 12:00", Adherence.In);

			var result = Target.Load(person);

			result.Percentage().Should().Be(100);
		}

		[Test]
		public void ShouldNotExcludeNeutralInTheMorning()
		{
			Now.Is("2017-12-08 23:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 09:00", "2017-12-08 20:00")
				.WithHistoricalStateChange("2017-12-08 06:00", Adherence.Neutral)
				.WithHistoricalStateChange("2017-12-08 10:00", Adherence.Out)
				.WithHistoricalStateChange("2017-12-08 11:00", Adherence.In)
				;

			var result = Target.Load(person);

			result.Percentage().Should().Be(90);
		}

		[Test]
		public void ShouldExcludeNeutralInTheAfternoon()
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

			result.Percentage().Should().Be(50);
		}

		[Test]
		public void ShouldExcludeNeutralWhenNeutralMultipleTimes()
		{
			Now.Is("2017-12-08 17:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-08")
				.WithActivity(null, "phone")
				.WithAssignedActivity("2017-12-08 08:00", "2017-12-08 16:00")
				;
			Database.WithHistoricalStateChange("2017-12-08 07:30", Adherence.Neutral);
			Database.WithHistoricalStateChange("2017-12-08 08:30", Adherence.Neutral);
			Database.WithHistoricalStateChange("2017-12-08 14:00", Adherence.In);

			var result = Target.Load(person);

			result.Percentage().Should().Be(100);
		}
	}

	public static class AgentAdherenceDayLoaderEx
	{
		public static Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay.IAgentAdherenceDay Load(this IAgentAdherenceDayLoader loader, Guid personId)
		{
			return loader.LoadUntilNow(personId, new DateOnly(ServiceLocatorForEntity.Now.UtcDateTime().Date));
		}
	}
}