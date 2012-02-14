using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class SchedulingResultServiceTest
    {
        #region Variables

        private SchedulingResultService _target;
        private PersonAssignmentListContainer _personAssignmentListContainer;
        private ISkillSkillStaffPeriodExtendedDictionary _skillStaffPeriods;
        private DateTimePeriod _inPeriod;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            _inPeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 15, 00, DateTimeKind.Utc));
            _personAssignmentListContainer = PersonAssignmentFactory.CreatePersonAssignmentListForActivityDividerTest();
            _skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills);
        }

        #endregion

        #region Constructor tests

        [Test]
        public void VerifyConstructorOverload1()
        {
            ISkill deletedSkill = SkillFactory.CreateSkill("deleted");
            DateTimePeriod periodToLoad = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 1, 00, 00, 00, DateTimeKind.Utc), new DateTime(2008, 1, 3, 00, 00, 00, DateTimeKind.Utc));
            ICollection<IPerson> personColl = _personAssignmentListContainer.ContainedPersons.Values;

            IScheduleDateTimePeriod schedulePeriod = new ScheduleDateTimePeriod(periodToLoad);
            IScheduleDictionary scheduled = new ScheduleDictionary(_personAssignmentListContainer.Scenario, schedulePeriod);
            foreach (IPersonAssignment ass in _personAssignmentListContainer.PersonAssignmentListForActivityDividerTest)
            {
                ((ScheduleRange)scheduled[ass.Person]).AddRange(new List<IPersonAssignment> { ass });
            }

            ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary = SkillDayFactory.CreateSkillDaysForActivityDividerTest2(_personAssignmentListContainer.ContainedSkills);
            MockRepository mocks = new MockRepository();
            IDictionary<ISkill, IList<ISkillDay>> dic = CreateSkillDaysFromSkillStaffPeriodDictionary(mocks, skillStaffPeriodDictionary);
            dic.Add(deletedSkill, new List<ISkillDay>());
            mocks.ReplayAll();
            ISchedulingResultStateHolder stateHolder = new SchedulingResultStateHolder(personColl, scheduled, dic);
            _target = new SchedulingResultService(stateHolder);
            Assert.IsNotNull(_target);
            mocks.VerifyAll();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IDictionary<ISkill, IList<ISkillDay>> CreateSkillDaysFromSkillStaffPeriodDictionary(MockRepository mocks, ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary)
        {
            IDictionary<ISkill,IList<ISkillDay>> skillDaysDic = new Dictionary<ISkill, IList<ISkillDay>>();
            foreach (var pair in skillStaffPeriodDictionary)
            {
                var skillDays = new List<ISkillDay>();
                foreach (var skillStaffPeriod in pair.Value)
                {
                    ISkillDay skillDay = mocks.StrictMock<ISkillDay>();
                    Expect.Call(skillDay.Skill).Return(pair.Key).Repeat.Any();
                    Expect.Call(skillDay.CurrentDate).Return(new DateOnly(skillStaffPeriod.Key.StartDateTime.Date)).Repeat.Any();
                    Expect.Call(skillDay.SkillStaffPeriodCollection).Return(
                        new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> {skillStaffPeriod.Value}))
                        .Repeat.Any();
                    skillDays.Add(skillDay);
                }
                
                skillDaysDic.Add(pair.Key,skillDays);
            }
            return skillDaysDic;
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            _target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), false);
            Assert.IsNotNull(_target);
        }

        #endregion

        [Test]
        public void VerifySchedulingResultWithoutPeriod()
        {
            _target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), false);
            Assert.IsNotNull(_target);

            ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
            Assert.AreEqual(0.83,
                            outDic[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                                s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource,
                            0.01);
        }

        [Test]
        public void VerifySchedulingResultWithoutPeriodAndNoSchedules()
        {
            _target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, new List<IVisualLayerCollection>(), false);
            Assert.IsNotNull(_target);

            ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
            Assert.AreEqual(0.0 / _inPeriod.ElapsedTime().TotalMinutes,
                            outDic[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                                s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource,
                            0.001);
        }

        [Test]
        public void VerifySchedulingResultWithPeriod()
        {
            _target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills,_personAssignmentListContainer.TestVisualLayerCollection(), false);

            ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult(_inPeriod);
            Assert.AreEqual(0.83,
                            outDic[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource, 0.01);
        }

        [Test]
        public void VerifySchedulingPeriodDoNotIntersectSkillStaffPeriod()
        {
            DateTime skillDayDate = new DateTime(2009, 1, 2, 10, 00, 00, DateTimeKind.Utc);
            _skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills, skillDayDate);
            _target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), false);

            ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
            Assert.AreEqual(outDic, _skillStaffPeriods);
        }

        [Test]
        public void VerifyInjectedSkillSkillDayDictionaryIsCalculated()
        {
            _target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), false);
            _target.SchedulingResult(_inPeriod);
            Assert.AreEqual(0.83,
                            _skillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource, 0.01);
        }

        [Test]
        public void VerifyInjectedSkillSkillDayDictionaryIsTheSameAsOut()
        {
            _target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), false);
            ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult(_inPeriod);
            Assert.AreEqual(_skillStaffPeriods, outDic);
        }

        [Test]
        public void VerifyDataIfStateHolderInjected()
        {
            ISkill deletedSkill = SkillFactory.CreateSkill("deleted");
            DateTimePeriod periodToLoad = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 1, 00, 00, 00, DateTimeKind.Utc), new DateTime(2008, 1, 3, 00, 00, 00, DateTimeKind.Utc));
            ICollection<IPerson> personColl = _personAssignmentListContainer.ContainedPersons.Values;

            IScheduleDateTimePeriod schedulePeriod = new ScheduleDateTimePeriod(periodToLoad);
            IScheduleDictionary scheduled = new ScheduleDictionary(_personAssignmentListContainer.Scenario, schedulePeriod);
            foreach (IPersonAssignment ass in _personAssignmentListContainer.PersonAssignmentListForActivityDividerTest)
            {
                ((ScheduleRange)scheduled[ass.Person]).AddRange(new List<IPersonAssignment>{ass});
            }

            MockRepository mocks = new MockRepository();
            ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest2(_personAssignmentListContainer.ContainedSkills);
            IDictionary<ISkill, IList<ISkillDay>> dic = CreateSkillDaysFromSkillStaffPeriodDictionary(mocks,skillStaffPeriods);
            dic.Add(deletedSkill, new List<ISkillDay>());

            mocks.ReplayAll();
            ISchedulingResultStateHolder stateHolder = new SchedulingResultStateHolder(personColl, scheduled, dic);
            _target = new SchedulingResultService(stateHolder, _personAssignmentListContainer.AllSkills, false);
            ISkillSkillStaffPeriodExtendedDictionary retDic = _target.SchedulingResult(_inPeriod);

            Assert.IsNotNull(_target);
            Assert.AreEqual(0.832,
                            retDic[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                                s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource,
                            0.01);
            Assert.IsFalse(retDic.ContainsKey(deletedSkill));
            mocks.VerifyAll();
        }
    }
}
