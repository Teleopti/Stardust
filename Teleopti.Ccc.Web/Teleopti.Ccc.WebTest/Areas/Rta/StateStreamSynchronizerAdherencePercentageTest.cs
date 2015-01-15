using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	[TestFixture]
	public class StateStreamSynchronizerAdherencePercentageTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public IRta Rta;
		public FakeAdherencePercentageReadModelPersister Persister;
		public MutableNow Now;

		[Test]
		public void ShouldInitializeAdherencePercentage()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Now.Is("2015-01-08 11:00");
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithSchedule(personId, phone, "2015-01-08 11:00", "2015-01-08 13:00")
				.WithAlarm("phone", phone, 0)
				.WithUser("user", personId)
				;
			Rta.SaveState(new ExternalUserStateForTest
			{
				UserCode = "user",
				StateCode = "phone"
			});
			Now.Is("2015-01-08 12:00");

			Target.Initialize();

			Persister.Get(new DateOnly("2015-01-08".Utc()), personId).Should().Not.Be.Null();
		}
	}
}