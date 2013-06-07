using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
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
    public class OvertimeAvailabilityRepositoryTest : RepositoryTest<IOvertimeAvailability>
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

            var foundItem = new OvertimeAvailabilityRepository(UnitOfWork).LoadAggregate(item.Id.GetValueOrDefault());
            Assert.AreEqual(item, foundItem);
        }

        [Test]
        public void CanFindStudentDaysBetweenDatesAndOnPersons()
        {
            var period = new DateOnlyPeriod(2009, 2, 1, 2009, 3, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(CreateOvertimeAvailability(new DateOnly(2009, 2, 3), _person, true));
            PersistAndRemoveFromUnitOfWork(CreateOvertimeAvailability(new DateOnly(2009, 3, 2), _person, false));
            IEnumerable<IPerson> persons = new Collection<IPerson> { _person };
            IList<IOvertimeAvailability> days = new OvertimeAvailabilityRepository(UnitOfWork).Find(period, persons);
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
            PersistAndRemoveFromUnitOfWork(CreateOvertimeAvailability(new DateOnly(2009, 2, 3), _person, true));
            PersistAndRemoveFromUnitOfWork(CreateOvertimeAvailability(new DateOnly(2009, 3, 2), _person, false));
            IEnumerable<IPerson> persons = new Collection<IPerson>();
            IList<IOvertimeAvailability> days = new OvertimeAvailabilityRepository(UnitOfWork).Find(period, persons);
            Assert.AreEqual(0, days.Count);
        }

        [Test]
        public void CanFindStudentDaysOnDayAndPerson()
        {
            DateOnly date = new DateOnly(2009, 2, 1);
            IPerson person2 = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(person2);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(CreateOvertimeAvailability(date, _person, true));
            PersistAndRemoveFromUnitOfWork(CreateOvertimeAvailability(date, person2, false));
            PersistAndRemoveFromUnitOfWork(CreateOvertimeAvailability(date.AddDays(1), _person, false));

            IList<IOvertimeAvailability > days = new OvertimeAvailabilityRepository(UnitOfWork).Find(date, _person);
            Assert.AreEqual(1, days.Count);

            Assert.IsTrue(days[0].NotAvailable);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldCreateRepositoryWithUnitOfWorkFactory()
        {
            Assert.IsNotNull(new OvertimeAvailabilityRepository(UnitOfWorkFactory.Current));
        }

        protected override IOvertimeAvailability CreateAggregateWithCorrectBusinessUnit()
        {
            IOvertimeAvailability availDay = CreateOvertimeAvailability(_dateOnly, _person, false);

            return availDay;
        }

        private static IOvertimeAvailability  CreateOvertimeAvailability(DateOnly date, IPerson person, bool notAvailable)
        {
            //var studentAvailabilityRestriction = new StudentAvailabilityRestriction();

            //var studentAvailabilityRestrictions = new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction };
            var studentAvailabilityDay = new OvertimeAvailability(person, date,TimeSpan.FromHours(8),TimeSpan.FromHours(10));
            studentAvailabilityDay.NotAvailable = notAvailable;
            return studentAvailabilityDay;
        }

        protected override void VerifyAggregateGraphProperties(IOvertimeAvailability  loadedAggregateFromDatabase)
        {
            IOvertimeAvailability org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
            //Assert.AreEqual(org.RestrictionDate, loadedAggregateFromDatabase.RestrictionDate);
            //Assert.AreEqual(1, loadedAggregateFromDatabase.RestrictionCollection.Count);
            Assert.IsFalse(loadedAggregateFromDatabase.NotAvailable);
        }

        protected override Repository<IOvertimeAvailability> TestRepository(IUnitOfWork unitOfWork)
        {
            return new OvertimeAvailabilityRepository(unitOfWork);
        }

       
    }

   
}
