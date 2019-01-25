using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	[DomainTest]
	public class PublishPersonEventsTest
	{
		[Test]
		public void ShouldPublishMultipleEvents()
		{
			var person = new Person();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod("2018-04-17".Date());

			person.SetName(new Name("roger", "kjatz"));
			person.SetEmploymentNumber("123");
			person.AddPersonPeriod(personPeriod);

			var result = person.PopAllEvents(null);
			result.OfType<PersonNameChangedEvent>().Should().Not.Be.Empty();
			result.OfType<PersonEmploymentNumberChangedEvent>().Should().Not.Be.Empty();
			result.OfType<PersonPeriodChangedEvent>().Should().Not.Be.Empty();
		}
	}
}