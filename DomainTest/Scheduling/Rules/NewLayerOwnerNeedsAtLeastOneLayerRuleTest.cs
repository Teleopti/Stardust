using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewLayerOwnerNeedsAtLeastOneLayerRuleTest
    {
        private IPerson _person;
        private NewLayerOwnerNeedsAtLeastOneLayerRule _target;
        private IPersonAssignment _ass;
        //private IScheduleRange range;
        private MockRepository _mocks;
        private IList<IScheduleDay> _days;
        private ReadOnlyCollection<IPersonAssignment> _personAssignments;
        
       [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
           _person = PersonFactory.CreatePersonWithBasicPermissionInfo("hej", "svejs");
            var scenario = ScenarioFactory.CreateScenarioAggregate();
            //var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            //var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(period), new Dictionary<IPerson, IScheduleRange>());
            _ass = createFullAss(scenario);
           var asses = new List<IPersonAssignment> {_ass};
           _personAssignments = new ReadOnlyCollection<IPersonAssignment>(asses);
            //range = new ScheduleRange(dic, new ScheduleParameters(scenario, _ass.Person, period));
            //((Schedule)range).Add(_ass);
            _target = new NewLayerOwnerNeedsAtLeastOneLayerRule();
            var day = _mocks.StrictMock<IScheduleDay>();
            _days = new List<IScheduleDay> {day};

           Expect.Call(day.PersonAssignmentCollection()).Return(_personAssignments);
           Expect.Call(day.Person).Return(_person);
           _mocks.ReplayAll();
        }

        [Test]
        public void CannotChangeHaltModify()
        {
            Assert.IsTrue(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            // ska man inte kunna ändra
            _target.HaltModify = false;
            Assert.IsTrue(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
        }

        [Test]
        public void VerifyOk()
        {
            Assert.AreEqual(0,_target.Validate(null,_days).Count());
            _mocks.VerifyAll();
        }

        [Test]
        public void MainShiftWithoutLayer()
        {
            _ass.MainShift.LayerCollection.Clear();
            var ret = _target.Validate(null, _days);
            Assert.AreNotEqual(0,ret.Count());
            StringAssert.Contains("MainShift", ret.First().Message);
            _mocks.VerifyAll();
        }

        [Test]
        public void PersonalShiftWithoutLayer()
        {
            _ass.PersonalShiftCollection[0].LayerCollection.Clear();
            var ret = _target.Validate(null, _days);
            Assert.AreNotEqual(0, ret.Count());
            StringAssert.Contains("PersonalShift", ret.First().Message);
            _mocks.VerifyAll();
        }

        [Test]
        public void OvertimeShiftWithoutLayer()
        {
            _ass.OvertimeShiftCollection[0].LayerCollection.Clear();
            var ret = _target.Validate(null, _days);
            Assert.AreNotEqual(0, ret.Count());
            StringAssert.Contains("OvertimeShift", ret.First().Message);
            _mocks.VerifyAll();
        }

        private static IPersonAssignment createFullAss(IScenario scenario)
        {
            var p = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
            var act = new Activity("ddsdf");
            var mShift = MainShiftFactory.CreateMainShift(act, p, new ShiftCategory("d"));
            var pShift = PersonalShiftFactory.CreatePersonalShift(act, p);
            var oShift = new OvertimeShift();
            var per = new Person();
            var pAss = new PersonAssignment(per, scenario);

            pAss.SetMainShift(mShift);
            pAss.AddPersonalShift(pShift);
            pAss.AddOvertimeShift(oShift);
            var defSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("sd",
                                                                                            MultiplicatorType.Overtime);
            var pContract = PersonContractFactory.CreatePersonContract();
            pContract.Contract.AddMultiplicatorDefinitionSetCollection(defSet);
            var pPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1800, 1, 1), pContract, new Team());

            per.AddPersonPeriod(pPeriod);
            oShift.LayerCollection.Add(new OvertimeShiftActivityLayer(act, p, defSet));
            return pAss;
        }

    }

    
}
