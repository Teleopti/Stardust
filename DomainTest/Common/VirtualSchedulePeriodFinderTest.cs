using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class VirtualSchedulePeriodFinderTest
	{
	    
	    [Test]
		public void ShouldFindVirtualSchedulePeriods()
		{
			var date = new DateOnly(2000, 1, 1);
			var personContract = PersonContractFactory.CreatePersonContract("Contract", "Testing", "Test");
			var team = TeamFactory.CreateSimpleTeam();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(date, personContract, team);
	        var _personAccountUpdater = new MockRepository().StrictMock<IPersonAccountUpdater>();

			var schedulePeriod1 = new SchedulePeriod(new DateOnly(2011, 06, 06), SchedulePeriodType.Week, 4);
			var schedulePeriod2 = new SchedulePeriod(new DateOnly(2011, 06, 13), SchedulePeriodType.Week, 2);
			var schedulePeriod3 = new SchedulePeriod(new DateOnly(2011, 06, 29), SchedulePeriodType.Day, 14);

			var period1 = new DateOnlyPeriod(new DateOnly(2011, 06, 10), new DateOnly(2011, 06, 12));
			var period2 = new DateOnlyPeriod(new DateOnly(2011, 06, 10), new DateOnly(2011, 06, 15));
			var period3 = new DateOnlyPeriod(new DateOnly(2011, 06, 01), new DateOnly(2011, 07, 26));
			var period4 = new DateOnlyPeriod(new DateOnly(2011, 07, 29), new DateOnly(2011, 07, 29));

			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			person.AddSchedulePeriod(schedulePeriod1);
			person.AddSchedulePeriod(schedulePeriod2);
			person.AddSchedulePeriod(schedulePeriod3);

			var checker = new VirtualSchedulePeriodSplitChecker(person);

			var periodFinder = new VirtualSchedulePeriodFinder(person);

			var virtualSchedulePeriods1 = periodFinder.FindVirtualPeriods(period1);
			var virtualSchedulePeriods2 = periodFinder.FindVirtualPeriods(period2);
			var virtualSchedulePeriods3 = periodFinder.FindVirtualPeriods(period3);
			var virtualSchedulePeriods4 = periodFinder.FindVirtualPeriods(period4);

			Assert.AreEqual(1, virtualSchedulePeriods1.Count);
			Assert.AreEqual(2, virtualSchedulePeriods2.Count);
			Assert.AreEqual(5, virtualSchedulePeriods3.Count);
			Assert.AreEqual(1, virtualSchedulePeriods4.Count);

			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 06, 09), checker), virtualSchedulePeriods1[0]);
			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 06, 09), checker), virtualSchedulePeriods2[0]);
			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 06, 09), checker), virtualSchedulePeriods3[0]);

			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 06, 17), checker), virtualSchedulePeriods2[1]);
			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 06, 17), checker), virtualSchedulePeriods3[1]);

			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 06, 27), checker), virtualSchedulePeriods3[2]);

			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 07, 03), checker), virtualSchedulePeriods3[3]);

			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 07, 22), checker), virtualSchedulePeriods3[4]);

			Assert.AreEqual(new VirtualSchedulePeriod(person, new DateOnly(2011, 07, 27), checker), virtualSchedulePeriods4[0]);

		    person.TerminatePerson(new DateOnly(2011, 06, 10),_personAccountUpdater);
            var virtualSchedulePeriods = periodFinder.FindVirtualPeriods(new DateOnlyPeriod(new DateOnly(2011, 06, 10), new DateOnly(2011, 07, 29)));
            Assert.AreEqual(1, virtualSchedulePeriods.Count);
		}
	}
}
