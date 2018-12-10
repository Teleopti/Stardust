using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class PlanningTimeBankExtractorTest
    {
        [Test]
        public void ShouldSetFalseOnEditableWhenVirtualPeriodIsNotValid()
        {
			var person = PersonFactory.CreatePerson();
			var target = new PlanningTimeBankExtractor(person);
			var dateOnly = new DateOnly(2011, 2, 16);

			var result = target.GetPlanningTimeBank(dateOnly);
            Assert.That(result.IsEditable,Is.False);
        }
        [Test]
        public void ShouldSetEditableWhenFirstSchedulePeriod()
		{
			var dateOnly = new DateOnly(2011, 2, 16);
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(),dateOnly);
			var target = new PlanningTimeBankExtractor(person);
			
            var result =target.GetPlanningTimeBank(dateOnly);
            Assert.That(result.IsEditable,Is.True);
        }
    }
}