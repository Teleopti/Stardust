using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class CheckForActivityChangeTest
	{
		public FakeRtaDatabase Database;
		public FakeMessageSender Sender;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldKeepPreviousStateCodeWhenNotifiedOfActivityChange()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId);
			Now.Is("2014-10-20 10:00");
			
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			Sender.NotificationsOfType<AgentStateReadModel>().Last().DeseralizeActualAgentState()
				.StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldKeepPreviousStateWhenNotifiedOfActivityChange()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, "alarm")
				;
			
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			Sender.NotificationsOfType<AgentStateReadModel>().Last().DeseralizeActualAgentState()
				.State.Should().Be("alarm");
		}

		[Test]
		public void ShouldHandleUnrecognizedPersonId()
		{
			var businessUnitId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId);

			Assert.DoesNotThrow(() => Target.ReloadAndCheckForActivityChanges(Database.TenantName(), Guid.NewGuid()));
		}
	}
}