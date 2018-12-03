using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillDayCalculatorTest
    {
        private SkillDayCalculator target;
        private ISkill skill;
        private IList<ISkillDay> skillDays;
        private DateOnlyPeriod _visiblePeriod;

        [SetUp]
        public void Setup()
        {
            skill = SkillFactory.CreateSkill("My skill");
            skillDays = new List<ISkillDay>
                            {
                                SkillDayFactory.CreateSkillDay(skill, SkillDayTemplate.BaseDate),
                                SkillDayFactory.CreateSkillDay(skill, SkillDayTemplate.BaseDate.AddDays(1)),
                                SkillDayFactory.CreateSkillDay(skill, SkillDayTemplate.BaseDate.AddDays(2))
                            };
            _visiblePeriod = new DateOnlyPeriod(skillDays[0].CurrentDate, skillDays[2].CurrentDate);

            target = new SkillDayCalculator(skill,skillDays,_visiblePeriod);
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(3,target.SkillDays.Count());
            Assert.AreEqual(skill,target.Skill);
        }

        [Test]
        public void VerifyCanCollectTasksFromLastOpen()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(), new List<TimePeriod>());
            workloadDay1.TaskPeriodList[0].Tasks = 12;

            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            workloadDay2.TaskPeriodList[8].Tasks = 13;

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2 }, new List<ISkillDataPeriod>());

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2 }, _visiblePeriod);
            var templateTaskPeriods = target.CalculateTaskPeriods(newSkillDay2,true);

            Assert.AreEqual(9,templateTaskPeriods.Count());
            Assert.AreEqual(13,templateTaskPeriods.First().TotalTasks);
            Assert.AreEqual(12, templateTaskPeriods.First().AggregatedTasks);
        }

        [Test]
        public void VerifyCanCollectTasksFromLastClosed()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> {new TimePeriod(8, 0, 17, 0)});
            workloadDay1.TaskPeriodList[0].Tasks = 12;
            workloadDay1.TaskPeriodList[19].Tasks = 7;

            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            workloadDay2.TaskPeriodList[8].Tasks = 13;

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2 }, new List<ISkillDataPeriod>());

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2 }, _visiblePeriod);
            var templateTaskPeriods = target.CalculateTaskPeriods(newSkillDay2,true);

            Assert.AreEqual(9, templateTaskPeriods.Count());
            Assert.AreEqual(13, templateTaskPeriods.First().TotalTasks);
            Assert.AreEqual(7,templateTaskPeriods.First().AggregatedTasks);
        }

        [Test]
        public void VerifyCanCollectTasksFromLastOpenWithTwoWorkloads()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(), new List<TimePeriod>());
            workloadDay1.TaskPeriodList[0].Tasks = 12;

            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            workloadDay2.TaskPeriodList[8].Tasks = 13;


            WorkloadDay workloadDay3 = new WorkloadDay();
            workloadDay3.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.ElementAt(1), new List<TimePeriod>());
            workloadDay3.TaskPeriodList[0].Tasks = 22;

            WorkloadDay workloadDay4 = new WorkloadDay();
            workloadDay4.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.ElementAt(1),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            workloadDay4.TaskPeriodList[8].Tasks = 13;

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1, workloadDay3 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2, workloadDay4 }, new List<ISkillDataPeriod>());

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2 }, _visiblePeriod);
            var templateTaskPeriods = target.CalculateTaskPeriods(newSkillDay2,true);

            Assert.AreEqual(9, templateTaskPeriods.Count());
            Assert.AreEqual(26, templateTaskPeriods.First().TotalTasks);
            Assert.AreEqual(34, templateTaskPeriods.First().AggregatedTasks);
        }

        [Test]
        public void VerifyOnlyAddsUpUntilNewOpenInterval()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> {new TimePeriod(8, 0, 17, 0)});
            workloadDay1.TaskPeriodList[0].Tasks = 12;
            workloadDay1.TaskPeriodList[19].Tasks = 7;

            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            workloadDay2.TaskPeriodList[0].Tasks = 10;
            workloadDay2.TaskPeriodList[18].Tasks = 13;

            WorkloadDay workloadDay3 = new WorkloadDay();
            workloadDay3.Create(skillDays[2].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            workloadDay3.TaskPeriodList[8].Tasks = 13;

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay3 = new SkillDay(skillDays[2].CurrentDate, skill, skillDays[2].Scenario, new List<IWorkloadDay> { workloadDay3 }, new List<ISkillDataPeriod>());

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2, newSkillDay3 }, _visiblePeriod);
            var templateTaskPeriods = target.CalculateTaskPeriods(newSkillDay3,true);

            Assert.AreEqual(9, templateTaskPeriods.Count());
            Assert.AreEqual(13, templateTaskPeriods.First().TotalTasks);
            Assert.AreEqual(13, templateTaskPeriods.First().AggregatedTasks);
        }

        [Test]
        public void VerifyDisableSpilloverWorks()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });
            workloadDay1.TaskPeriodList[0].Tasks = 12;
            workloadDay1.TaskPeriodList[19].Tasks = 7;

            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            workloadDay2.TaskPeriodList[0].Tasks = 10;
            workloadDay2.TaskPeriodList[18].Tasks = 13;

            WorkloadDay workloadDay3 = new WorkloadDay();
            workloadDay3.Create(skillDays[2].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay3 = new SkillDay(skillDays[2].CurrentDate, skill, skillDays[2].Scenario, new List<IWorkloadDay> { workloadDay3 }, new List<ISkillDataPeriod>());

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2, newSkillDay3 }, _visiblePeriod);
            var templateTaskPeriods = target.CalculateTaskPeriods(newSkillDay2, false);

            Assert.AreEqual(9,templateTaskPeriods.Count());
            Assert.AreEqual(9, newSkillDay3.SkillStaffPeriodCollection.Length);
            Assert.AreEqual(0, newSkillDay3.SkillStaffPeriodCollection[0].ForecastedDistributedDemand);
        }

        [Test]
        public void VerifyCanGetTaskPeriodsWithInboundTelephonySkill()
        {
            skill.SkillType = new SkillTypePhone(new Description("Phone"), ForecastSource.InboundTelephony);
            skill.DefaultResolution = skill.SkillType.DefaultResolution; //15

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> {new TimePeriod(8, 0, 17, 0)});

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod>());
            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1 }, _visiblePeriod);
            var result = target.CalculateTaskPeriods(newSkillDay1,true);

            Assert.AreEqual(36,result.Count());
            Assert.IsTrue(target.IsCalculatedWithinDay);
        }

        [Test]
        public void VerifyCanSetTasksOnWorkloadDayLevel()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });
            workloadDay1.MergeTemplateTaskPeriods(workloadDay1.OpenTaskPeriodList);

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod> { new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(0, 10), ((IPeriodized)skillDays[0]).Period) });
            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1 }, _visiblePeriod);

            workloadDay1.Tasks = 500d;

            Assert.AreEqual(9, newSkillDay1.SkillStaffPeriodCollection.Length);
            Assert.AreEqual(281.25d, Math.Round(newSkillDay1.TotalTasks,2));

            workloadDay1.Tasks = 1000d;

            Assert.AreEqual(9, newSkillDay1.SkillStaffPeriodCollection.Length);
            Assert.AreEqual(562.5d, Math.Round(newSkillDay1.TotalTasks,2));
        }

        [Test]
        public void VerifyCanFindNextOpenDay()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });

            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>());

            WorkloadDay workloadDay3 = new WorkloadDay();
            workloadDay3.Create(skillDays[2].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay3 = new SkillDay(skillDays[2].CurrentDate, skill, skillDays[2].Scenario, new List<IWorkloadDay> { workloadDay3 }, new List<ISkillDataPeriod>());

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2, newSkillDay3 }, _visiblePeriod);

            var result = target.FindNextOpenDay(skill.WorkloadCollection.First(), workloadDay1.CurrentDate);
            Assert.AreEqual(newSkillDay3,result);
        }

        [Test]
        public void VerifyCannotFindNextOpenDayWhenThereAreNone()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });

            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>());

            WorkloadDay workloadDay3 = new WorkloadDay();
            workloadDay3.Create(skillDays[2].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario, new List<IWorkloadDay> { workloadDay1 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2 }, new List<ISkillDataPeriod>());
            SkillDay newSkillDay3 = new SkillDay(skillDays[2].CurrentDate, skill, skillDays[2].Scenario, new List<IWorkloadDay> { workloadDay3 }, new List<ISkillDataPeriod>());

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2, newSkillDay3 }, _visiblePeriod);

            var result = target.FindNextOpenDay(skill.WorkloadCollection.First(), workloadDay3.CurrentDate);
            Assert.IsNull(result);
        }

        [Test]
        public void VerifyVisiblePeriod()
        {
            Assert.AreEqual(_visiblePeriod,target.VisiblePeriod);
            DateOnlyPeriod visiblePeriod = new DateOnlyPeriod(skillDays[0].CurrentDate, skillDays[0].CurrentDate.AddDays(1));
            target.VisiblePeriod = visiblePeriod;
            Assert.AreEqual(visiblePeriod,target.VisiblePeriod);
        }

        [Test]
        public void VerifyVisibleSkillDaysWorks()
        {
            Assert.AreEqual(3,target.VisibleSkillDays.Count());
            DateOnlyPeriod visiblePeriod = new DateOnlyPeriod(skillDays[0].CurrentDate, skillDays[0].CurrentDate);
            target.VisiblePeriod = visiblePeriod;
            Assert.AreEqual(1,target.VisibleSkillDays.Count());
            Assert.AreEqual(skillDays[0],target.VisibleSkillDays.First());
        }

        [Test]
        public void VerifyCanGetPeriodToLoad()
        {
            DateOnlyPeriod periodToLoad = SkillDayCalculator.GetPeriodToLoad(_visiblePeriod);
            
            DateOnly expectedStartToLoad = _visiblePeriod.StartDate.AddDays(-7);
            DateOnly expectedEndToLoad = _visiblePeriod.EndDate.AddDays(1);
            Assert.AreEqual(expectedStartToLoad, periodToLoad.StartDate);
            Assert.AreEqual(expectedEndToLoad,periodToLoad.EndDate);
        }

        [Test]
        public void VerifyCheckRestrictions()
        {
            skillDays[1].SkillDataPeriodCollection[0].MinimumPersons = 5;
            skillDays[1].SkillDataPeriodCollection[0].MaximumPersons = 4;

			Assert.Throws<ValidationException>(() => target.CheckRestrictions());
        }

        [Test]
        public void VerifyCanGetUpcomingSkillStaffPeriods()
        {
            Assert.AreEqual(0,target.SkillStaffPeriodCount);
            skillDays[0].RecalculateDailyTasks();
            skillDays[1].RecalculateDailyTasks();
            skillDays[2].RecalculateDailyTasks();
			skillDays[1].SkillStaffPeriodCollection[0].Payload.ServiceAgreementData = skillDays[1].SkillStaffPeriodCollection[0].Payload.ServiceAgreementData.WithServiceLevel(new ServiceLevel(new Percent(0.8), 16*60));
            var result = target.GetSkillStaffPeriodsForDayCalculation(skillDays[1]);
            Assert.AreEqual(3*96,target.SkillStaffPeriodCount);
            Assert.AreEqual(120,result.Count()); //Skill staff periods for two days
        }

        [Test]
        public void VerifyCanResetSkillStaffPeriods()
        {
            Assert.AreEqual(0, target.SkillStaffPeriodCount);
            skillDays[0].RecalculateDailyTasks();
            skillDays[1].RecalculateDailyTasks();
            skillDays[2].RecalculateDailyTasks();
			skillDays[1].SkillStaffPeriodCollection[0].Payload.ServiceAgreementData = skillDays[1].SkillStaffPeriodCollection[0].Payload.ServiceAgreementData.WithServiceLevel(new ServiceLevel(new Percent(0.8), 16 * 60));
			skillDays[2].SkillStaffPeriodCollection[0].Payload.ServiceAgreementData = skillDays[2].SkillStaffPeriodCollection[0].Payload.ServiceAgreementData.WithServiceLevel(new ServiceLevel(new Percent(0.8), 16 * 60));
			
            var result = target.GetSkillStaffPeriodsForDayCalculation(skillDays[1]);
            Assert.AreEqual(3 * 96, target.SkillStaffPeriodCount);
            Assert.AreEqual(120, result.Count()); 
            target.ClearSkillStaffPeriods();
            Assert.AreEqual(0,target.SkillStaffPeriodCount);
            result = target.GetSkillStaffPeriodsForDayCalculation(skillDays[2]);
            Assert.AreEqual(60, result.Count()); 
            Assert.AreEqual(3 * 96, target.SkillStaffPeriodCount);
        }

        [Test]
        public void VerifyCanDoCompleteSkillStaffCalculation()
        {
            SetupEmailWorkload();

            WorkloadDay workloadDay1 = new WorkloadDay();
            workloadDay1.Create(skillDays[0].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod> { new TimePeriod(8, 0, 17, 0) });
            
            WorkloadDay workloadDay2 = new WorkloadDay();
            workloadDay2.Create(skillDays[1].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });
            
            WorkloadDay workloadDay3 = new WorkloadDay();
            workloadDay3.Create(skillDays[2].CurrentDate, skill.WorkloadCollection.First(),
                                new List<TimePeriod>
                                    {
                                        new TimePeriod(8, 0, 17, 0)
                                    });

            SkillDay newSkillDay1 = new SkillDay(skillDays[0].CurrentDate, skill, skillDays[0].Scenario,
                                                 new List<IWorkloadDay> {workloadDay1},
                                                 new List<ISkillDataPeriod>
                                                     {
                                                         new SkillDataPeriod(
                                                             new ServiceAgreement(new ServiceLevel(new Percent(1), 7200),
                                                                                  new Percent(0), new Percent(1)),
                                                             new SkillPersonData(),
                                                             ((IPeriodized)skillDays[0]).Period)
                                                     });
            SkillDay newSkillDay2 = new SkillDay(skillDays[1].CurrentDate, skill, skillDays[1].Scenario, new List<IWorkloadDay> { workloadDay2 }, new List<ISkillDataPeriod>
                                                     {
                                                         new SkillDataPeriod(
                                                             new ServiceAgreement(new ServiceLevel(new Percent(1), 7200),
                                                                                  new Percent(0), new Percent(1)),
                                                             new SkillPersonData(),
                                                             ((IPeriodized)skillDays[1]).Period)
                                                     });
            SkillDay newSkillDay3 = new SkillDay(skillDays[2].CurrentDate, skill, skillDays[2].Scenario, new List<IWorkloadDay> { workloadDay3 }, new List<ISkillDataPeriod>
                                                     {
                                                         new SkillDataPeriod(
                                                             new ServiceAgreement(new ServiceLevel(new Percent(1), 7200),
                                                                                  new Percent(0), new Percent(1)),
                                                             new SkillPersonData(),
                                                             ((IPeriodized)skillDays[2]).Period)
                                                     });

            target = new SkillDayCalculator(skill, new List<ISkillDay> { newSkillDay1, newSkillDay2, newSkillDay3 }, _visiblePeriod);

            workloadDay1.TaskPeriodList[0].Tasks = 12;
            workloadDay1.TaskPeriodList[0].AverageTaskTime = TimeSpan.FromSeconds(12);
            workloadDay1.TaskPeriodList[19].Tasks = 7;
            workloadDay1.TaskPeriodList[19].AverageTaskTime = TimeSpan.FromSeconds(7);
            workloadDay2.TaskPeriodList[0].Tasks = 10;
            workloadDay2.TaskPeriodList[0].AverageTaskTime = TimeSpan.FromSeconds(10);
            workloadDay2.TaskPeriodList[16].Tasks = 13;
            workloadDay2.TaskPeriodList[16].AverageTaskTime = TimeSpan.FromSeconds(13);
            workloadDay3.TaskPeriodList[8].Tasks = 13;
            workloadDay3.TaskPeriodList[8].AverageTaskTime = TimeSpan.FromSeconds(13);

            Assert.AreEqual(2, newSkillDay3.SkillStaffPeriodCollection[0].SegmentInThisCollection.Count);
            
            target.DistributeStaff();
            
            Assert.AreEqual(2,newSkillDay3.SkillStaffPeriodCollection[0].SegmentInThisCollection.Count);
            Assert.AreEqual(2, newSkillDay3.SkillStaffPeriodCollection[0].SortedSegmentCollection.Count);
        }

        [Test]
        public void VerifyGetPercentageForIntervalReturnsHundredPercentForNonMultisiteSkills()
        {
            Percent percentage = target.GetPercentageForInterval(skill, new DateTimePeriod());
            Assert.AreEqual(1,percentage.Value);
        }

        [Test]
        public void VerifyCloneToScenario()
        {
            MockRepository mocks = new MockRepository();
            IScenario scenario = mocks.StrictMock<IScenario>();

            mocks.ReplayAll();
            var skillDayCalculator = target.CloneToScenario(scenario);
            Assert.AreNotEqual(skillDayCalculator.SkillDays.First(),target.SkillDays.First());
            Assert.AreEqual(skillDayCalculator.VisiblePeriod,target.VisiblePeriod);
            Assert.AreEqual(scenario,skillDayCalculator.SkillDays.First().Scenario);
            mocks.VerifyAll();
        }

        private void SetupEmailWorkload()
        {
            skill.SkillType = new SkillTypeEmail(new Description("Email"), ForecastSource.Email);
            skill.DefaultResolution = skill.SkillType.DefaultResolution;
        }
    }
}
