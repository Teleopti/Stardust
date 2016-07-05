using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonAssociationChanged
{
	[TestFixture]
	public class PersonAssociationChangedEventVersion2
	{
		// No Previous team + Current Team => Agent started
		// Previous team + No Current Team => Agent terminated
		[Test]
		public void ToOptimizeReadmodelUpdatesWeNeedInformationAboutPreviousTeamsAndSites()
		{
			var @event = new PersonAssociationChangedEvent();

			@event.Version.Should().Be(2);
		}
	}
}