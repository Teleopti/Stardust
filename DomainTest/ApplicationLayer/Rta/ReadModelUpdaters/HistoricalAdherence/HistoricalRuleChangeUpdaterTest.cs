using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.HistoricalAdherence
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	[Toggle(Toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)]
	public class HistoricalRuleChangeUpdaterTest
	{
		public HistoricalAdherenceWithProofUpdater Target;
		public FakeHistoricalChangeReadModelPersister Persister;
		
		[Test]
		public void ShouldSubscribe()
		{
			var subscriptionsRegistrator = new SubscriptionRegistrator();

			Target.Subscribe(subscriptionsRegistrator);

			subscriptionsRegistrator.SubscribesTo(typeof(PersonRuleChangedEvent)).Should().Be(true);
		}

		[Test]
		public void ShouldAddRuleChange()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonRuleChangedEvent
				{
					PersonId = personId,
					Timestamp = "2017-03-07 10:00".Utc(),
					StateName = "phone"
				}
			});

			var change = Persister.Read(personId, "2017-03-07".Date()).Single();
			change.PersonId.Should().Be(personId);
			change.StateName.Should().Be("phone");
		}

		[Test]
		public void ShouldAddRuleChange2()
		{
			var personId = Guid.NewGuid();
			var state = Guid.NewGuid();

			Target.Handle(new[]
			{
				new PersonRuleChangedEvent
				{
					PersonId = personId,
					BelongsToDate = "2017-03-07".Date(),
					Timestamp = "2017-03-07 10:00".Utc(),
					StateName = "ready",
					StateGroupId = state,
					ActivityName = "phone",
					ActivityColor = Color.DarkGoldenrod.ToArgb(),
					RuleName = "in",
					RuleColor = Color.Azure.ToArgb(),
					Adherence = EventAdherence.In
				}
			});

			var change = Persister.Read(personId, "2017-03-07".Date()).Single();
			change.PersonId.Should().Be(personId);
			change.BelongsToDate.Should().Be("2017-03-07".Date());
			change.Timestamp.Should().Be("2017-03-07 10:00".Utc());
			change.StateName.Should().Be("ready");
			change.StateGroupId.Should().Be(state);
			change.ActivityName.Should().Be("phone");
			change.ActivityColor.Should().Be(Color.DarkGoldenrod.ToArgb());
			change.RuleName.Should().Be("in");
			change.RuleColor.Should().Be(Color.Azure.ToArgb());
			change.Adherence.Should().Be(HistoricalChangeInternalAdherence.In);
		}
	}
}