using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.DomainTest.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class PersonAssignmentModifyBusinessRulesTest
    {
        private PersonAssignmentModifyBusinessRules _target;
        private MockRepository _mocker;
        private IScheduleDictionary _dictionary;
        private ISchedulePart _schedulePart;
        private IScenario _scenario;
        private IPerson _person;
        private DateTimePeriod _period;
        private IDictionary<IPerson, IScheduleRange> _sourceDictionary;
        private IScheduleDateTimePeriod _sccheduleDateTimePeriod;
        

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _sourceDictionary = new Dictionary<IPerson, IScheduleRange>();
            _scenario = new Scenario("for test");
            _person = new Person();
            _period = new DateTimePeriod(2001,1,1,2001,1,10);
            _sccheduleDateTimePeriod = new ScheduleDateTimePeriod(_period); 
            _dictionary = new ScheduleDictionaryForTest(_scenario, _sccheduleDateTimePeriod, _sourceDictionary);
            _schedulePart = CreatePart(_dictionary);
            _target = new PersonAssignmentModifyBusinessRules(_schedulePart);

        }

        [Test]
        public void VerifyIncludesPersonAccountRule()
        {
            Assert.AreEqual(1, _target.BusinessRules.OfType<PersonAccountRule>().Count());
        }

        [Test]
        public void VerifyThatCorrectOriginalIsExtractedFromStateHolder()
        {
            IScheduleRange range = _mocker.CreateMock<IScheduleRange>();
            _sourceDictionary.Add(_person,range);
            ISchedulingResultStateHolder stateHolder = _mocker.CreateMock<ISchedulingResultStateHolder>();
            IPersonAccountDataProvider provider = _mocker.CreateMock<IPersonAccountDataProvider>();
            using(_mocker.Record())
            {
                Expect.Call(stateHolder.Schedules).Return(_dictionary).Repeat.Twice();
                Expect.Call(stateHolder.PersonAccountProvider).Return(provider);
            }
            using (_mocker.Playback())
            {
                PersonAssignmentModifyBusinessRules businessRules =
                    new PersonAssignmentModifyBusinessRules(_schedulePart, stateHolder);
                IScheduleRange calculatedRange = ((PersonAccountRule) businessRules.BusinessRules[0]).OriginalRange;
                Assert.AreEqual(range, calculatedRange);
                businessRules = new PersonAssignmentModifyBusinessRules(_schedulePart);
                Assert.IsNull(((PersonAccountRule) businessRules.BusinessRules[0]).OriginalRange);
            }
        }

        
        private  ISchedulePart CreatePart(IScheduleDictionary dictionary)
        {
            return new SchedulePartUsedInTest(dictionary, new ScheduleParameters(_scenario, _person, _period), true, true);
        }
    }
}
