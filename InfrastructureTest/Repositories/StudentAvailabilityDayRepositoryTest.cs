﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class StudentAvailabilityDayRepositoryTest : RepositoryTest<IStudentAvailabilityDay>
    {
        private IPerson _person;
        private DateOnly _dateOnly;

        protected override void ConcreteSetup()
        {
            _person = PersonFactory.CreatePerson();
            _dateOnly = new DateOnly(2009, 2, 2);

            PersistAndRemoveFromUnitOfWork(_person);
        }

        [Test]
        public void ShouldFindStudentDayById()
        {
            var item = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(item);

            var foundItem = new StudentAvailabilityDayRepository(UnitOfWork).LoadAggregate(item.Id.GetValueOrDefault());
            Assert.AreEqual(item, foundItem);
        }

        [Test]
        public void CanFindStudentDaysBetweenDatesAndOnPersons()
        {
            var period = new DateOnlyPeriod(2009, 2, 1, 2009, 3, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(new DateOnly(2009, 2, 3), _person, true));
            PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(new DateOnly(2009, 3, 2), _person, false));
            IEnumerable<IPerson> persons = new Collection<IPerson> { _person };
            IList<IStudentAvailabilityDay> days = new StudentAvailabilityDayRepository(UnitOfWork).Find(period, persons);
            Assert.AreEqual(2, days.Count);
            Assert.IsTrue(days[1].NotAvailable);
            Assert.IsFalse(days[0].NotAvailable);
        }

        /// <summary>
        /// Cants the find student days between dates and on persons when person list is empty.
        /// Bug 9184
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-03-11
        /// </remarks>
        [Test]
        public void CannotFindStudentDaysBetweenDatesAndOnPersonsWhenPersonListIsEmpty()
        {
            var period = new DateOnlyPeriod(2009, 2, 1, 2009, 3, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(new DateOnly(2009, 2, 3), _person, true));
            PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(new DateOnly(2009, 3, 2), _person, false));
            IEnumerable<IPerson> persons = new Collection<IPerson>();
            IList<IStudentAvailabilityDay> days = new StudentAvailabilityDayRepository(UnitOfWork).Find(period, persons);
            Assert.AreEqual(0, days.Count);
        }

        [Test]
        public void CanFindStudentDaysOnDayAndPerson()
        {
            DateOnly date = new DateOnly(2009, 2, 1);
            IPerson person2 = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(person2);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(date, _person, true));
            PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(date, person2, false));
            PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(date.AddDays(1), _person, false));

            IList<IStudentAvailabilityDay> days = new StudentAvailabilityDayRepository(UnitOfWork).Find(date, _person);
            Assert.AreEqual(1, days.Count);

            Assert.IsTrue(days[0].NotAvailable);
        }

		[Test]
		public void CanFindStudentDaysOnDayAndPersonAndLock()
		{
			var date = new DateOnly(2009, 2, 1);
			IPerson person2 = PersonFactory.CreatePerson();
			PersistAndRemoveFromUnitOfWork(person2);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(date, _person, true));
			PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(date, person2, false));
			PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(date.AddDays(1), _person, false));

			IList<IStudentAvailabilityDay> days = new StudentAvailabilityDayRepository(UnitOfWork).FindAndLock(date, _person);
			Assert.AreEqual(1, days.Count);

			Assert.IsTrue(days[0].NotAvailable);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            Assert.IsNotNull(new StudentAvailabilityDayRepository(UnitOfWorkFactory.Current));
        }

        protected override IStudentAvailabilityDay CreateAggregateWithCorrectBusinessUnit()
        {
            IStudentAvailabilityDay availDay = CreateStudentAvailabilityDay(_dateOnly, _person, false);

            return availDay;
        }

        private static IStudentAvailabilityDay CreateStudentAvailabilityDay(DateOnly date, IPerson person, bool notAvailable)
        {
            var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
            var studentAvailabilityRestriction2 = new StudentAvailabilityRestriction();

            var studentAvailabilityRestrictions = new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction, studentAvailabilityRestriction2 };
            var studentAvailabilityDay = new StudentAvailabilityDay(person, date, studentAvailabilityRestrictions);
            studentAvailabilityDay.NotAvailable = notAvailable;
            return studentAvailabilityDay;
        }

        protected override void VerifyAggregateGraphProperties(IStudentAvailabilityDay loadedAggregateFromDatabase)
        {
            IStudentAvailabilityDay org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
            Assert.AreEqual(org.RestrictionDate, loadedAggregateFromDatabase.RestrictionDate);
            Assert.AreEqual(2, loadedAggregateFromDatabase.RestrictionCollection.Count);
            Assert.IsFalse(loadedAggregateFromDatabase.NotAvailable);
        }

        protected override Repository<IStudentAvailabilityDay> TestRepository(IUnitOfWork unitOfWork)
        {
            return new StudentAvailabilityDayRepository(unitOfWork);
        }
    }
}
