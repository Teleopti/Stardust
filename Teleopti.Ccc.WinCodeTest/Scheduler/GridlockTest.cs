using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.Domain.Common;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class GridlockTest
    {
        Gridlock _gridlock;
        Person _person;
        DateOnly _date;

        [SetUp]
        public void Setup()
        {
            _person = new Person();
            _date = new DateOnly(2006, 1, 1);
            DateTimePeriod period = new DateTimePeriod(2006,1,1,2006,1,2);
            _gridlock = new Gridlock(_person, _date, LockType.Authorization,period);
        }

        [Test]
        public void CanCreateGridlock()
        {
            Assert.IsNotNull(_gridlock);
        }

        [Test]
        public void CheckProperties()
        {
            Assert.AreEqual(_person, _gridlock.Person);
            Assert.AreEqual(_date, _gridlock.LocalDate);
            Assert.AreEqual(LockType.Authorization, _gridlock.LockType);
            Assert.AreEqual(_person.GetHashCode().ToString(CultureInfo.InvariantCulture) + "|" +
								_date.GetHashCode().ToString(CultureInfo.InvariantCulture) + "|" + LockType.Authorization, _gridlock.Key);
			Assert.AreEqual(new DateTimePeriod(2006, 1, 1, 2006, 1, 2), _gridlock.Period);
        }
    }
}
