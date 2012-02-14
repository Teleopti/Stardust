using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewMaxOneDayOffRuleTest
    {
        private NewMaxOneDayOffRule _target;
        private MockRepository _mocks;

        private IPermissionInformation _permissionInformation;
        private ICccTimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _target = new NewMaxOneDayOffRule();
            _mocks = new MockRepository();
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
        }

        [Test]
        public void CanCreateRuleAndAccessSimpleProperties()
        {
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            // ska man inte kunna ändra
            _target.HaltModify = false;
            Assert.IsTrue(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
        }

        [Test]
        public void ValidateReturnsEmptyListIfOnlyOneDayOff()
        {
            var day = _mocks.StrictMock<IScheduleDay>();
            var days = new List<IScheduleDay> {day};
            var personDayOff = _mocks.StrictMock<IPersonDayOff>();
            var readOnlyDays = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> {personDayOff});

            Expect.Call(day.PersonDayOffCollection()).Return(readOnlyDays);
            _mocks.ReplayAll();

            var ret =_target.Validate(null, days);
            _mocks.VerifyAll();
            Assert.AreEqual(0,ret.Count());
        }

        [Test]
        public void ValidateReturnsListOffResponsesIfMoreThanOneDayOff()
        {
            var person = _mocks.StrictMock<IPerson>();
            var dayPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2010, 8, 23), _timeZone);
            var day = _mocks.StrictMock<IScheduleDay>();
            var days = new List<IScheduleDay> { day };
            var personDayOff = _mocks.StrictMock<IPersonDayOff>();
            var personDayOff2 = _mocks.StrictMock<IPersonDayOff>();
            var readOnlyDays = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { personDayOff, personDayOff2 });

            Expect.Call(day.PersonDayOffCollection()).Return(readOnlyDays);
            Expect.Call(day.Person).Return(person);
            Expect.Call(day.DateOnlyAsPeriod).Return(dayPeriod);
            Expect.Call(person.PermissionInformation).Return(_permissionInformation);
            Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone);
            _mocks.ReplayAll();

            var ret = _target.Validate(null, days);
            _mocks.VerifyAll();
            Assert.AreNotEqual(0, ret.Count());
        }
    }

    
}
