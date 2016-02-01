using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.RTA_TerminatedPersons_36042)]
	[Toggle(Toggles.RTA_TeamChanges_36043)]
	public class PeriodChangesTest
	{
		public PersonAssociationChangedEventPublisher Target;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public FakeDatabase Data;

		[Test, Ignore]
		public void ShouldPublishWhenPeriodChanges()
		{
			Now.Is("2016-02-01 00:00");
			Data.HasPerson("pierre")
				.WithPeriod("2016-02-01");

			Target.Handle(new TenantHearbeatEvent());

			Publisher.PublishedEvents.Single().Should().Be.OfType<PersonAssociationChangedEvent>();
		}
	}
}