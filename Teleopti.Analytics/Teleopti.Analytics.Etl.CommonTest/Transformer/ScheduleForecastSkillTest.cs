using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class ScheduleForecastSkillTest
    {
        private IScheduleForecastSkill _target;
        private DateTime _startDateTime;
        const int IntervalId = 2;
        Guid _skillCode;
        Guid _scenarioCode;

        [SetUp]
        public void Setup()
        {
            _startDateTime = new DateTime(2009, 9, 8, 22, 30, 0);
            _skillCode = Guid.NewGuid();
            _scenarioCode = Guid.NewGuid();

            _target = new ScheduleForecastSkill(_startDateTime, IntervalId, _skillCode, _scenarioCode);
        }

        [Test]
        public void VerifyEquals()
        {
            DateTime now = DateTime.Now;
            Guid skillCode = Guid.NewGuid();
            Guid scenarioCode = Guid.NewGuid();
            IScheduleForecastSkill scheduleForecastSkill1 = new ScheduleForecastSkill(now, 1, skillCode, scenarioCode);
            IScheduleForecastSkillKey scheduleForecastSkillKey1 = new ScheduleForecastSkillKey(now, 1, skillCode, scenarioCode);
            IScheduleForecastSkillKey scheduleForecastSkillKey2 = new ScheduleForecastSkillKey(now, 1, skillCode, scenarioCode);
            //IScheduleForecastSkill scheduleForecastSkillOut;

            var scheduleForecastSkills =
                new Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill>(
                    new ScheduleForecastSkillEqualComparer());

            scheduleForecastSkills.Add(scheduleForecastSkillKey1, scheduleForecastSkill1);

            Assert.IsTrue(scheduleForecastSkills.TryGetValue(scheduleForecastSkillKey2, out _target));
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanSetAndAccessProperties()
        {
            
            const double forecastedResourcesMinutes = 4.4d;
            const double forecastedResources = 2.2d;
            const double forecastedResourcesIncludingShrinkageMinutes = 6.2d;
            const double forecastedResourcesIncludingShrinkage = 3.2d;
            const double scheduledResourcesMinutes = 4.8d;
            const double scheduledResources = 2.4d;
            const double scheduledResourcesIncludingShrinkageMinutes = 5.4d;
            const double scheduledResourcesIncludingShrinkage = 2.7d;
            Guid businessUnitCode = Guid.NewGuid();
            const string businessUnitName = "bu name";
            const int dataSourceId = 1;
            DateTime insertDate = DateTime.Now;
            DateTime updateDate = DateTime.Now;

            _target.ForecastedResourcesMinutes = forecastedResourcesMinutes;
            _target.ForecastedResources = forecastedResources;
            _target.ForecastedResourcesIncludingShrinkageMinutes = forecastedResourcesIncludingShrinkageMinutes;
            _target.ForecastedResourcesIncludingShrinkage = forecastedResourcesIncludingShrinkage;
            _target.ScheduledResourcesMinutes = scheduledResourcesMinutes;
            _target.ScheduledResources = scheduledResources;
            _target.ScheduledResourcesIncludingShrinkageMinutes = scheduledResourcesIncludingShrinkageMinutes;
            _target.ScheduledResourcesIncludingShrinkage = scheduledResourcesIncludingShrinkage;
            _target.BusinessUnitCode = businessUnitCode;
            _target.BusinessUnitName = businessUnitName;
            _target.DataSourceId = dataSourceId;
            _target.InsertDate = insertDate;
            _target.UpdateDate = updateDate;

            Assert.AreEqual(_startDateTime, _target.StartDateTime);
            Assert.AreEqual(IntervalId, _target.IntervalId);
            Assert.AreEqual(_scenarioCode, _target.ScenarioCode);
            Assert.AreEqual(_skillCode, _target.SkillCode);
            Assert.AreEqual(forecastedResourcesMinutes, _target.ForecastedResourcesMinutes);
            Assert.AreEqual(forecastedResources, _target.ForecastedResources);
            Assert.AreEqual(forecastedResourcesIncludingShrinkageMinutes, _target.ForecastedResourcesIncludingShrinkageMinutes);
            Assert.AreEqual(forecastedResourcesIncludingShrinkage, _target.ForecastedResourcesIncludingShrinkage);
            Assert.AreEqual(scheduledResourcesMinutes, _target.ScheduledResourcesMinutes);
            Assert.AreEqual(scheduledResources, _target.ScheduledResources);
            Assert.AreEqual(scheduledResourcesIncludingShrinkageMinutes, _target.ScheduledResourcesIncludingShrinkageMinutes);
            Assert.AreEqual(scheduledResourcesIncludingShrinkage, _target.ScheduledResourcesIncludingShrinkage);
            Assert.AreEqual(businessUnitCode, _target.BusinessUnitCode);
            Assert.AreEqual(businessUnitName, _target.BusinessUnitName);
            Assert.AreEqual(dataSourceId, _target.DataSourceId);
            Assert.AreEqual(insertDate, _target.InsertDate);
            Assert.AreEqual(updateDate, _target.UpdateDate);
        }
    }

}
