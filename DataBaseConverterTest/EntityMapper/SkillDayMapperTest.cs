using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class SkillDayMapperTest
    {
        private SkillDayMapper _mapper;
        private MappedObjectPair _mop;
    	const int intervalLength = 15;
 
        [SetUp]
        public void Setup()
        {
            _mop = new MappedObjectPair();
            _mapper = new SkillDayMapper(_mop, new CccTimeZoneInfo(TimeZoneInfo.Utc), intervalLength);
        }

        [Test, SetUICulture("en-GB")]
        public void VerifyMapper()
        {
            const double tasks = 9;
            const int totalTime = 122 * (int)tasks;
            const int totalAfterCallWorkTime = 13 * (int)tasks;
            const int interval = 26;

            var skillDayFactory = new SkillDayFactory();

            //Scenario
            _mop.Scenario = skillDayFactory.ScenarioPair;

            //Forecast
            _mop.Workload = skillDayFactory.WorkloadPair;

            //Skill
            _mop.Skill = skillDayFactory.SkillPair;

            //CreateProjection open hours
            IList<global::Domain.TimePeriod> openHours = new List<global::Domain.TimePeriod>
                                                         	{
                                                         		new global::Domain.TimePeriod(
                                                         			TimeSpan.FromMinutes(interval*15),
                                                         			TimeSpan.FromMinutes(interval*15 + 15))
                                                         	};

        	//CreateProjection old forecaste data
            var oldForecastData = new global::Domain.ForecastData(interval, (int)tasks, totalTime, totalAfterCallWorkTime);
            IDictionary<int, global::Domain.ForecastData> ofdDic = new Dictionary<int, global::Domain.ForecastData>
                                                                   	{{interval, oldForecastData}};
        	var oldForecastDay = new global::Domain.ForecastDay(skillDayFactory.OldForecast, new DateTime(2007, 1, 1), skillDayFactory.OldScenario, null, ofdDic, null, null, openHours, "Teleoptic");

            //CreateProjection old skill data
            var oldSkillData = new global::Domain.SkillData(15,10,10,1,50,40,90,90,true,3,3,50,70, new global::Domain.SystemSetting(new Dictionary<int,int>(), skillDayFactory.OldScenario));
            IDictionary<int, global::Domain.SkillData> osdDic = new Dictionary<int, global::Domain.SkillData>
                                                                	{{interval, oldSkillData}};

        	IDictionary<global::Domain.IntegerDateKey, global::Domain.ForecastDay> fcdDic = new Dictionary<global::Domain.IntegerDateKey, global::Domain.ForecastDay>
        	                                                                                	{
        	                                                                                		{
        	                                                                                			new global::Domain.
        	                                                                                			IntegerDateKey(1,
        	                                                                                			               new DateTime(
        	                                                                                			               	2007, 1, 1))
        	                                                                                			, oldForecastDay
        	                                                                                			}
        	                                                                                	};

        	//CreateProjection old skill day
            var oldSkillDay = new global::Domain.SkillDay(
                skillDayFactory.OldSkill, 
                new DateTime(2007, 1, 1), 
                skillDayFactory.OldScenario, 
                osdDic, 
                fcdDic);

            ISkillDay skillDay = _mapper.Map(oldSkillDay);
            Assert.AreEqual(skillDayFactory.NewScenario, skillDay.Scenario);
            Assert.AreEqual(skillDayFactory.NewSkill, skillDay.Skill);
            Assert.AreEqual(1, skillDay.WorkloadDayCollection.Count);
            Assert.AreEqual(1, skillDay.WorkloadDayCollection[0].TaskPeriodList.Count);
            Assert.AreEqual("<NONE>", skillDay.TemplateReference.TemplateName);
            Assert.IsNotNull(skillDayFactory.NewWorkload); // For a little better Coverage.
        }
    }
}
