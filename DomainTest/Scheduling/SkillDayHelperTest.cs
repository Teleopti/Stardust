using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class SkillDayHelperTest
    {
        private ISkill _skill;

        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("testSkill");
        }

        [Test]
        public void VerifyScheduledHoursAndScheduledTime()
        {
            _skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateOnly startDateLocal;
            IList<ISkillDay> skillDays = GetBaseData(_skill, out startDateLocal);

            foreach (SkillStaffPeriod dataPeriod in skillDays[0].SkillStaffPeriodCollection)
            {
                SkillStaffPeriodFactory.InjectCalculatedResource(dataPeriod, 2d);
            }

            double value = SkillStaffPeriodHelper.ScheduledHours(skillDays[0].SkillStaffPeriodCollection).Value;
            Assert.AreEqual(48, value);

            value = SkillStaffPeriodHelper.ScheduledTime(skillDays[0].SkillStaffPeriodCollection).Value.TotalHours;
            Assert.AreEqual(48, value);
        }

        [Test]
        public void VerifyEstimatedServiceLevel()
        {
            _skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateOnly startDateLocal;
            IList<ISkillDay> baseData = GetBaseData(_skill, out startDateLocal);

            List<ISkillStaffPeriod> periodlist1 = new List<ISkillStaffPeriod>(baseData[0].SkillStaffPeriodCollection);
            List<ISkillStaffPeriod> periodlist2 = new List<ISkillStaffPeriod>(baseData[1].SkillStaffPeriodCollection);

            Assert.AreEqual(2.435d, periodlist1[0].Payload.ForecastedIncomingDemand, 0.01);
            Assert.AreEqual(3.764d, periodlist2[0].Payload.ForecastedIncomingDemand, 0.01);

            SkillStaffPeriodFactory.InjectEstimatedServiceLevel((SkillStaffPeriod)periodlist1[0], new Percent(0.9));
            SkillStaffPeriodFactory.InjectEstimatedServiceLevel((SkillStaffPeriod)periodlist2[0], new Percent(0.4));

            Assert.AreEqual(0.9, periodlist1[0].EstimatedServiceLevel.Value);
            Assert.AreEqual(0.4, periodlist2[0].EstimatedServiceLevel.Value);

            Percent exp = new Percent(((0.9*5) + (0.4*10))/15);
            Assert.AreEqual(exp, SkillStaffPeriodHelper.EstimatedServiceLevel(new List<ISkillStaffPeriod> { periodlist1[0], periodlist2[0] }));

        }

  

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Domain.Forecasting.SkillDayCalculator")]
        private static IList<ISkillDay> GetBaseData(ISkill skill, out DateOnly startDateLocal)
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            IWorkload workload = WorkloadFactory.CreateWorkload(skill);

            startDateLocal = new DateOnly(2008, 1, 2);
            DateTime startDate = TimeZoneInfo.ConvertTimeToUtc(startDateLocal.Date, skill.TimeZone);

            SkillPersonData skillPersonData = new SkillPersonData(1, 99);
            TimeSpan interval = TimeSpan.FromMinutes(15);

            SkillDataPeriod skillDataPeriod;
            IList<ISkillDataPeriod> skillDataPeriodsOne = new List<ISkillDataPeriod>();
            for (DateTime t = startDate; t < startDate.AddDays(1); t = t.Add(interval))
            {
                skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), skillPersonData, new DateTimePeriod(t, t.Add(interval)));
                skillDataPeriodsOne.Add(skillDataPeriod);
            }

            IList<ISkillDataPeriod> skillDataPeriodsTwo = new List<ISkillDataPeriod>();
            for (DateTime t = startDate.AddDays(1); t < startDate.AddDays(2); t = t.Add(interval))
            {
                skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), skillPersonData, new DateTimePeriod(t, t.Add(interval)));
                skillDataPeriodsTwo.Add(skillDataPeriod);
            }

            IList<TimePeriod> openHourList = new List<TimePeriod>();
            openHourList.Add(new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(24)));

            WorkloadDay workloadDayOne = new WorkloadDay();
            workloadDayOne.Create(startDateLocal, workload, openHourList);
            workloadDayOne.Tasks = 480d;
            workloadDayOne.AverageTaskTime = TimeSpan.FromMinutes(3);

            WorkloadDay workloadDayTwo = new WorkloadDay();
            workloadDayTwo.Create(startDateLocal.AddDays(1), workload, openHourList);
            workloadDayTwo.Tasks = 960d;
            workloadDayTwo.AverageTaskTime = TimeSpan.FromMinutes(3);

            ISkillDay skillDayOne = new SkillDay(startDateLocal, skill, scenario, new List<IWorkloadDay> { workloadDayOne }, skillDataPeriodsOne);
            ISkillDay skillDayTwo = new SkillDay(startDateLocal.AddDays(1), skill, scenario, new List<IWorkloadDay> { workloadDayTwo }, skillDataPeriodsTwo);
            new SkillDayCalculator(skill, new List<ISkillDay> { skillDayOne, skillDayTwo }, new DateOnlyPeriod(startDateLocal, startDateLocal.AddDays(3)));
            skillDayOne.RecalculateDailyTasks();
            skillDayTwo.RecalculateDailyTasks();

            return new List<ISkillDay>{ skillDayOne, skillDayTwo };
        }
    }
}
