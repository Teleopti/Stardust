using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;


namespace Teleopti.Ccc.DomainTest.Scheduling.PersonalAccount
{
    public class PersonAccountCollectionTest
    {
        private IPersonAccountCollection _target;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _person = new Person();
            _target = new PersonAccountCollection(_person);
        }

        [Test]
        public void CanAdd()
        {
            var foo = new PersonAbsenceAccount(_person, new Absence());
            _target.Add(foo);
            Assert.AreEqual(1, _target.Count());
            CollectionAssert.Contains(_target, foo);
        }

        [Test]
        public void AllPersonAccounts()
        {
            var foo = new PersonAbsenceAccount(_person, new Absence());
            _target.Add(foo);
            var bar = new PersonAbsenceAccount(_person, new Absence());
            _target.Add(bar);

            foo.Add(new AccountDay(new DateOnly(2000, 1, 1)));
            foo.Add(new AccountDay(new DateOnly(2001, 1, 1)));
            bar.Add(new AccountTime(new DateOnly(2001, 1, 1)));

            Assert.AreEqual(3, _target.AllPersonAccounts().Count());
        }
        
        [Test]
        public void CannotAddMultipleAbsences()
        {
            var abs = new Absence();
            _target.Add(new PersonAbsenceAccount(_person, abs));
            Assert.Throws<ArgumentException>(() => _target.Add(new PersonAbsenceAccount(_person, abs)));
        }

        [Test]
        public void CannotAddWrongPerson()
        {
            Assert.Throws<ArgumentException>(() => _target.Add(new PersonAbsenceAccount(new Person(), new Absence())));
        }

        [Test]
        public void CanAddAccount()
        {
            var account = new AccountDay(new DateOnly(2001, 12, 27));
            var absence = new Absence();
            _target.Add(absence, account);
            CollectionAssert.Contains(_target.AllPersonAccounts(), account);
            _target.Add(absence, new AccountDay(new DateOnly(2008,12,2)));
            Assert.AreEqual(2,_target.AllPersonAccounts().Count());
        }
        
        [Test]
        public void CanRemoveAccount()
        {
            var account1 = new AccountDay(new DateOnly(2001, 12, 27));
            var account2 = new AccountDay(new DateOnly(2001, 12, 28));

            var absence = new Absence();
            _target.Add(absence, account1);
            _target.Add(absence, account2);

            Assert.AreEqual(2, _target.AllPersonAccounts().Count());
            _target.Remove(account1);

            CollectionAssert.Contains(_target.AllPersonAccounts(), account2);
            CollectionAssert.DoesNotContain(_target.AllPersonAccounts(), account1);
        }

        [Test]
        public void FindShouldNotReturnNullValues()
        {
            _target.Add(new PersonAbsenceAccount(_person, new Absence()));

            var list = _target.Find(new DateOnly(2000, 5, 1));
            CollectionAssert.DoesNotContain(list,null);
        }
    }
}