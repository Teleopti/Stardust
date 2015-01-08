using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NoBroker_31237)]
	[TestFixture]
	public class StateStreamSynchronizerAdherenceDetailsTest
	{
		public FakeRtaDatabase Database;
		public IStateStreamSynchronizer Target;
		public FakeAdherenceDetailsReadModelPersister Model;
		public MutableNow Now;

		[Test]
		public void ShouldInitializeAdherenceDetails()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Now.Is("2015-01-08 12:00");
			Database
				.WithExistingState(personId, activityId)
				.WithSchedule(personId, activityId, "2015-01-08 11:00", "2015-01-08 13:00")
				.WithUser("", personId)
				;

			Target.Initialize();

			Model.Get(personId, new DateOnly(Now.UtcDateTime())).Model.Details.Single().StartTime
				.Should().Be("2015-01-08 11:00".Utc());
		}

	}
}