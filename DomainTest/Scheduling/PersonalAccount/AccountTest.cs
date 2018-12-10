using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.PersonalAccount
{

    [TestFixture]
    public class AccountTest
    {
        private PersonAbsenceAccount _parent;
      
        [SetUp]
        public void Setup()
        {
            _parent = new PersonAbsenceAccount(PersonFactory.CreatePerson(), new Absence());
        }


        [Test]
        public void VerifyLastCalculatedBalance()
        {
            var firstDate = new DateOnly(2000, 1, 1);
            AccountDay d1 = new AccountDay(firstDate);
            _parent.Add(d1);
            d1.LatestCalculatedBalance = TimeSpan.FromDays(3); // it is the "Used"
            d1.Accrued = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(2), d1.Remaining);

            d1.Track(TimeSpan.FromDays(1));
            Assert.AreEqual(TimeSpan.FromDays(2), d1.Remaining);

            d1.Track(TimeSpan.FromDays(3));
            Assert.AreEqual(TimeSpan.FromDays(0), d1.Remaining);

            d1.Track(TimeSpan.FromDays(0));
            Assert.AreEqual(TimeSpan.FromDays(3), d1.Remaining);
        }

        [Test] 
        public void VerifyIsExceeded()
        {
            var firstDate = new DateOnly(2000, 1, 1);
            AccountDay d1 = new AccountDay(firstDate);
            _parent.Add(d1);
            d1.Accrued = TimeSpan.FromDays(5);
            d1.LatestCalculatedBalance = TimeSpan.FromDays(1);
            d1.Track(TimeSpan.Zero);

            d1.Track(TimeSpan.FromDays(2));

            Assert.IsFalse(d1.IsExceeded);
            d1.Track(TimeSpan.FromDays(5));
            Assert.IsTrue(d1.IsExceeded);
        }

        [Test]
        public void VerifyPeriod()
        {
            var firstDate = new DateOnly(2000, 1, 1);
            var secondDate = new DateOnly(2000, 4, 1);
            var lastDayOfFirstPeriod = secondDate.AddDays(-1);
            var firstPeriod = new DateOnlyPeriod(firstDate, lastDayOfFirstPeriod);
            var secondPeriod = new DateOnlyPeriod(secondDate, secondDate.AddDays(3600));

            _parent.Add(new AccountDay(firstDate));
            _parent.Add(new AccountDay(secondDate));

            Assert.AreEqual(secondPeriod, _parent.AccountCollection().First().Period());
            Assert.AreEqual(firstPeriod, _parent.AccountCollection().Last().Period(), "Check that the periods are not intersected");
        }

        [Test]
        public void VerifyPeriodOutsideCollection()
        {
            var acc = new AccountDay(new DateOnly(2001, 1, 1));
            var period = new DateOnlyPeriod(new DateOnly(2001, 1, 1), new DateOnly(2001, 1, 1).AddDays(3600));
            Assert.AreEqual(period, acc.Period());
        }

		  [Test]
		  public void ShouldHaveTerminalDateAsEndDateIfInPeriod()
		  {
			  var person = MockRepository.GenerateMock<IPerson>();
			  var owner = new PersonAbsenceAccount(person, new Absence());
			  var acc = new AccountTime(new DateOnly(2013, 12, 1));
			  owner.Add(acc);

			  person.Stub(p => p.TerminalDate).Return(new DateOnly(2013, 12, 18));

			  Assert.That(acc.Period().EndDate, Is.EqualTo(new DateOnly(2013, 12, 18)));
		  }
    }

}