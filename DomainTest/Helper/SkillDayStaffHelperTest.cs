using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Helper
{
    [TestFixture]
    public class SkillDayStaffHelperTest
    {
        private ITemplateTaskPeriod _taskPeriod;
        private SkillDataPeriod _saPeriod;
        private DateOnly _dt = new DateOnly(2008, 2, 1);

        [Test]
        public void VerifyCombineListPeriods()
        {
            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            taskPeriods.Add(CreatePeriod(1, 2));
            taskPeriods.Add(CreatePeriod(2, 3));
            taskPeriods.Add(CreatePeriod(2, 3));
            taskPeriods.Add(CreatePeriod(4, 6));
            taskPeriods.Add(CreatePeriod(5, 7));
            taskPeriods.Add(CreatePeriod(6, 8));
            taskPeriods.Add(CreatePeriod(8, 9));
            taskPeriods.Add(CreatePeriod(8, 10));
            taskPeriods.Add(CreatePeriod(10, 12));
            taskPeriods.Add(CreatePeriod(11, 14));
            taskPeriods.Add(CreatePeriod(13, 17));
            taskPeriods.Add(CreatePeriod(14, 15));
            taskPeriods.Add(CreatePeriod(17, 18));
            var outList = SkillDayStaffHelper.CombineList(taskPeriods);

            Assert.AreEqual(15, outList.Count());
            Assert.AreEqual(CreatePeriod(15, 17).Period, outList.ElementAt(13).Period);
            Assert.AreEqual(CreatePeriod(4, 5).Period, outList.ElementAt(2).Period);

        }

        [Test]
        public void VerifyCombineListValues()
        {
            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            taskPeriods.Add(CreatePeriod(1, 2));
            taskPeriods.Add(CreatePeriod1(2, 3));
            taskPeriods.Add(CreatePeriod(2, 3));
            taskPeriods.Add(CreatePeriod(4, 6));
            taskPeriods.Add(CreatePeriod1(5, 7));
            taskPeriods.Add(CreatePeriod(6, 8));
            taskPeriods.Add(CreatePeriod(8, 9));
            taskPeriods.Add(CreatePeriod(8, 10));
            taskPeriods.Add(CreatePeriod(10, 12));
            taskPeriods.Add(CreatePeriod(11, 14));
            taskPeriods.Add(CreatePeriod(13, 17));
            taskPeriods.Add(CreatePeriod(14, 15));
            taskPeriods.Add(CreatePeriod(17, 18));
            var outList = SkillDayStaffHelper.CombineList(taskPeriods);

            Assert.AreEqual(100, outList.ElementAt(0).Task.Tasks);
            Assert.AreEqual(120, outList.ElementAt(0).Task.AverageTaskTime.TotalSeconds);
            Assert.AreEqual(0d, outList.ElementAt(0).CampaignTasks.Value);
            Assert.AreEqual(0d, outList.ElementAt(0).CampaignTaskTime.Value);
            Assert.AreEqual(0d, outList.ElementAt(0).CampaignAfterTaskTime.Value);
            Assert.AreEqual(200, outList.ElementAt(1).Task.Tasks);
            Assert.AreEqual(180, outList.ElementAt(1).Task.AverageTaskTime.TotalSeconds);
            Assert.AreEqual(0d, outList.ElementAt(1).CampaignTasks.Value);
            Assert.AreEqual(0d, outList.ElementAt(1).CampaignTaskTime.Value);
            Assert.AreEqual(0d, outList.ElementAt(1).CampaignAfterTaskTime.Value);
            Assert.AreEqual(50, outList.ElementAt(2).Task.Tasks);
            Assert.AreEqual(20, outList.ElementAt(2).Task.AverageAfterTaskTime.TotalSeconds);
            Assert.AreEqual(0d, outList.ElementAt(2).CampaignTasks.Value);
            Assert.AreEqual(0d, outList.ElementAt(2).CampaignTaskTime.Value);
            Assert.AreEqual(0d, outList.ElementAt(2).CampaignAfterTaskTime.Value);
            Assert.AreEqual(100, outList.ElementAt(3).Task.Tasks);
            Assert.AreEqual(30, outList.ElementAt(3).Task.AverageAfterTaskTime.TotalSeconds);
            Assert.AreEqual(0d, outList.ElementAt(3).CampaignTasks.Value);
            Assert.AreEqual(0d, outList.ElementAt(3).CampaignTaskTime.Value);
            Assert.AreEqual(0d, outList.ElementAt(3).CampaignAfterTaskTime.Value);
            Assert.AreEqual(100, outList.ElementAt(4).Task.Tasks);
            Assert.AreEqual(30, outList.ElementAt(4).Task.AverageAfterTaskTime.TotalSeconds);
            Assert.AreEqual(0d, outList.ElementAt(4).CampaignTasks.Value);
            Assert.AreEqual(0d, outList.ElementAt(4).CampaignTaskTime.Value);
            Assert.AreEqual(0d, outList.ElementAt(4).CampaignAfterTaskTime.Value);
            Assert.AreEqual(50, outList.ElementAt(5).Task.Tasks);
            Assert.AreEqual(20, outList.ElementAt(5).Task.AverageAfterTaskTime.TotalSeconds);
            Assert.AreEqual(0d, outList.ElementAt(5).CampaignTasks.Value);
            Assert.AreEqual(0d, outList.ElementAt(5).CampaignTaskTime.Value);
            Assert.AreEqual(0d, outList.ElementAt(5).CampaignAfterTaskTime.Value);

            Assert.AreEqual(125, outList.ElementAt(12).Task.Tasks);
            Assert.AreEqual(120, outList.ElementAt(12).Task.AverageTaskTime.TotalSeconds);
            Assert.AreEqual(50, outList.ElementAt(13).Task.Tasks);
            Assert.AreEqual(120, outList.ElementAt(13).Task.AverageTaskTime.TotalSeconds);

            Assert.AreEqual(15d, outList.ElementAt(0).AggregatedTasks);
            Assert.AreEqual(15d, outList.ElementAt(1).AggregatedTasks);
            Assert.AreEqual(7.5d, outList.ElementAt(2).AggregatedTasks);
        }

		[Test]
		public void VerifyCombineCampaignValues()
		{
			IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
			taskPeriods.Add(CreatePeriod(1, 2));
			taskPeriods.Add(CreatePeriod(1, 2));

			taskPeriods[0].Tasks = 10;
			taskPeriods[0].CampaignTasks = new Percent(0.10);
			taskPeriods[0].AverageTaskTime = TimeSpan.FromSeconds(10);
			taskPeriods[0].AverageAfterTaskTime = TimeSpan.FromSeconds(10);
			taskPeriods[1].Tasks = 0;
			taskPeriods[1].AverageTaskTime = TimeSpan.Zero;
			taskPeriods[1].AverageAfterTaskTime = TimeSpan.Zero;
			
			var outList = SkillDayStaffHelper.CombineList(taskPeriods);

			Assert.AreEqual(10, outList.ElementAt(0).Task.Tasks);
			Assert.AreEqual(10, outList.ElementAt(0).Task.AverageTaskTime.TotalSeconds);
			Assert.AreEqual(10, outList.ElementAt(0).Task.AverageAfterTaskTime.TotalSeconds);
			Assert.AreEqual(0.1d, Math.Round(outList.ElementAt(0).CampaignTasks.Value,2));
			Assert.AreEqual(0d, outList.ElementAt(0).CampaignTaskTime.Value);
			Assert.AreEqual(0d, outList.ElementAt(0).CampaignAfterTaskTime.Value);
		}

        /// <summary>
        /// Verifies the combine list values with campaign information.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-06
        /// </remarks>
        [Test]
        public void VerifyCombineListValuesWithCampaignInformation()
        {
            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            taskPeriods.Add(CreatePeriod1(2, 3));
            taskPeriods.Add(CreatePeriod(2, 3));
            taskPeriods[0].CampaignTasks =new Percent(0.20d);
            taskPeriods[0].CampaignTaskTime = new Percent(0.40d);
            taskPeriods[0].CampaignAfterTaskTime = new Percent(0.50d);
            var outList = SkillDayStaffHelper.CombineList(taskPeriods);
            Assert.AreEqual(1, outList.Count());
            Assert.AreEqual(0.1d, Math.Round(outList.ElementAt(0).CampaignTasks.Value,2));
            Assert.AreEqual(Math.Round(0.32d,2), Math.Round(outList.ElementAt(0).CampaignTaskTime.Value,2));
            Assert.AreEqual(Math.Round(0.39d, 2), Math.Round(outList.ElementAt(0).CampaignAfterTaskTime.Value, 2));
        }

        [Test]
        public void VerifyCombineTaskPeriodsAndServiceLevelPeriods()
        {
            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            taskPeriods.Add(CreatePeriod(0, 3));
            taskPeriods.Add(CreatePeriod(3, 4));
            taskPeriods.Add(CreatePeriod(4, 7));
            taskPeriods.Add(CreatePeriod(8, 9));
            taskPeriods.Add(CreatePeriod(9, 13));

            IList<ISkillDataPeriod> saPeriods = new List<ISkillDataPeriod>();
            saPeriods.Add(CreateSaPeriod(0, 6));
            saPeriods.Add(CreateSaPeriod(6, 11));
            saPeriods.Add(CreateSaPeriod(12, 15));

            ISkill skill = SkillFactory.CreateSkill("Test");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            SkillDayCalculator skillDayCalculator = new SkillDayCalculator(skill,
                                   new List<ISkillDay>
                                       {
                                           new SkillDay(_dt, skill, scenario, new List<IWorkloadDay>(),
                                                        new List<ISkillDataPeriod>())
                                       }, new DateOnlyPeriod());
            skillDayCalculator.SkillDays.First().RecalculateDailyTasks();

            IList<ISkillStaffPeriod> outList = new List<ISkillStaffPeriod>(skillDayCalculator.SkillDays.First().CompleteSkillStaffPeriodCollection);
            SkillDayStaffHelper.CombineTaskPeriodsAndServiceLevelPeriods(taskPeriods, saPeriods, outList);

            Assert.AreEqual(44, skillDayCalculator.SkillDays.First().SkillStaffPeriodCollection.Length);
            Assert.AreEqual(2, outList[5].Payload.SkillPersonData.MinimumPersons);
            Assert.AreEqual(10, outList[5].Payload.SkillPersonData.MaximumPersons);
        }

        [Test]
        public void VerifyCombineTaskPeriodsAndServiceLevelPeriodsWhenServiceLevelPeriodIsMissingInPartsOfTheDay()
        {
            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            taskPeriods.Add(CreatePeriod(0, 3));
            taskPeriods.Add(CreatePeriod(3, 5));
            taskPeriods.Add(CreatePeriod(5, 7));
            taskPeriods.Add(CreatePeriod(8, 9));
            taskPeriods.Add(CreatePeriod(9, 13));

            IList<ISkillDataPeriod> saPeriods = new List<ISkillDataPeriod>();
            saPeriods.Add(CreateSaPeriod(4, 9));

            ISkill skill = SkillFactory.CreateSkill("Test");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            SkillDayCalculator skillDayCalculator = new SkillDayCalculator(skill,
                                   new List<ISkillDay>
                                       {
                                           new SkillDay(_dt, skill, scenario, new List<IWorkloadDay>(),
                                                        new List<ISkillDataPeriod>())
                                       }, new DateOnlyPeriod());
            skillDayCalculator.SkillDays.First().RecalculateDailyTasks();

            IList<ISkillStaffPeriod> outList = new List<ISkillStaffPeriod>(skillDayCalculator.SkillDays.First().CompleteSkillStaffPeriodCollection);
            SkillDayStaffHelper.CombineTaskPeriodsAndServiceLevelPeriods(taskPeriods, saPeriods, outList);

            Assert.AreEqual(16, skillDayCalculator.SkillDays.First().SkillStaffPeriodCollection.Length);
        }

        [Test]
        public void VerifyCombineSkillDataPeriodsWithLongTaskPeriods()
        {
            ITemplateTaskPeriod taskPeriod = CreatePeriod(1, 5);
            SkillDataPeriod skillData1 = CreateSaPeriod(1, 2);
            SkillDataPeriod skillData2 = CreateSaPeriod(2, 3);
            SkillDataPeriod skillData3 = CreateSaPeriod(3, 4);
            SkillDataPeriod skillData4 = CreateSaPeriod(4, 5);

            IList<ISkillDataPeriod> skillDataList = new List<ISkillDataPeriod> { skillData1, skillData2, skillData3, skillData4 };
            IList<ITemplateTaskPeriod> taskPeriodsList = new List<ITemplateTaskPeriod> { taskPeriod };

            ISkill skill = SkillFactory.CreateSkill("Test");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            SkillDayCalculator skillDayCalculator = new SkillDayCalculator(skill, new List<ISkillDay> { new SkillDay(_dt, skill, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>()) }, new DateOnlyPeriod());
            skillDayCalculator.SkillDays.First().RecalculateDailyTasks();

            IList<ISkillStaffPeriod> outList = new List<ISkillStaffPeriod>(skillDayCalculator.SkillDays.First().CompleteSkillStaffPeriodCollection);
            SkillDayStaffHelper.CombineTaskPeriodsAndServiceLevelPeriods(taskPeriodsList, skillDataList, outList);

            var result = skillDayCalculator.SkillDays.First().SkillStaffPeriodCollection;

            Assert.AreEqual(16, result.Count());
            Assert.AreEqual(7.1875d, result[0].Payload.TaskData.Tasks);
            Assert.AreEqual(7.1875d, result[1].Payload.TaskData.Tasks);
            Assert.AreEqual(7.1875d, result[14].Payload.TaskData.Tasks);
            Assert.AreEqual(7.1875d, result[15].Payload.TaskData.Tasks);
        }

        [Test]
        public void VerifyCombineSkillStaffPeriodAndMultisitePeriod()
        {
            ITask task = CreatePeriod(1, 2).Task;
            ServiceAgreement sa = CreateSaPeriod(1, 2).ServiceAgreement;
            DateTimePeriod period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dt.Date.Add(TimeSpan.FromHours(1)),
                    _dt.Date.Add(TimeSpan.FromHours(2)),TimeZoneInfoFactory.UtcTimeZoneInfo());

            ISkillStaffPeriod ssp1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period.ChangeEndTime(TimeSpan.FromHours(2)), task, sa);
            ISkillStaffPeriod ssp2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period.MovePeriod(TimeSpan.FromHours(3)), task, sa);
            ISkillStaffPeriod ssp3 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period.MovePeriod(TimeSpan.FromHours(4)), task, sa);
            ISkillStaffPeriod ssp4 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period.MovePeriod(TimeSpan.FromHours(5)), task, sa);
            ISkillStaffPeriod ssp5 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period.MovePeriod(TimeSpan.FromHours(6)), task, sa);

            IMultisiteSkill skill = SkillFactory.CreateMultisiteSkill("test");
            IChildSkill child1 = SkillFactory.CreateChildSkill("test1",skill);
            IChildSkill child2 = SkillFactory.CreateChildSkill("test2",skill);
            
            IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
            distribution.Add(child1,new Percent(0.4));
            distribution.Add(child2,new Percent(0.6));
            
            MultisitePeriod mp1 = new MultisitePeriod(period, distribution);
            MultisitePeriod mp2 = new MultisitePeriod(period.MovePeriod(TimeSpan.FromHours(1)),distribution);
            MultisitePeriod mp3 = new MultisitePeriod(period.MovePeriod(TimeSpan.FromHours(2)), distribution);
            MultisitePeriod mp4 = new MultisitePeriod(period.MovePeriod(TimeSpan.FromHours(3))
                .ChangeEndTime(TimeSpan.FromHours(3)), distribution);

            IList<IMultisitePeriod> mpList = new List<IMultisitePeriod> { mp1, mp2, mp3, mp4 };
            IList<ISkillStaffPeriod> sspList = new List<ISkillStaffPeriod> { ssp1, ssp2, ssp3, ssp4, ssp5 };

            IList<ISkillStaffPeriod> result = SkillDayStaffHelper.CombineSkillStaffPeriodsAndMultisitePeriods(
                sspList,
                mpList);

            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(33.33d, Math.Round(result[0].Payload.TaskData.Tasks,2));
            Assert.AreEqual(33.33d, Math.Round(result[1].Payload.TaskData.Tasks, 2));
            Assert.AreEqual(33.33d, Math.Round(result[2].Payload.TaskData.Tasks, 2));
            Assert.AreEqual(100, result[3].Payload.TaskData.Tasks);
            Assert.AreEqual(100, result[4].Payload.TaskData.Tasks);
            Assert.AreEqual(100, result[5].Payload.TaskData.Tasks);
            Assert.AreEqual(100, result[6].Payload.TaskData.Tasks);
        }

        [Test]
        public void VerifyCombineSkillStaffPeriodAndMultisitePeriodWithShrinkage()
        {
            ITask task = CreatePeriod(1, 2).Task;
            ServiceAgreement sa = CreateSaPeriod(1, 2).ServiceAgreement;
            DateTimePeriod period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dt.Date.Add(TimeSpan.FromHours(1)),
                    _dt.Date.Add(TimeSpan.FromHours(2)), TimeZoneInfoFactory.UtcTimeZoneInfo());

            ISkillStaffPeriod ssp1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, task, sa);
            ssp1.Payload.Shrinkage = new Percent(0.5);

            IMultisiteSkill skill = SkillFactory.CreateMultisiteSkill("test");
            IChildSkill child1 = SkillFactory.CreateChildSkill("test1", skill);
            IChildSkill child2 = SkillFactory.CreateChildSkill("test2", skill);

            IDictionary<IChildSkill, Percent> distribution = new Dictionary<IChildSkill, Percent>();
            distribution.Add(child1, new Percent(0.4));
            distribution.Add(child2, new Percent(0.6));

            MultisitePeriod mp1 = new MultisitePeriod(period, distribution);

            IList<IMultisitePeriod> mpList = new List<IMultisitePeriod> { mp1};
            IList<ISkillStaffPeriod> sspList = new List<ISkillStaffPeriod> { ssp1 };

            IList<ISkillStaffPeriod> result = SkillDayStaffHelper.CombineSkillStaffPeriodsAndMultisitePeriods(
                sspList,
                mpList);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0.5, result[0].Payload.Shrinkage.Value);
        }

        [Test]
        public void VerifyCombineTaskPeriodsAndServiceLevelPeriodsWithShrinkage()
        {
            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            taskPeriods.Add(CreatePeriod(0, 3));

            IList<ISkillDataPeriod> saPeriods = new List<ISkillDataPeriod>();
            saPeriods.Add(CreateSaPeriod(0, 3));
            saPeriods[0].Shrinkage = new Percent(0.3);

            ISkill skill = SkillFactory.CreateSkill("Test");
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            SkillDayCalculator skillDayCalculator = new SkillDayCalculator(skill,
                                                                           new List<ISkillDay>
                                                                               {
                                                                                   new SkillDay(_dt, skill, scenario,
                                                                                                new List<IWorkloadDay>(),
                                                                                                new List
                                                                                                    <ISkillDataPeriod>())
                                                                               },
                                                                           new DateOnlyPeriod());
            skillDayCalculator.SkillDays.First().RecalculateDailyTasks();

            IList<ISkillStaffPeriod> outList =
                new List<ISkillStaffPeriod>(skillDayCalculator.SkillDays.First().CompleteSkillStaffPeriodCollection);
            SkillDayStaffHelper.CombineTaskPeriodsAndServiceLevelPeriods(taskPeriods, saPeriods, outList);

            var result = skillDayCalculator.SkillDays.First().SkillStaffPeriodCollection;

            Assert.AreEqual(12, result.Length);
            Assert.AreEqual(new Percent(0.3), result[0].Payload.Shrinkage);
        }

        private ITemplateTaskPeriod CreatePeriod(int fromHours, int toHours)
        {
            _taskPeriod = new TemplateTaskPeriod(
                    new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20)),
                    TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dt.Date.Add(TimeSpan.FromHours(fromHours)),
                    _dt.Date.Add(TimeSpan.FromHours(toHours)),TimeZoneInfoFactory.UtcTimeZoneInfo()));
            _taskPeriod.AggregatedTasks = 15d;
            return _taskPeriod;
        }

        private ITemplateTaskPeriod CreatePeriod1(int fromHours, int toHours)
        {
            _taskPeriod = new TemplateTaskPeriod(
                    new Task(100, TimeSpan.FromSeconds(240), TimeSpan.FromSeconds(40)),
                    TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dt.Date.Add(TimeSpan.FromHours(fromHours)),
                    _dt.Date.Add(TimeSpan.FromHours(toHours)),TimeZoneInfoFactory.UtcTimeZoneInfo()));
            return _taskPeriod;
        }

        private SkillDataPeriod CreateSaPeriod(int fromHours, int toHours)
        {
            _saPeriod = new SkillDataPeriod(
                new ServiceAgreement(), 
                new SkillPersonData(),
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_dt.Date.Add(TimeSpan.FromHours(fromHours)),
                    _dt.Date.Add(TimeSpan.FromHours(toHours)),TimeZoneInfoFactory.UtcTimeZoneInfo()));
            _saPeriod.MaximumPersons = 10;
            _saPeriod.MinimumPersons = 2;
            return _saPeriod;
        }

    }
}
