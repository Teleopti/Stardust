using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class WorkloadDayMapperTest
    {
        private WorkloadDayMapper _mapper;
        private MappedObjectPair _mop;
        private const int intervalLength = 15;
        private SkillDayFactory _skillDayFactory;

        [SetUp]
        public void Setup()
        {
            _mop = new MappedObjectPair();
            _mapper = new WorkloadDayMapper(_mop, new CccTimeZoneInfo(TimeZoneInfo.Utc), intervalLength);

            _skillDayFactory = new SkillDayFactory();
            _mop.Scenario = _skillDayFactory.ScenarioPair;
            _mop.Workload = _skillDayFactory.WorkloadPair;
            _mop.Skill = _skillDayFactory.SkillPair;
        }

		[Test, SetUICulture("en-GB")]
        public void VerifyMapper()
        {
            const double tasks = 9;
            const int totalTime = 122*(int)tasks;
            const int totalAfterCallWorkTime = 13 * (int)tasks;
            const int interval = 26;


            //Create open hours
            IList<global::Domain.TimePeriod> openHours = new List<global::Domain.TimePeriod>
                                                         	{
                                                         		new global::Domain.TimePeriod(
                                                         			TimeSpan.FromMinutes(interval*intervalLength),
                                                         			TimeSpan.FromMinutes(interval*intervalLength +
                                                         			                     intervalLength))
                                                         	};

        	//Create old forecaste data
            var oldForecastData = new global::Domain.ForecastData(interval, (int)tasks, totalTime, totalAfterCallWorkTime);
            IDictionary<int, global::Domain.ForecastData> ofdDic = new Dictionary<int, global::Domain.ForecastData>
                                                                   	{{interval, oldForecastData}};
        	var oldForecastDay = new global::Domain.ForecastDay(_skillDayFactory.OldForecast, new DateTime(2007, 1, 1), _skillDayFactory.OldScenario, null, ofdDic, null, null, openHours, "Teleoptic");

            WorkloadDay workloadDay = _mapper.Map(oldForecastDay);
            Assert.AreEqual(1,workloadDay.TaskPeriodList.Count);
            Assert.AreEqual("<NONE>", workloadDay.TemplateReference.TemplateName);
            Assert.AreEqual("Teleoptic", workloadDay.Annotation);
        }

        [Test]
        public void VerifyMapperWhenSwitchingFromDst()
        {
            const double tasks = 9;
            const int totalTime = 122 * (int)tasks;
            const int totalAfterCallWorkTime = 13 * (int)tasks;
            const int startInterval = 3;
            const int endInterval = 16;

            _mapper = new WorkloadDayMapper(_mop, new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), intervalLength);

            //Create open hours
            IList<global::Domain.TimePeriod> openHours = new List<global::Domain.TimePeriod>
                                                         	{
                                                         		new global::Domain.TimePeriod(
                                                         			TimeSpan.FromMinutes(startInterval*intervalLength),
                                                         			TimeSpan.FromMinutes(endInterval*intervalLength +
                                                         			                     intervalLength))
                                                         	};

        	//Create old forecaste data
            IDictionary<int, global::Domain.ForecastData> ofdDic = new Dictionary<int, global::Domain.ForecastData>();
            for (int interval = startInterval; interval <= endInterval; interval++)
            {
                var oldForecastData = new global::Domain.ForecastData(interval, (int)tasks, totalTime, totalAfterCallWorkTime);
                ofdDic.Add(interval, oldForecastData);
            }

            var oldForecastDay = new global::Domain.ForecastDay(_skillDayFactory.OldForecast, new DateTime(2006, 10, 29), _skillDayFactory.OldScenario, null, ofdDic, null, null, openHours, "Teleoptic");

            WorkloadDay workloadDay = _mapper.Map(oldForecastDay);
            Assert.AreEqual(14, workloadDay.TaskPeriodList.Count);
        }


        [Test]
        public void VerifyMapperWhenSwitchingToDst()
        {
            const double tasks = 9;
            const int totalTime = 122 * (int)tasks;
            const int totalAfterCallWorkTime = 13 * (int)tasks;
            const int startInterval = 3;
            const int endInterval = 16;

            _mapper = new WorkloadDayMapper(_mop, new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), intervalLength);

            //Create open hours
            IList<global::Domain.TimePeriod> openHours = new List<global::Domain.TimePeriod>
                                                         	{
                                                         		new global::Domain.TimePeriod(
                                                         			TimeSpan.FromMinutes(startInterval*intervalLength),
                                                         			TimeSpan.FromMinutes(endInterval*intervalLength +
                                                         			                     intervalLength))
                                                         	};

        	//Create old forecaste data
            IDictionary<int, global::Domain.ForecastData> ofdDic = new Dictionary<int, global::Domain.ForecastData>();
            for (int interval = startInterval; interval <= endInterval; interval++)
            {
                var oldForecastData = new global::Domain.ForecastData(interval, (int)tasks, totalTime, totalAfterCallWorkTime);
                ofdDic.Add(interval, oldForecastData);
            }

            var oldForecastDay = new global::Domain.ForecastDay(_skillDayFactory.OldForecast, new DateTime(2008, 3, 30), _skillDayFactory.OldScenario, null, ofdDic, null, null, openHours, "Teleoptic");

            WorkloadDay workloadDay = _mapper.Map(oldForecastDay);
            Assert.AreEqual(14, workloadDay.TaskPeriodList.Count);
        }

    }
}
