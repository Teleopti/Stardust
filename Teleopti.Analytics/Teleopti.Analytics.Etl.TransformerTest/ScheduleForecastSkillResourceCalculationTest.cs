﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using SkillFactory = Teleopti.Ccc.TestCommon.FakeData.SkillFactory;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class ScheduleForecastSkillResourceCalculationTest
    {
        private ScheduleForecastSkillResourceCalculation _target;
        private MockRepository _mocks;
        private ISchedulingResultService _schedulingResultService;
        private IDictionary<ISkill, IList<ISkillDay>> _skillDaysDictionary;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultService = _mocks.StrictMock<ISchedulingResultService>();
            
            ISkill skill = SkillFactory.CreateSkill("Skill A", SkillTypeFactory.CreateSkillType(), 15);
            skill.SetId(Guid.NewGuid());
            
            ICollection<ISkillDay> skillDayCollection = ForecastFactory.CreateSkillDayCollection(new DateOnlyPeriod(2009,9,11,2009,9,11), skill);
            _skillDaysDictionary = new Dictionary<ISkill, IList<ISkillDay>>();
            _skillDaysDictionary.Add(skill, skillDayCollection.ToList());

            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod();
            IList<ISkillStaffPeriod> skillStaffPeriods = new List<ISkillStaffPeriod>{skillStaffPeriod};

            const int intervalPerDay = 96;

            _target = new ScheduleForecastSkillResourceCalculation(_skillDaysDictionary, _schedulingResultService, skillStaffPeriods, intervalPerDay);
        }

        [Test]
        public void VerifyGetResourceDataExcludingShrinkage()
        {
            using (_mocks.Record())
            {
                Expect.Call(_schedulingResultService.SchedulingResult()).Return(new SkillSkillStaffPeriodExtendedDictionary()).Repeat.Twice();
            }

            using (_mocks.Playback())
            {
                // First get 
                Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> resourceDataExcludingShrinkage =
                    _target.GetResourceDataExcludingShrinkage(DateTime.Now);

                KeyValuePair<IScheduleForecastSkillKey, IScheduleForecastSkill> exclShrinkage = resourceDataExcludingShrinkage.First();
                IScheduleForecastSkill scheduleForecastSkill;
                resourceDataExcludingShrinkage.TryGetValue(exclShrinkage.Key, out scheduleForecastSkill);

                Assert.IsNotNull(scheduleForecastSkill);
                Assert.Greater(scheduleForecastSkill.ForecastedResources, 0d);
                Assert.AreEqual(scheduleForecastSkill.ForecastedResourcesIncludingShrinkage, 0d);

                Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> resourceDataIncludingShrinkage =
                    _target.GetResourceDataIncludingShrinkage(DateTime.Now);

                Assert.AreSame(resourceDataExcludingShrinkage, resourceDataIncludingShrinkage);
                Assert.Greater(scheduleForecastSkill.ForecastedResourcesIncludingShrinkage, 0d);
            }
        }
    }
}
