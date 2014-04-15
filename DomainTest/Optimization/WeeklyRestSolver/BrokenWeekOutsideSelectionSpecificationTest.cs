using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.DomainTest.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class BrokenWeekOutsideSelectionSpecificationTest
    {
        private IBrokenWeekOutsideSelectionSpecification _target;
        private IEnsureWeeklyRestRule _ensureWeeklyRestRule;
        private MockRepository _mock;
        private IPerson _person;
        private DateOnlyPeriod _weekPeriod;
        private PersonWeek _personWeekBefore;
        private PersonWeek _personWeekAfter;
        private IScheduleRange _personScheduleRange;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personScheduleRange = _mock.StrictMock<IScheduleRange>();
            _person = PersonFactory.CreatePerson();
            _personWeekBefore = new PersonWeek(_person, new DateOnlyPeriod(2014,03,19,2014,03,24));
            _personWeekAfter = new PersonWeek(_person, new DateOnlyPeriod(2014,03,26,2014,04,03));
            _weekPeriod = new DateOnlyPeriod(2014,03,20,2014,03,27);
            _ensureWeeklyRestRule = _mock.StrictMock<IEnsureWeeklyRestRule>();
            _target = new BrokenWeekOutsideSelectionSpecification(_ensureWeeklyRestRule);
        }

        [Test]
        public void ReturnFalseIfNoPreviousOrNextWeekFound()
        {
            PersonWeek personWeek = new PersonWeek(_person,_weekPeriod);
            var selectedPersonWeek = new List<PersonWeek>() { personWeek};
            var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
            weeklyRestInPersonWeek.Add(_personWeekBefore,TimeSpan.FromHours( 24));
            weeklyRestInPersonWeek.Add(personWeek ,TimeSpan.FromHours( 26));
            weeklyRestInPersonWeek.Add(_personWeekAfter,TimeSpan.FromHours( 24));
            Assert.IsFalse(_target.IsSatisfy(personWeek,selectedPersonWeek,weeklyRestInPersonWeek,_personScheduleRange));
            
        }

        [Test]
        public void ReturnTrueIfPreviousWeekIsBroken()
        {
            var personWeek = new PersonWeek(_person, _weekPeriod);
            var selectedPersonWeek = new List<PersonWeek>() {_personWeekBefore , personWeek,_personWeekAfter  };
            var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
            weeklyRestInPersonWeek.Add(_personWeekBefore, TimeSpan.FromHours(24));
            weeklyRestInPersonWeek.Add(personWeek, TimeSpan.FromHours(26));
            weeklyRestInPersonWeek.Add(_personWeekAfter, TimeSpan.FromHours(24));
            using (_mock.Record())
            {
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeekBefore, _personScheduleRange,
                    TimeSpan.FromHours(24))).IgnoreArguments().Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue( _target.IsSatisfy(personWeek, selectedPersonWeek, weeklyRestInPersonWeek, _personScheduleRange));
            }
        }

        [Test]
        public void ReturnTrueIfNextWeekIsBroken()
        {
            var personWeek = new PersonWeek(_person, _weekPeriod);
            var selectedPersonWeek = new List<PersonWeek>() { _personWeekBefore, personWeek, _personWeekAfter };
            var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
            weeklyRestInPersonWeek.Add(_personWeekBefore, TimeSpan.FromHours(24));
            weeklyRestInPersonWeek.Add(personWeek, TimeSpan.FromHours(26));
            weeklyRestInPersonWeek.Add(_personWeekAfter, TimeSpan.FromHours(24));
            using (_mock.Record())
            {
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeekBefore, _personScheduleRange,
                    TimeSpan.FromHours(24))).IgnoreArguments().Return(true);
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeekAfter , _personScheduleRange,
                    TimeSpan.FromHours(24))).IgnoreArguments().Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfy(personWeek, selectedPersonWeek, weeklyRestInPersonWeek, _personScheduleRange));
            }
        }

        [Test]
        public void ReturnFalseIfNoWeekIsBroken()
        {
            var personWeek = new PersonWeek(_person, _weekPeriod);
            var selectedPersonWeek = new List<PersonWeek>() { _personWeekBefore, personWeek, _personWeekAfter };
            var weeklyRestInPersonWeek = new Dictionary<PersonWeek, TimeSpan>();
            weeklyRestInPersonWeek.Add(_personWeekBefore, TimeSpan.FromHours(24));
            weeklyRestInPersonWeek.Add(personWeek, TimeSpan.FromHours(26));
            weeklyRestInPersonWeek.Add(_personWeekAfter, TimeSpan.FromHours(24));
            using (_mock.Record())
            {
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeekBefore, _personScheduleRange,
                    TimeSpan.FromHours(24))).IgnoreArguments().Return(true);
                Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeekAfter, _personScheduleRange,
                    TimeSpan.FromHours(24))).IgnoreArguments().Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfy(personWeek, selectedPersonWeek, weeklyRestInPersonWeek, _personScheduleRange));
            }
        }
    }

    
}
