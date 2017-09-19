using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	public class ShiftTradeValidationFailedRuleTest
	{
		[Test]
		public void ShouldCreateResponseWithCorrectProperty()
		{
			const string errorMessage = "Test error";
			var period = DateOnly.Today.AddDays(2).ToDateOnlyPeriod();
			var target = new ShiftTradeValidationFailedRule(errorMessage, period);

			var person = PersonFactory.CreatePersonWithId();
			var personSchedules = new Dictionary<IPerson, IScheduleRange> {{person, null}};
			var response = target.Validate(personSchedules, null).Single();

			Assert.AreEqual(typeof(ShiftTradeValidationFailedRule), response.TypeOfRule);
			Assert.AreEqual(true, response.Error);
			Assert.AreEqual(period, response.DateOnlyPeriod);
			Assert.AreEqual(string.Empty, response.FriendlyName);
			Assert.AreEqual(true, response.Mandatory);
			Assert.AreEqual(errorMessage, response.Message);
			Assert.AreEqual(false, response.Overridden);
			Assert.AreEqual(person, response.Person);
		}
	}
}