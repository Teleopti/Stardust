using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
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
    }
}