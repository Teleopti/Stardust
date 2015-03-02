using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class NeutralAdherenceTest
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		[Test]
		public void ShouldPersistTimeOutOfAdhernceWhenEnteringNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-02 08:00".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-02 08:05".Utc(),
				InAdherence = null
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2015-03-02 08:10".Utc(),
				InAdherence = false
			});

			Persister.Details.Single().TimeOutOfAdherence.Should().Be("5".Minutes());
		}

		[Test]
		public void ShouldPersistLastAdherenceWhenEnteringNeutralAdherence()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2015-03-02 08:00".Utc(),
				InAdherence = null
			});

			Persister.Model.LastAdherence.Should().Be(null);
		}
	}
}