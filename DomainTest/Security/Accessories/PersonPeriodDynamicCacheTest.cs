using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Accessories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Accessories
{
    [TestFixture]
    public class PersonPeriodDynamicCacheTest
    {
        private IPersonPeriodDynamicCache _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new PersonPeriodDynamicCache();
            _target.Enabled = true;
        }

        [Test]
        public void VerifyEnabled()
        {
            bool setValue = false;
            _target.Enabled = setValue;
            Assert.AreEqual(setValue, _target.Enabled);

            setValue = true;
            _target.Enabled = setValue;
            Assert.AreEqual(setValue, _target.Enabled);
        }

        [Test]
        public void VerifyPersonPeriodsWithOneCall()
        {
            IPerson person = _mocks.StrictMock<IPerson>();
            DateTimePeriod period = new DateTimePeriod();
            IList<IPersonPeriod> resultList = new List<IPersonPeriod>();

            _mocks.Record();

            Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(resultList).Repeat.Once();
            Expect.Call(person.PermissionInformation).Return(new PermissionInformation(person));

            _mocks.ReplayAll();

            _target.PersonPeriods(person, period);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyPersonPeriodsWithTwoCallsSameData()
        {
            IPerson person = _mocks.StrictMock<IPerson>();
            DateTimePeriod period = new DateTimePeriod();
            IList<IPersonPeriod> resultList = new List<IPersonPeriod>();

            _mocks.Record();

            Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(resultList).Repeat.Once();
            Expect.Call(person.Equals(person)).Return(true).Repeat.Once();
            Expect.Call(person.PermissionInformation).Return(new PermissionInformation(person));

            _mocks.ReplayAll();

            _target.PersonPeriods(person, period);
            _target.PersonPeriods(person, period);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyPersonPeriodsWithTwoCallsDifferentPersons()
        {
            IPerson person = _mocks.StrictMock<IPerson>();
            IPerson person2 = _mocks.StrictMock<IPerson>();
            DateTimePeriod period = new DateTimePeriod();
            IList<IPersonPeriod> resultList = new List<IPersonPeriod>();

            _mocks.Record();

            Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(resultList).Repeat.Once();
            Expect.Call(person2.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(resultList).Repeat.Once();
            Expect.Call(person.Equals(person2)).Return(false).Repeat.Once();
            Expect.Call(person.PermissionInformation).Return(new PermissionInformation(person));
            Expect.Call(person2.PermissionInformation).Return(new PermissionInformation(person2));

            _mocks.ReplayAll();

            _target.PersonPeriods(person, period);
            _target.PersonPeriods(person2, period);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyPersonPeriodsWithTwoCallsDifferentPeriods()
        {
            IPerson person = _mocks.StrictMock<IPerson>();
            DateTimePeriod period = new DateTimePeriod();
            DateTimePeriod period2 = new DateTimePeriod();
            IList<IPersonPeriod> resultList = new List<IPersonPeriod>();

            _mocks.Record();

            Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(resultList).Repeat.Once();
            Expect.Call(person.PersonPeriods(new DateOnlyPeriod())).IgnoreArguments().Return(resultList).Repeat.Once();
            Expect.Call(person.Equals(person)).Return(false).Repeat.Once();
            Expect.Call(person.PermissionInformation).Return(new PermissionInformation(person)).Repeat.AtLeastOnce();

            _mocks.ReplayAll();

            _target.PersonPeriods(person, period);
            _target.PersonPeriods(person, period2);

            _mocks.VerifyAll();
        }
    }
}
