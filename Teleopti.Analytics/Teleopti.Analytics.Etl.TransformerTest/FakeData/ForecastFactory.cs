using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public static class ForecastFactory
    {
        public static WorkloadDay CreateWorkload(DateOnly dt, ISkill skill)
        {
            var workload = new Workload(skill);
            ((IEntity)workload).SetId(Guid.NewGuid());

            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod(new TimeSpan(0), new TimeSpan(1, 0, 0, 0)));

            var workloadDay = new WorkloadDay();
            ((IEntity)workloadDay).SetId(Guid.NewGuid());
            workloadDay.Create(dt, workload, openHours);

            return workloadDay;
        }

        public static IList<ISkillDay> CreateSkillDayCollection(DateOnlyPeriod period, ISkill skill)
        {
            IList<ISkillDay> skillDayCollection = new List<ISkillDay>();
            for (var dt = period.StartDate; dt <= period.EndDate; dt = dt.AddDays(1))
            {
                skillDayCollection.Add(CreateSkillDay(dt, skill));
            }

            var skillDayCalculator = new SkillDayCalculator(skill, skillDayCollection, period);

            foreach (ISkillDay skillDay in skillDayCollection)
            {
                PrepareSkillDay(skillDay, period);
            }

            return skillDayCalculator.SkillDays.ToList();
        }

        private static SkillDay CreateSkillDay(DateOnly dt, ISkill skill)
        {
            IScenario scenario = new Scenario("Scenario 1");
            scenario.SetId(Guid.NewGuid());

            //IList<SkillDataPeriod> skillDataPeriods = new List<SkillDataPeriod>();
            
            //skillDataPeriods.Add(
            //    new SkillDataPeriod(
            //        serviceAgreement,
            //        new SkillPersonData(),
            //        new DateTimePeriod(dt.Add(TimeSpan.FromHours(4)), dt.Add(TimeSpan.FromHours(19)))));

            SkillDay skillDay = new SkillDay(dt, skill, scenario,
                                             WorkloadDayFactory.GetWorkloadDaysForTest(dt, skill),
                                             new List<ISkillDataPeriod>());

            return skillDay;
        }

        private static void PrepareSkillDay(ISkillDay skillDay, DateOnlyPeriod period)
        {
            ServiceAgreement serviceAgreement = new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                        new Percent(0.5),
                        new Percent(0.7));

            //DateTimePeriod period = new DateTimePeriod(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(1900, 1, 1, 0, 30, 0, DateTimeKind.Utc));
            IList<ISkillStaffPeriod> skillStaffPeriods = new List<ISkillStaffPeriod>();
            skillStaffPeriods.Add(SkillStaffPeriodFactory.CreateSkillStaffPeriod(period.ToDateTimePeriod(skillDay.Skill.TimeZone),
                                                                                 new Task(100d,
                                                                                          new TimeSpan
                                                                                              (0, 0,
                                                                                               30),
                                                                                          new TimeSpan
                                                                                              (0, 0,
                                                                                               10)),
                                                                                 serviceAgreement));
            var newSkillStaffPeriodValues = new NewSkillStaffPeriodValues(skillStaffPeriods);
            skillDay.SetCalculatedStaffCollection(newSkillStaffPeriodValues);
            newSkillStaffPeriodValues.BatchCompleted();
        }
    }
}