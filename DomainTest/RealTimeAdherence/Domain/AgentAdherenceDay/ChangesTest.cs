﻿using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class ChangesTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetRuleChanges()
		{
			Now.Is("2017-03-14 18:00");
			var personId = Guid.NewGuid();
			History
				.RuleChanged(personId, "2017-03-14 18:00", "in")
				;

			var data = Target.Load(personId, "2017-03-14".Date());

			data.Changes().Single().RuleName.Should().Be("in");
		}

		[Test]
		public void ShouldGetRuleChangeProperties()
		{
			Now.Is("2017-03-14 18:00");
			var personId = Guid.NewGuid();
			History
				.RuleChanged(personId, "2017-03-14 18:00", "InCall", "phone", Color.Crimson, null, Color.DarkKhaki, Adherence.In)
				;

			var data = Target.Load(personId, "2017-03-14".Date());

			data.Changes().Single().ActivityName.Should().Be("phone");
			data.Changes().Single().ActivityColor.Should().Be(Color.Crimson.ToArgb());
			data.Changes().Single().StateName.Should().Be("InCall");
			data.Changes().Single().RuleColor.Should().Be(Color.DarkKhaki.ToArgb());
			data.Changes().Single().Adherence.Should().Be(HistoricalChangeAdherence.In);
		}
	}
}