using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.AgentAdherenceDay
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class LateForWorkTest
	{
		public IAgentAdherenceDayLoader Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetLateForWork()
		{
			Now.Is("2018-05-28 17:00");
			var personId = Guid.NewGuid();
			History
				.ArrivedLateForWork(personId, "2018-05-28 09:00", "2018-05-28 10:00")
				;

			var data = Target.Load(personId, "2018-05-28".Date());

			data.Changes().Single().LateForWork.Should().Be(string.Format(UserTexts.Resources.LateXMinutes, "60"));
		}

		[Test]
		public void ShouldGetLateForWorkOnStateChanged()
		{
			Now.Is("2018-05-28 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-05-28 10:00")
				.ArrivedLateForWork(personId, "2018-05-28 09:00", "2018-05-28 10:00")
				;

			var data = Target.Load(personId, "2018-05-28".Date());

			data.Changes().Single().LateForWork.Should().Be(string.Format(UserTexts.Resources.LateXMinutes, "60"));
		}

		[Test]
		public void ShouldHaveLateForWorkOnSecondStateChange()
		{
			Now.Is("2018-05-28 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-05-28 09:55")
				.StateChanged(personId, "2018-05-28 10:00")
				.ArrivedLateForWork(personId, "2018-05-28 09:00", "2018-05-28 10:00")
				;

			var data = Target.Load(personId, "2018-05-28".Date());

			data.Changes().First().LateForWork.Should().Be.Null();
			data.Changes().Second().LateForWork.Should().Be(string.Format(UserTexts.Resources.LateXMinutes, "60"));
		}

		[Test]
		public void ShouldHaveSolidProofProperties()
		{
			Now.Is("2018-05-28 17:00");
			var personId = Guid.NewGuid();
			History
				.ArrivedLateForWork(personId, "2018-05-28 09:00", "2018-05-28 10:00", "InCall", "Phone", Color.Crimson, "InAdherence", Color.DarkKhaki, Adherence.In)
				;

			var data = Target.Load(personId, "2018-05-28".Date());

			data.Changes().Single().Timestamp.Should().Be("2018-05-28 10:00".Utc());
			data.Changes().Single().ActivityName.Should().Be("Phone");
			data.Changes().Single().ActivityColor.Should().Be(Color.Crimson.ToArgb());
			data.Changes().Single().StateName.Should().Be("InCall");
			data.Changes().Single().RuleName.Should().Be("InAdherence");
			data.Changes().Single().RuleColor.Should().Be(Color.DarkKhaki.ToArgb());
			data.Changes().Single().Adherence.Should().Be(HistoricalChangeAdherence.In);
		}
		
		
		[Test]
		public void ShouldNotHaveLateForWorkWithSecondResolution()
		{
			Now.Is("2018-06-14 17:00");
			var personId = Guid.NewGuid();
			History
				.ArrivedLateForWork(personId, "2018-06-14 09:00", "2018-06-14 10:00:01");

			var data = Target.Load(personId, "2018-06-14".Date());

			data.Changes().Single().LateForWork.Should().Be(string.Format(UserTexts.Resources.LateXMinutes, "60"));
		}
	}
}