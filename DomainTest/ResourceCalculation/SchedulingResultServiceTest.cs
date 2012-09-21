using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
			_target = new SchedulingResultService(_skillStaffPeriods, 
				_personAssignmentListContainer.AllSkills, 
				_personAssignmentListContainer.TestVisualLayerCollection(), 
				new SingleSkillLoadedDecider(), 
				new SingleSkillCalculator(), 
				false);
        }

        #endregion

        #region Constructor tests

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
            Assert.IsNotNull(_target);
        }

        #endregion

        [Test]
        public void VerifySchedulingResultWithoutPeriod()
        {
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
        	_target = new SchedulingResultService(_skillStaffPeriods,
        	                                      _personAssignmentListContainer.AllSkills,
        	                                      new List<IVisualLayerCollection>(),
        	                                      new SingleSkillLoadedDecider(),
        	                                      new SingleSkillCalculator(),
        	                                      false);
            Assert.IsNotNull(_target);

            ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
            Assert.AreEqual(0.0 / _inPeriod.ElapsedTime().TotalMinutes,
                            outDic[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                                s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource,
                            0.001);
        }

        
        [Test]
        public void VerifySchedulingPeriodDoNotIntersectSkillStaffPeriod()
        {
            DateTime skillDayDate = new DateTime(2009, 1, 2, 10, 00, 00, DateTimeKind.Utc);
            _skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills, skillDayDate);
			_target = new SchedulingResultService(_skillStaffPeriods,
				_personAssignmentListContainer.AllSkills,
				_personAssignmentListContainer.TestVisualLayerCollection(),
				new SingleSkillLoadedDecider(),
				new SingleSkillCalculator(),
				false);

            ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
            Assert.AreEqual(outDic, _skillStaffPeriods);
        }
    }
}
