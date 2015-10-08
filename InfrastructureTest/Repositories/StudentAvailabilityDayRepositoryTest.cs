using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Foundation;
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


		[Test]
		public void CanChangeStudentAvailabilityDays()
		{
			// create start point
			var dateOnly = new DateOnly(2009, 2, 3);

			var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			studentAvailabilityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(8), null);
			studentAvailabilityRestriction.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.FromHours(20));
            var studentAvailabilityRestrictions = new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction };
            var studentAvailabilityDay = new StudentAvailabilityDay(_person, dateOnly, studentAvailabilityRestrictions);
			PersistAndRemoveFromUnitOfWork(studentAvailabilityDay);

			
			// load again
			IList<IStudentAvailabilityDay> days = new StudentAvailabilityDayRepository(UnitOfWork).Find(dateOnly, _person);

			var loadedStudentAvailabilityDay = days[0];

			loadedStudentAvailabilityDay.Change(new TimePeriod(TimeSpan.FromHours(9), TimeSpan.FromHours(20)));
			PersistAndRemoveFromUnitOfWork(loadedStudentAvailabilityDay);


			IList<IStudentAvailabilityDay> daysAgain = new StudentAvailabilityDayRepository(UnitOfWork).Find(dateOnly, _person);
			var loadedStudentAvailabilityDayAgain = daysAgain[0];

			var restrictionAgain = loadedStudentAvailabilityDayAgain.RestrictionCollection[0];
			var index = ((StudentAvailabilityDay) restrictionAgain.Parent).IndexInCollection(restrictionAgain);
			Assert.AreEqual(0, index);
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
		public void CanFindStudentDaysOnDayAndPersonOrderByUpdatedOn()
		{
			DateOnly date = new DateOnly(2009, 2, 1);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			var firstStudentAvailability = CreateStudentAvailabilityDay(date, _person, true);
			var lastStudentAvailability = CreateStudentAvailabilityDay(date, _person, true);
			PersistAndRemoveFromUnitOfWork(firstStudentAvailability);
			Thread.Sleep(1000);
			PersistAndRemoveFromUnitOfWork(lastStudentAvailability);

			IList<IStudentAvailabilityDay> days = new StudentAvailabilityDayRepository(UnitOfWork).Find(date, _person);
			Assert.AreEqual(2, days.Count);

			Assert.That(days.FirstOrDefault(), Is.EqualTo(lastStudentAvailability));
		}

        protected override IStudentAvailabilityDay CreateAggregateWithCorrectBusinessUnit()
        {
            IStudentAvailabilityDay availDay = CreateStudentAvailabilityDay(_dateOnly, _person, false);

            return availDay;
        }

        private static IStudentAvailabilityDay CreateStudentAvailabilityDay(DateOnly date, IPerson person, bool notAvailable)
        {
            var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
            
            var studentAvailabilityRestrictions = new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction };
            var studentAvailabilityDay = new StudentAvailabilityDay(person, date, studentAvailabilityRestrictions);
            studentAvailabilityDay.NotAvailable = notAvailable;
            return studentAvailabilityDay;
        }

        protected override void VerifyAggregateGraphProperties(IStudentAvailabilityDay loadedAggregateFromDatabase)
        {
            IStudentAvailabilityDay org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
            Assert.AreEqual(org.RestrictionDate, loadedAggregateFromDatabase.RestrictionDate);
            Assert.AreEqual(1, loadedAggregateFromDatabase.RestrictionCollection.Count);
            Assert.IsFalse(loadedAggregateFromDatabase.NotAvailable);
        }

        protected override Repository<IStudentAvailabilityDay> TestRepository(IUnitOfWork unitOfWork)
        {
            return new StudentAvailabilityDayRepository(unitOfWork);
        }

		[Test]
		public void CanFindDaysNewerThan()
		{
			var newerThan = DateTime.UtcNow.AddHours(-1);
			PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
			PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(new DateOnly(2013, 2, 2), _person, false));
			PersistAndRemoveFromUnitOfWork(CreateStudentAvailabilityDay(new DateOnly(2013, 3, 2), _person, false));

			var days = new StudentAvailabilityDayRepository(UnitOfWork).FindNewerThan(newerThan);
			Assert.AreEqual(3, days.Count);
			LazyLoadingManager.IsInitialized(days[0].RestrictionCollection.First().StartTimeLimitation).Should().Be.True();
		}
    }
}
