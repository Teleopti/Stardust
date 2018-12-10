using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon;

using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.PersonalAccount
{
	[TestFixture]
	public class PersonAbsenceAccountTest
	{
		private IPersonAbsenceAccount _target;
		private IAbsence _absence;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_absence = new Absence();
			_person = new Person();
			_target = new PersonAbsenceAccount(_person, _absence);
		}

		[Test]
		public void CanAdd()
		{
			var day = new AccountDay(new DateOnly(2000, 1, 1));
			var time = new AccountTime(new DateOnly(2000, 1, 2));
			_target.Add(day);
			_target.Add(time);
			CollectionAssert.Contains(_target.AccountCollection(), day);
			CollectionAssert.Contains(_target.AccountCollection(), time);
			Assert.AreSame(_target, day.Parent);
			Assert.AreSame(_target, time.Parent);
		}

		[Test]
		public void CanAddMultipleWithSameDate()
		{
			var day = new AccountDay(new DateOnly(2000, 1, 1));
			var time = new AccountTime(new DateOnly(2000, 1, 1));
			_target.Add(day);
			_target.Add(time);
			Assert.AreEqual(2, _target.AccountCollection().Count());
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Domain.Scheduling.PersonalAccount.PersonAbsenceAccount"), Test]
		public void VerifyNoAbsenceThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new PersonAbsenceAccount(_person, null));
		}

		[Test]
		public void CanRemove()
		{
			var account = new AccountDay(new DateOnly(2000, 1, 1));
			_target.Add(account);
			_target.Remove(account);
			CollectionAssert.DoesNotContain(_target.AccountCollection(), account);
		}

		[Test]
		public void VerifyOrder()
		{
			_target.Add(new AccountDay(new DateOnly(2000, 1, 3)));
			_target.Add(new AccountDay(new DateOnly(2000, 1, 1)));
			_target.Add(new AccountDay(new DateOnly(2000, 1, 4)));
			_target.Add(new AccountDay(new DateOnly(2000, 1, 2)));
			_target.Add(new AccountDay(new DateOnly(2000, 1, 5)));

			var list = new List<IAccount>(_target.AccountCollection());

			Assert.AreEqual(new DateOnly(2000, 1, 5), list[0].StartDate);
			Assert.AreEqual(new DateOnly(2000, 1, 4), list[1].StartDate);
			Assert.AreEqual(new DateOnly(2000, 1, 3), list[2].StartDate);
			Assert.AreEqual(new DateOnly(2000, 1, 2), list[3].StartDate);
			Assert.AreEqual(new DateOnly(2000, 1, 1), list[4].StartDate);
		}

		[Test]
		public void CanFindPerPeriod()
		{
			var acc1 = new AccountDay(new DateOnly(2000, 1, 1));
			var acc2 = new AccountDay(new DateOnly(2001, 1, 1));
			var acc3 = new AccountDay(new DateOnly(2002, 1, 1));
			var acc4 = new AccountDay(new DateOnly(2003, 1, 1));
			_target.Add(acc2);
			_target.Add(acc4);
			_target.Add(acc1);
			_target.Add(acc3);

			var res = _target.Find(new DateOnlyPeriod(2000, 4, 4, 2001, 12, 31));
			Assert.AreEqual(2, res.Count());
			CollectionAssert.Contains(res, acc1);
			CollectionAssert.Contains(res, acc2);
		}

		[Test]
		public void CanFindPeriodAfterOnlyOne()
		{
			var acc1 = new AccountDay(new DateOnly(2010, 5, 4));
			var acc2 = new AccountDay(new DateOnly(2010, 4, 1));
			_target.Add(acc1);
			_target.Add(acc2);

			var res = _target.Find(new DateOnlyPeriod(2010, 5, 9, 2010, 5, 12));
			Assert.AreEqual(1, res.Count());
			CollectionAssert.Contains(res, acc1);
		}

		[Test]
		public void CannotFindPeriodBeforeOnlyOne()
		{
			var acc1 = new AccountDay(new DateOnly(2010, 5, 4));
			var acc2 = new AccountDay(new DateOnly(2010, 5, 9));
			_target.Add(acc1);
			_target.Add(acc2);

			var res = _target.Find(new DateOnlyPeriod(2010, 5, 1, 2010, 5, 3));
			Assert.AreEqual(0, res.Count());
		}

		[Test]
		public void CanFindPeriodBeforeOnlyOneAccount()
		{
			var acc1 = new AccountDay(new DateOnly(2010, 5, 10));
			_target.Add(acc1);
			var res = _target.Find(new DateOnlyPeriod(2010, 5, 1, 2010, 5, 2));
			Assert.AreEqual(0, res.Count());
		}

		[Test]
		public void VerifyFind()
		{
			var acc1 = new AccountDay(new DateOnly(2010, 5, 4));
			_target.Add(acc1);
			var res = _target.Find(new DateOnly(2010, 5, 9));
			Assert.AreEqual(acc1, res);
		}

		[Test]
		public void VerifyFindReturnsNullIfNotFound()
		{
			var acc1 = new AccountDay(new DateOnly(2010, 5, 4));
			_target.Add(acc1);
			var res = _target.Find(new DateOnly(2010, 1, 1));
			Assert.IsNull(res);
		}

		[Test]
		public void RestoreShouldKeepAccountId()
		{
			var targetId = Guid.NewGuid();

			var acc1 = new AccountDay(new DateOnly(2010, 5, 4));
			((IEntity)acc1).SetId(Guid.NewGuid());
			_target.Add(acc1);
			_target.SetId(targetId);

			var newTarget = new PersonAbsenceAccount(_person, _absence);
			((IEntity)newTarget).SetId(targetId);

			newTarget.Restore(_target);

			Assert.AreNotSame(_target, newTarget);
			Assert.AreEqual(_target, newTarget);
			Assert.AreEqual(1, _target.AccountCollection().Count());
			Assert.IsNotNull(_target.AccountCollection().First().Id);
		}

        [Test]
        public void ShouldHaveDefaultConstructor()
        {
            ReflectionHelper.HasDefaultConstructor(_target.GetType(),true).Should().Be.True();
        }

        [Test]
        public void ShouldCreateMemento()
        {
            var memento = _target.CreateMemento();
            memento.Should().Not.Be.Null();
        }

        [Test]
        public void ShouldClone()
        {
            _target.SetId(Guid.NewGuid());
            _target.Add(new AccountDay(DateOnly.Today));

            var clone = _target.EntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.AccountCollection().Should().Not.Be.SameInstanceAs(_target.AccountCollection());
            clone.AccountCollection().Count().Should().Be.EqualTo(_target.AccountCollection().Count());
            clone.Id.Should().Be.EqualTo(_target.Id);

            clone = _target.NoneEntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.AccountCollection().Should().Not.Be.SameInstanceAs(_target.AccountCollection());
            clone.AccountCollection().Count().Should().Be.EqualTo(_target.AccountCollection().Count());
            clone.Id.HasValue.Should().Be.False();

            clone = (IPersonAbsenceAccount)_target.Clone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.AccountCollection().Should().Not.Be.SameInstanceAs(_target.AccountCollection());
            clone.AccountCollection().Count().Should().Be.EqualTo(_target.AccountCollection().Count());
            clone.Id.Should().Be.EqualTo(_target.Id);
        }
	}
}
