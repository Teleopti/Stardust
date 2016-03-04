using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_ScaleOut_36979)]
	public class ScaleOutTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public MutableNow Now;
		public FakeEventPublisher EventPublisher;
		
		[Test]
		public void ShouldUpdateNextSchedule()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2016-03-04 8:00", "2016-03-04 10:15")
				;
			Now.Is("2016-03-04 9:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			Target.CheckForActivityChanges(Database.TenantName());

			Database.ClearSchedule(personId);
			Database.WithSchedule(personId, phone, "phone", "2016-03-04 8:00", "2016-03-04 10:00");
			Database.WithSchedule(personId, brejk, "break", "2016-03-04 10:00", "2016-03-04 10:15");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.PersistedReadModel.ScheduledNext.Should().Be("break");
		}
		
	}
}