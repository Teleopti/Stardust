using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewPersonAccountRuleTest
    {
        private INewBusinessRule _target;
        private ISchedulingResultStateHolder _stateHolder;
        private IDictionary<IPerson, IPersonAccountCollection> _allAccounts;


        [SetUp]
        public void Setup()
        {
            _stateHolder = new SchedulingResultStateHolder();
            _allAccounts = new Dictionary<IPerson, IPersonAccountCollection>();
            _target = new NewPersonAccountRule(_stateHolder, _allAccounts);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsTrue(_target.HaltModify);
            Assert.IsFalse(_target.IsMandatory);
            Assert.IsFalse(_target.ForDelete);
            Assert.AreEqual(string.Empty, _target.ErrorMessage);
        }

        [Test]
        public void RuleShouldReturnResponseForEverydayInLoadedPartsOfAccountPeriodIfAlarmed()
        {
            Assert.IsTrue(_target.HaltModify);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "NUnit.Framework.Assert.Fail(System.String)"), Test]
        public void ShouldSkipPersonAccountValidationInOtherScenarioThanDefault()
        {
            var mocks = new MockRepository();
            var scheduleDay = mocks.Stub<IScheduleDay>();
            var person = PersonFactory.CreatePerson();
            using (mocks.Record())
            {
                scheduleDay.Stub(x => x.Person).Do(new Func<IPerson>(() =>
                {
                    Assert.Fail(
                        "Should not process other scenario than default.");
                    return (IPerson)null;
                }));
            }
            using (mocks.Playback())
            {
                _allAccounts.Add(person, new PersonAccountCollection(person));

                var otherScenario = ScenarioFactory.CreateScenarioAggregate("what if?", false, false);
                _stateHolder.Schedules = new ScheduleDictionary(otherScenario,
                                                                new ScheduleDateTimePeriod(new DateTimePeriod()));

                _target.Validate(new Dictionary<IPerson, IScheduleRange> { { person, null } }, new[] { scheduleDay }).Should().Be.Empty();
            }
        }

        [Test]
        public void ShouldUpdateRuleResponse()
        {
            var mocks = new MockRepository();
            var scheduleDay = mocks.StrictMock<IScheduleDay>();
            var person = PersonFactory.CreatePerson();
            var range = new DateTimePeriod(2012, 12, 14, 2012, 12, 14);
            var personAccountCollection = new PersonAccountCollection(person);
            var absence1 = AbsenceFactory.CreateAbsence("ab1");
            absence1.Tracker = Tracker.CreateDayTracker();
            var absense2 = AbsenceFactory.CreateAbsence("ab2");
            absense2.Tracker = Tracker.CreateDayTracker();
            var account1 = new PersonAbsenceAccount(person, absence1);
            var account2 = new PersonAbsenceAccount(person, absense2);
            account1.Add(new AccountDay(new DateOnly(2012, 12, 13)) { BalanceOut = TimeSpan.FromDays(2), Accrued = TimeSpan.FromDays(1) });
            account2.Add(new AccountDay(new DateOnly(2012, 12, 13)) { BalanceOut = TimeSpan.FromDays(1), Accrued = TimeSpan.FromDays(1) });
            personAccountCollection.Add(account1);
            personAccountCollection.Add(account2);

            _allAccounts.Add(person, personAccountCollection);
            var scenario = new Scenario("Default") { DefaultScenario = true };
            _stateHolder.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(range));

            using (mocks.Record())
            {
                Expect.Call(scheduleDay.Person).Return(person).Repeat.AtLeastOnce();
                Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(
                                                                     new DateOnly(2012, 12, 14),
                                                                     new CccTimeZoneInfo(
                                                                         TimeZoneInfo.FindSystemTimeZoneById(
                                                                             "W. Europe Standard Time"))));

            }
            using (mocks.Playback())
            {
                var responses = _target.Validate(new Dictionary<IPerson, IScheduleRange>
                                     {
                                         {
                                             person,
                                             new ScheduleRange(_stateHolder.Schedules,
                                                               new ScheduleParameters(scenario, person, range))
                                             }
                                     }, new Collection<IScheduleDay> { scheduleDay });
                Assert.That(responses.Count(), Is.EqualTo(1));
            }
        }
    }
}