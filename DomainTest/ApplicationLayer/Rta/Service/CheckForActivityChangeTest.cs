using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class CheckForActivityChangeTest
	{
		public FakeRtaDatabase Database;
		public IAgentStateReadModelReader Reader;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldKeepPreviousStateCodeWhenNotifiedOfActivityChange()
		{
			var personId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId);
			Now.Is("2014-10-20 10:00");
			
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.PersistedReadModel.StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldKeepPreviousStateWhenNotifiedOfActivityChange()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 11:00")
				.WithRule("phone", activityId, "alarm")
				;
			
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Database.PersistedReadModel.StateName.Should().Be("alarm");
		}

		[Test]
		public void ShouldHandleUnrecognizedPersonId()
		{
			Assert.DoesNotThrow(() => Target.CheckForActivityChanges(Database.TenantName(), Guid.NewGuid()));
		}
    }
}