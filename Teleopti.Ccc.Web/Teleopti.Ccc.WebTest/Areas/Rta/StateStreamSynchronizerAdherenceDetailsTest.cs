using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
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
		public FakeAdherenceDetailsReadModelPersister Persister;
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

			Persister.Get(personId, new DateOnly("2015-01-08 12:00".Utc())).Model.Details.Single().StartTime
				.Should().Be("2015-01-08 11:00".Utc());
		}

		[Test]
		public void ShouldNotReinitializeAdherenceDetails()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Now.Is("2015-01-08 12:00");
			Database
				.WithExistingState(personId, activityId)
				.WithSchedule(personId, activityId, "2015-01-08 11:00", "2015-01-08 13:00")
				.WithUser("", personId)
				;
			Persister.Add(new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = "2015-01-08".Utc(),
				Model = null
			});

			Target.Initialize();

			Persister.Get(personId, new DateOnly("2015-01-08 12:00".Utc())).Model.Should().Be.Null();
		}

	}
}