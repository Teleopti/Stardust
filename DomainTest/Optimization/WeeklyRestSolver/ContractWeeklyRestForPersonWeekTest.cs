using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class ContractWeeklyRestForPersonWeekTest
    {
	    [Test]
	    public void ReturnNothingIfNoPersonPeriodFound()
	    {
		    var person = PersonFactory.CreatePerson();
		    var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 23);
		    var personWeek = new PersonWeek(person, dateOnlyPeriod);

			var target = new ContractWeeklyRestForPersonWeek();
		    var weeklyRest = target.GetWeeklyRestFromContract(personWeek);
		    Assert.AreEqual(TimeSpan.Zero, weeklyRest);
	    }

	    [Test]
        public void ReturnWeeklyRestOnStartDate()
        {
	        var contract = new Contract("asad")
	        {
		        WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(36))
	        };

	        var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 23);
	        var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(),
		        dateOnlyPeriod.StartDate, contract, new PartTimePercentage("test"));
            var personWeek = new PersonWeek(person, dateOnlyPeriod);

			var target = new ContractWeeklyRestForPersonWeek();
	        var weeklyRest = target.GetWeeklyRestFromContract(personWeek);
	        Assert.AreEqual(TimeSpan.FromHours(36), weeklyRest);
        }

        [Test]
        public void ReturnWeeklyRestOnEndDate()
		{
			var contract = new Contract("asad")
			{
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(18))
			};

			var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 23);
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson(),
				dateOnlyPeriod.EndDate, contract, new PartTimePercentage("test"));
			var personWeek = new PersonWeek(person, dateOnlyPeriod);

			var target = new ContractWeeklyRestForPersonWeek();
			var weeklyRest = target.GetWeeklyRestFromContract(personWeek);
			Assert.AreEqual(TimeSpan.FromHours(18), weeklyRest);
		}
    }
}
