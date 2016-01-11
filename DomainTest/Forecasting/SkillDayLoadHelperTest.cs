using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the SkillDayLoadHelper class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-05-08
    /// </remarks>
    [TestFixture]
    public class SkillDayLoadHelperTest
    {
        private DateOnlyPeriod _dtp;
        private IList<ISkill> _skills;
        private IScenario _scenario;
        private IMultisiteDayRepository _multisiteDayRep;
        private ISkillDayRepository _skillDayRep;
        private MockRepository _mocks;
    	private ISkillDayLoadHelper _target;

    	/// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _multisiteDayRep = _mocks.StrictMock<IMultisiteDayRepository>();
            _skillDayRep = _mocks.StrictMock<ISkillDayRepository>();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _dtp = new DateOnlyPeriod(new DateOnly(2008, 7, 16), new DateOnly(2008, 7, 19));
            _skills = new List<ISkill>
                          {
                              SkillFactory.CreateSkill("skill2")
                          };
        	_target = new SkillDayLoadHelper(_skillDayRep,_multisiteDayRep);
        }

        /// <summary>
        /// Verifies the load with multisite days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        [Test]
        public void VerifyLoadWithMultisiteDays()
        {
            IMultisiteSkill multisiteSkill = SkillFactory.CreateMultisiteSkill("skill1");
            _skills.Add(multisiteSkill);
                
            IList<ISkillDay> skillDays1 = new List<ISkillDay>
                                              { 
                                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, _dtp.StartDate),
                                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, _dtp.StartDate.AddDays(1)),
                                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, _dtp.StartDate.AddDays(2))
                                                             };

            IList<ISkillDay> skillDays2 = new List<ISkillDay>
                                              { 
                                                                 SkillDayFactory.CreateSkillDay(_skills[0], _dtp.StartDate),
                                                                 SkillDayFactory.CreateSkillDay(_skills[0], _dtp.StartDate.AddDays(1)),
                                                                 SkillDayFactory.CreateSkillDay(_skills[0], _dtp.StartDate.AddDays(2))
                                                             };

            var allSkillDays = new List<ISkillDay>(skillDays1);
            allSkillDays.AddRange(skillDays2);
            IList<IMultisiteDay> multisiteDays = new List<IMultisiteDay>
                                                    {
                                                                             new MultisiteDay(_dtp.StartDate,multisiteSkill,_scenario),
                                                                             new MultisiteDay(_dtp.StartDate.AddDays(1),multisiteSkill,_scenario),
                                                                             new MultisiteDay(_dtp.StartDate.AddDays(2),multisiteSkill,_scenario)
                                                                         };

            Expect.Call(_skillDayRep.FindReadOnlyRange(new DateOnlyPeriod(_dtp.StartDate.AddDays(-8), _dtp.EndDate.AddDays(2)), _skills, _scenario)).Return(allSkillDays).Repeat.Once();
            
            Expect.Call(_multisiteDayRep.FindRange(SkillDayCalculator.GetPeriodToLoad(_dtp),multisiteSkill, _scenario)).Return(multisiteDays).Repeat.Once();

            _mocks.ReplayAll();
            var result = _target.LoadSchedulerSkillDays(_dtp, _skills, _scenario);
            var resultSkillDays = result[multisiteSkill];

            Assert.AreEqual(3, resultSkillDays.Count);
            Assert.AreEqual(multisiteDays[0].MultisiteSkillDay,resultSkillDays.ElementAt(0));
            Assert.AreEqual(multisiteDays[1].MultisiteSkillDay, resultSkillDays.ElementAt(1)); 
            Assert.AreEqual(multisiteDays[2].MultisiteSkillDay, resultSkillDays.ElementAt(2));
            Assert.IsNotNull(resultSkillDays.ElementAt(0).SkillDayCalculator);
            Assert.IsNotNull(resultSkillDays.ElementAt(2).SkillDayCalculator);
            _mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the load without multisite days.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        [Test]
        public void VerifyLoadWithoutMultisiteDays()
        {
            IList<ISkillDay> skillDays = new List<ISkillDay>
                                             { 
                                                                 SkillDayFactory.CreateSkillDay(_skills[0], _dtp.StartDate),
                                                                 SkillDayFactory.CreateSkillDay(_skills[0], _dtp.StartDate.AddDays(1)),
                                                                 SkillDayFactory.CreateSkillDay(_skills[0], _dtp.StartDate.AddDays(2))
                                                             };

            Expect.Call(_skillDayRep.FindReadOnlyRange(new DateOnlyPeriod(_dtp.StartDate.AddDays(-8), _dtp.EndDate.AddDays(2)), _skills, _scenario)).Return(skillDays).Repeat.Once();
            
            _mocks.ReplayAll();
            var result = _target.LoadSchedulerSkillDays(_dtp, _skills, _scenario);

            var resultSkillDays = result[_skills[0]];
            Assert.AreEqual(3, resultSkillDays.Count);
            Assert.AreEqual(skillDays[0], resultSkillDays.ElementAt(0));
            Assert.AreEqual(skillDays[1], resultSkillDays.ElementAt(1));
            Assert.AreEqual(skillDays[2], resultSkillDays.ElementAt(2));
            Assert.IsNotNull(resultSkillDays.ElementAt(0).SkillDayCalculator);
            Assert.IsNotNull(resultSkillDays.ElementAt(2).SkillDayCalculator);
            _mocks.VerifyAll();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyLoadBudgetSkillDaysWithMultisiteSkill()
        {
            var multisiteSkill = SkillFactory.CreateMultisiteSkill("skill1");
            _skills.Add(multisiteSkill);
            _mocks.ReplayAll();

            _target.LoadBudgetSkillDays(_dtp, _skills, _scenario);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyLoadBudgetSkillDays()
        {
            var multisiteSkill = SkillFactory.CreateMultisiteSkill("multisite skill");
            var childSkill = SkillFactory.CreateChildSkill("Sub 1", multisiteSkill);
            multisiteSkill.AddChildSkill(childSkill);
            _skills.Add(childSkill);
            var skillToBeLoad = new List<ISkill> {_skills[0], multisiteSkill};
            IList<ISkillDay> skillDays = new List<ISkillDay>
                                             {
                                                 SkillDayFactory.CreateSkillDay(_skills[0], _dtp.StartDate.AddDays(1)),
                                                 SkillDayFactory.CreateSkillDay(multisiteSkill, _dtp.StartDate),
                                                 SkillDayFactory.CreateSkillDay(childSkill,_dtp.StartDate)
                                             };
            IList<IMultisiteDay> multisiteDays = new List<IMultisiteDay>
                                             {
                                                 new MultisiteDay(_dtp.StartDate, multisiteSkill, _scenario)
                                             };
            Expect.Call(_skillDayRep.FindReadOnlyRange(new DateOnlyPeriod(_dtp.StartDate.AddDays(-8), _dtp.EndDate.AddDays(2)),
                                               skillToBeLoad, _scenario)).Return(skillDays).Repeat.Any();
            Expect.Call(_multisiteDayRep.FindRange(SkillDayCalculator.GetPeriodToLoad(_dtp), multisiteSkill, _scenario)).Return(multisiteDays).Repeat.Once();

            Expect.Call(_skillDayRep.FindRange(SkillDayCalculator.GetPeriodToLoad(_dtp),
                                               childSkill, _scenario)).Return(new List<ISkillDay> { skillDays[2] }).Repeat.Once();
            _mocks.ReplayAll();

            var result = _target.LoadBudgetSkillDays(_dtp, _skills, _scenario);
            var resultSkillDays = result[childSkill];
            
            Assert.AreEqual(1, resultSkillDays.Count);
            _mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the null values give empty results.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-08
        /// </remarks>
        [Test]
        public void VerifyNullValuesGiveEmptyResults()
        {
            IDictionary<ISkill,IList<ISkillDay>> result;
            result = _target.LoadSchedulerSkillDays(_dtp, null, _scenario);
            Assert.AreEqual(0, result.Count);
            result = _target.LoadSchedulerSkillDays(_dtp, _skills, null);
            Assert.AreEqual(0, result.Count);
        }
    }
}