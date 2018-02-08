﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class PercentageTest
	{
		public AgentAdherenceDayLoader Target;
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
				.WithAdherenceIn("2017-12-08 08:00".Utc());

			var result = Target.Load(person, "2017-12-08".Date());

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
				.WithAdherenceIn("2017-12-08 07:30".Utc());

			var result = Target.Load(person, "2017-12-08".Date());

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
				.WithAdherenceIn("2017-12-08 07:30".Utc())
				.WithAdherenceIn("2017-12-08 08:30".Utc())
				.WithAdherenceIn("2017-12-08 09:30".Utc());

			var result = Target.Load(person, "2017-12-08".Date());

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
				.WithAdherenceOut("2017-12-08 07:30".Utc())
				.WithAdherenceOut("2017-12-08 08:30".Utc())
				.WithAdherenceIn("2017-12-08 12:00".Utc());

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
			Database.WithAdherenceIn("2017-12-08 08:00".Utc());
			Database.WithAdherenceOut("2017-12-08 12:00".Utc());

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
			Database.WithAdherenceOut("2017-12-08 08:00".Utc());
			Database.WithAdherenceIn("2017-12-08 14:00".Utc());

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
			Database.WithAdherenceOut("2017-12-08 08:00".Utc());
			Database.WithAdherenceIn("2017-12-08 10:00".Utc());
			Database.WithAdherenceOut("2017-12-08 12:00".Utc());
			Database.WithAdherenceIn("2017-12-08 14:00".Utc());
			Database.WithAdherenceOut("2017-12-08 16:00".Utc());

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
			Database.WithAdherenceOut("2017-12-01 00:00".Utc());

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
			Database.WithAdherenceIn("2017-12-01 00:00".Utc());

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
			Database.WithAdherenceIn("2017-12-08 07:30".Utc());
			Database.WithAdherenceOut("2017-12-08 07:31".Utc());

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
			Database.WithAdherenceOut("2017-12-08 08:00".Utc());
			Database.WithAdherenceIn("2017-12-08 16:30".Utc());

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
				;
			Database.WithAdherenceIn("2017-12-08 08:00".Utc());

			var result = Target.Load(person);

			result.Percentage().Should().Be(50);
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
			Database.WithAdherenceNeutral("2017-12-08 08:00".Utc());
			Database.WithAdherenceIn("2017-12-08 12:00".Utc());

			var result = Target.Load(person);

			result.Percentage().Should().Be(100);
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
			Database.WithAdherenceOut("2017-12-08 08:00".Utc());
			Database.WithAdherenceIn("2017-12-08 10:00".Utc());
			Database.WithAdherenceNeutral("2017-12-08 12:00".Utc());

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
			Database.WithAdherenceNeutral("2017-12-08 07:30".Utc());
			Database.WithAdherenceNeutral("2017-12-08 08:30".Utc());
			Database.WithAdherenceIn("2017-12-08 14:00".Utc());

			var result = Target.Load(person);

			result.Percentage().Should().Be(100);
		}
	}

	public static class AgentAdherenceDayLoaderEx
	{
		public static Domain.ApplicationLayer.Rta.AgentAdherenceDay.AgentAdherenceDay Load(this AgentAdherenceDayLoader loader, Guid personId)
		{
			return loader.Load(personId, new DateOnly(ServiceLocatorForEntity.Now.UtcDateTime().Date));
		}
	}
}