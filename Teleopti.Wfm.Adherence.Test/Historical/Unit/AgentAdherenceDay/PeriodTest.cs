﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class PeriodTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldHavePeriod1HourExtra()
		{
			Now.Is("2018-11-07 08:00");
			var person = Guid.NewGuid();
			History
				.ShiftStart(person, "2018-11-06 09:00", "2018-11-06 17:00")
				;

			var actual = Target.Load(person, "2018-11-06".Date()).Period();
			actual.StartDateTime.Should().Be("2018-11-06 08:00".Utc());
			actual.EndDateTime.Should().Be("2018-11-06 18:00".Utc());
		}

		[Test]
		public void ShouldHavePeriodFromLatestShiftStart()
		{
			Now.Is("2018-11-07 08:00");
			var person = Guid.NewGuid();
			History
				.ShiftStart(person, "2018-11-06 09:00", "2018-11-06 17:00")
				.ShiftStart(person, "2018-11-06 10:00", "2018-11-06 18:00")
				;

			var actual = Target.Load(person, "2018-11-06".Date()).Period();
			actual.StartDateTime.Should().Be("2018-11-06 09:00".Utc());
			actual.EndDateTime.Should().Be("2018-11-06 19:00".Utc());
		}
		
		[Test]
		public void ShouldHavePeriodFromShiftEnd()
		{
			Now.Is("2018-11-07 08:00");
			var person = Guid.NewGuid();
			History
				.ShiftStart(person, "2018-11-06 09:00", "2018-11-06 17:00")
				.ShiftEnd(person, "2018-11-06 10:00", "2018-11-06 18:00")
				;

			var actual = Target.Load(person, "2018-11-06".Date()).Period();
			actual.StartDateTime.Should().Be("2018-11-06 09:00".Utc());
			actual.EndDateTime.Should().Be("2018-11-06 19:00".Utc());
		}
	}
}