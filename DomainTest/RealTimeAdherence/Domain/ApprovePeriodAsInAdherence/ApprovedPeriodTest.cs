﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class ApprovedPeriodTest
	{
		public FakeDatabase Database;
		public IAgentAdherenceDayLoader Target;

		[Test]
		public void ShouldApprove()
		{
			var person = Guid.NewGuid();
			Database
				.WithPerson(person)
				.WithApprovedPeriod("2018-02-07 08:00:00", "2018-02-07 08:00:00");

			var result = Target.Load(person, "2018-02-07".Date());

			result.ApprovedPeriods().Single().StartTime.Should().Be("2018-02-07 08:00:00".Utc());
		}
	}
}