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
        public void ShouldFindOvertimeDayById()
        {
            var item = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(item);

            var foundItem = new OvertimeAvailabilityRepository(UnitOfWork).LoadAggregate(item.Id.GetValueOrDefault());
            Assert.AreEqual(item, foundItem);
        }

        [Test]
        public void CanFindOvertimeDayBetweenDatesAndOnPersons()
        {
            var period = new DateOnlyPeriod(2009, 2, 1, 2009, 3, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(new DateOnly(2009, 2, 3), _person, true));
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(new DateOnly(2009, 3, 2), _person, false));
            IEnumerable<IPerson> persons = new Collection<IPerson> { _person };
            IList<IOvertimeAvailability> days = new OvertimeAvailabilityRepository(UnitOfWork).Find(period, persons);
            Assert.AreEqual(2, days.Count);
            //Assert.IsTrue(days[1].NotAvailable);
            //Assert.IsFalse(days[0].NotAvailable);
        }

        [Test]
        public void CannotFindOvertimeDaysBetweenDatesAndOnPersonsWhenPersonListIsEmpty()
        {
            var period = new DateOnlyPeriod(2009, 2, 1, 2009, 3, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(new DateOnly(2009, 2, 3), _person, true));
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(new DateOnly(2009, 3, 2), _person, false));
            IEnumerable<IPerson> persons = new Collection<IPerson>();
            IList<IOvertimeAvailability> days = new OvertimeAvailabilityRepository(UnitOfWork).Find(period, persons);
            Assert.AreEqual(0, days.Count);
        }

        [Test]
        public void CanFindOvertimeDaysOnDayAndPerson()
        {
            DateOnly date = new DateOnly(2009, 2, 1);
            IPerson person2 = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(person2);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date, _person, true));
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date, person2, false));
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date.AddDays(1), _person, false));

            IList<IOvertimeAvailability > days = new OvertimeAvailabilityRepository(UnitOfWork).Find(date, _person);
            Assert.AreEqual(1, days.Count);

            //Assert.IsTrue(days[0].NotAvailable);
        }

        [Test]
        public void CanFindOvertimeAvailabilityWithNullStartEndTime()
        {
            DateOnly date = new DateOnly(2009, 2, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date, _person, true));
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date.AddDays(1), _person, false));

            IList<IOvertimeAvailability> days = new OvertimeAvailabilityRepository(UnitOfWork).Find(date, _person);
            Assert.IsFalse( days[0].StartTime.HasValue);
            Assert.IsFalse( days[0].EndTime.HasValue);
            
        }
        
        [Test]
        public void CanFindOvertimeAvailabilityWithStartTime()
        {
            DateOnly date = new DateOnly(2009, 2, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date, _person, true,TimeSpan.FromHours(10),null ));
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date.AddDays(1), _person, false, TimeSpan.FromHours(8), null));

            IList<IOvertimeAvailability> days = new OvertimeAvailabilityRepository(UnitOfWork).Find(date, _person);

            var startTime = days[0].StartTime;
            if (startTime != null) 
                Assert.AreEqual( startTime.Value,TimeSpan.FromHours(10) );
            else
                Assert.Fail();
        }

        [Test]
        public void CanFindOvertimeAvailabilityWithEndTime()
        {
            DateOnly date = new DateOnly(2009, 2, 1);
            PersistAndRemoveFromUnitOfWork(CreateAggregateWithCorrectBusinessUnit());
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date, _person, true,null, TimeSpan.FromHours(5)));
            PersistAndRemoveFromUnitOfWork(createOvertimeAvailability(date.AddDays(1), _person, false,null, TimeSpan.FromHours(3)));

            IList<IOvertimeAvailability> days = new OvertimeAvailabilityRepository(UnitOfWork).Find(date, _person);

            var endTime = days[0].EndTime;
            if (endTime != null)
                Assert.AreEqual(endTime.Value, TimeSpan.FromHours(5));
            else
                Assert.Fail();
        }

        protected override IOvertimeAvailability CreateAggregateWithCorrectBusinessUnit()
        {
            IOvertimeAvailability availDay = createOvertimeAvailability(_dateOnly, _person, false);

            return availDay;
        }

        private static IOvertimeAvailability  createOvertimeAvailability(DateOnly date, IPerson person, bool notAvailable)
        {
            var overtimeAvailability = new OvertimeAvailability(person, date,null,null );
            overtimeAvailability.NotAvailable = notAvailable;
            return overtimeAvailability;
        }

        private static IOvertimeAvailability createOvertimeAvailability(DateOnly date, IPerson person, bool notAvailable,TimeSpan? startTime,TimeSpan? endTime)
        {
            var overtimeAvailability = new OvertimeAvailability(person, date, startTime, endTime);
            overtimeAvailability.NotAvailable = notAvailable;
            return overtimeAvailability;
        }

        protected override void VerifyAggregateGraphProperties(IOvertimeAvailability  loadedAggregateFromDatabase)
        {
            IOvertimeAvailability org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
            Assert.IsFalse(loadedAggregateFromDatabase.NotAvailable);
        }

        protected override Repository<IOvertimeAvailability> TestRepository(IUnitOfWork unitOfWork)
        {
            return new OvertimeAvailabilityRepository(unitOfWork);
        }

       
    }

   
}
