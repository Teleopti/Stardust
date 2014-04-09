using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class SkillDayHelperTest
    {
        private ISkill _skill;
        private ITask _task;
        private MockRepository _mocks;
        private ISkillStaffPeriod _stPeriod1;
        private ISkillStaffPeriod _stPeriod2;
        private ISkillStaffPeriod _stPeriod3;
        private ISkillStaffPeriod _stPeriod4;
        private ISkillStaffPeriod _stPeriod5;
        private ISkillStaffPeriod _stPeriod6;
        private ISkillStaffPeriod _stPeriod7;

        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("testSkill");
            _task = new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20));
            _mocks = new MockRepository();
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

        [Test]
        public void VerifySkillDayGridRows()
        {
            List<ISkillStaffPeriod> periodlist = GetPeriodlist();
            using (_mocks.Playback())
            {
                CalculateStaff(periodlist);

                TimeSpan expected =
                    TimeSpan.FromHours((4 + 4 + 4 + 4 + 0 + 0 + 0) *
                                       new DateTimePeriod(2000, 1, 6, 2000, 1, 7).ElapsedTime().TotalHours);
                Assert.AreEqual(expected,
                                SkillStaffPeriodHelper.ForecastedIncoming(periodlist));


                Assert.AreEqual(1, _stPeriod1.ForecastedDistributedDemand);
                Assert.AreEqual(2, _stPeriod2.ForecastedDistributedDemand);
                Assert.AreEqual(3, _stPeriod3.ForecastedDistributedDemand);
                Assert.AreEqual(4, _stPeriod4.ForecastedDistributedDemand);
                Assert.AreEqual(3, _stPeriod5.ForecastedDistributedDemand);
                Assert.AreEqual(2, _stPeriod6.ForecastedDistributedDemand);
                Assert.AreEqual(1, _stPeriod7.ForecastedDistributedDemand);

                _stPeriod1.SetCalculatedResource65(0);
                _stPeriod2.SetCalculatedResource65(8);
                _stPeriod3.SetCalculatedResource65(0);
                _stPeriod4.SetCalculatedResource65(0);
                _stPeriod5.SetCalculatedResource65(0);
                _stPeriod6.SetCalculatedResource65(0);
                _stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, _stPeriod1.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(4, _stPeriod2.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, _stPeriod3.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, _stPeriod4.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, _stPeriod5.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, _stPeriod6.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, _stPeriod7.Payload.BookedAgainstIncomingDemand65);

                Assert.AreEqual(4, _stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(4, _stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod7.ScheduledAgentsIncoming);

                expected =
                    TimeSpan.FromHours((4 + 4 + 0 + 0 + 0 + 0 + 0) *
                                       new DateTimePeriod(2000, 1, 6, 2000, 1, 7).ElapsedTime().TotalHours);
                Assert.AreEqual(expected,
                                SkillStaffPeriodHelper.ScheduledIncoming(periodlist));


                Assert.AreEqual(0, _stPeriod1.IncomingDifference);
                Assert.AreEqual(0, _stPeriod2.IncomingDifference);
                Assert.AreEqual(-4, _stPeriod3.IncomingDifference);
                Assert.AreEqual(-4, _stPeriod4.IncomingDifference);
                Assert.AreEqual(0, _stPeriod5.IncomingDifference);
                Assert.AreEqual(0, _stPeriod6.IncomingDifference);
                Assert.AreEqual(0, _stPeriod7.IncomingDifference);

                expected =
                    TimeSpan.FromHours((0 + 0 + -4 + -4 + 0 + 0 + 0) *
                                       new DateTimePeriod(2000, 1, 6, 2000, 1, 7).ElapsedTime().TotalHours);
                Assert.AreEqual(expected,
                                SkillStaffPeriodHelper.AbsoluteDifferenceIncoming(periodlist));

                Assert.AreEqual(-8d/16d, SkillStaffPeriodHelper.RelativeDifferenceIncoming(periodlist));
                Assert.AreEqual(-0.5, SkillStaffPeriodHelper.RelativeDifference(periodlist));
                Assert.AreEqual(-0.5, SkillStaffPeriodHelper.RelativeDifferenceForDisplay(periodlist));

                Assert.AreEqual(33.94d, SkillStaffPeriodHelper.SkillDayRootMeanSquare(periodlist), 0.01);

                expected =
                   TimeSpan.FromHours((0 + 8 + 0 + 0 + 0 + 0 + 0) *
                                      new DateTimePeriod(2000, 1, 6, 2000, 1, 7).ElapsedTime().TotalHours);
                Assert.AreEqual(expected, SkillStaffPeriodHelper.ScheduledTime(periodlist));

               

                Assert.AreEqual(0, _stPeriod1.FStaff);
                Assert.AreEqual(8, _stPeriod2.FStaff);
                Assert.AreEqual(1, _stPeriod3.FStaff);
                Assert.AreEqual(2, _stPeriod4.FStaff);
                Assert.AreEqual(2, _stPeriod5.FStaff);
                Assert.AreEqual(2, _stPeriod6.FStaff);
                Assert.AreEqual(1, _stPeriod7.FStaff);

                expected =
                    TimeSpan.FromHours((0 + 8 + 1 + 2 + 2 + 2 + 1)*
                                       new DateTimePeriod(2000, 1, 6, 2000, 1, 7).ElapsedTime().TotalHours);
                Assert.AreEqual(expected,
                                SkillStaffPeriodHelper.ForecastedTime(periodlist));

                expected =
                    TimeSpan.FromHours((0 + 0 + -1 + -2 + -2 + -2 + -1) *
                                       new DateTimePeriod(2000, 1, 6, 2000, 1, 7).ElapsedTime().TotalHours);
                Assert.AreEqual(expected, SkillStaffPeriodHelper.AbsoluteDifference(periodlist, false, false));

                _stPeriod1.SetCalculatedResource65(0);
                _stPeriod2.SetCalculatedResource65(10);
                _stPeriod3.SetCalculatedResource65(0);
                _stPeriod4.SetCalculatedResource65(0);
                _stPeriod5.SetCalculatedResource65(0);
                _stPeriod6.SetCalculatedResource65(0);
                _stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, _stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, _stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod7.ScheduledAgentsIncoming);

                Assert.AreEqual(0, _stPeriod1.IncomingDifference);
                Assert.AreEqual(2, _stPeriod2.IncomingDifference);
                Assert.AreEqual(-4, _stPeriod3.IncomingDifference);
                Assert.AreEqual(-4, _stPeriod4.IncomingDifference);
                Assert.AreEqual(0, _stPeriod5.IncomingDifference);
                Assert.AreEqual(0, _stPeriod6.IncomingDifference);
                Assert.AreEqual(0, _stPeriod7.IncomingDifference);

                Assert.AreEqual(0, _stPeriod1.SortedSegmentCollection[0].FStaff());
                Assert.AreEqual(4, _stPeriod1.SortedSegmentCollection[1].FStaff());
                Assert.AreEqual(0, _stPeriod1.SortedSegmentCollection[2].FStaff());
                Assert.AreEqual(0, _stPeriod1.SortedSegmentCollection[3].FStaff());

                double devider = _stPeriod2.Payload.BookedAgainstIncomingDemand65;
                Assert.AreEqual(4, devider);

                devider = _stPeriod2.SortedSegmentCollection[0].BelongsTo.Payload.BookedAgainstIncomingDemand65;
                Assert.AreEqual(4, devider);

                Assert.AreEqual(4, _stPeriod2.SortedSegmentCollection[0].BookedResource65);

                Assert.AreEqual(4, _stPeriod2.SortedSegmentCollection[0].FStaff());
                Assert.AreEqual(0, _stPeriod2.SortedSegmentCollection[1].FStaff());
                Assert.AreEqual(0, _stPeriod2.SortedSegmentCollection[2].FStaff());
                Assert.AreEqual(0, _stPeriod2.SortedSegmentCollection[3].FStaff());

                Assert.AreEqual(0, _stPeriod1.FStaff);
                Assert.AreEqual(8, _stPeriod2.FStaff);
                Assert.AreEqual(1, _stPeriod3.FStaff);
                Assert.AreEqual(2, _stPeriod4.FStaff);
                Assert.AreEqual(2, _stPeriod5.FStaff);
                Assert.AreEqual(2, _stPeriod6.FStaff);
                Assert.AreEqual(1, _stPeriod7.FStaff);


                _stPeriod1.SetCalculatedResource65(0);
                _stPeriod2.SetCalculatedResource65(10);
                _stPeriod3.SetCalculatedResource65(0);
                _stPeriod4.SetCalculatedResource65(8);
                _stPeriod5.SetCalculatedResource65(0);
                _stPeriod6.SetCalculatedResource65(0);
                _stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, _stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, _stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(4, _stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(4, _stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod7.ScheduledAgentsIncoming);

                Assert.AreEqual(0, _stPeriod1.FStaff);
                Assert.AreEqual(8, _stPeriod2.FStaff);
                Assert.AreEqual(0, _stPeriod3.FStaff);
                Assert.AreEqual(8, _stPeriod4.FStaff);
                Assert.AreEqual(0, _stPeriod5.FStaff);
                Assert.AreEqual(0, _stPeriod6.FStaff);
                Assert.AreEqual(0, _stPeriod7.FStaff);

                _stPeriod1.SetCalculatedResource65(0);
                _stPeriod2.SetCalculatedResource65(10);
                _stPeriod3.SetCalculatedResource65(0);
                _stPeriod4.SetCalculatedResource65(8);
                _stPeriod5.SetCalculatedResource65(4);
                _stPeriod6.SetCalculatedResource65(0);
                _stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, _stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, _stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(4, _stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(4, _stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(4, _stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, _stPeriod7.ScheduledAgentsIncoming);

                Assert.AreEqual(0, _stPeriod1.FStaff);
                Assert.AreEqual(8, _stPeriod2.FStaff);
                Assert.AreEqual(0, _stPeriod3.FStaff);
                Assert.AreEqual(8, _stPeriod4.FStaff);
                Assert.AreEqual(0, _stPeriod5.FStaff);
                Assert.AreEqual(0, _stPeriod6.FStaff);
                Assert.AreEqual(0, _stPeriod7.FStaff);


            }
        }

        private static void CalculateStaff(List<ISkillStaffPeriod> list)
        {
            for (int index = list.Count - 1; index >= 0; index--)
            {
                list[index].CalculateStaff(list.GetRange(index, list.Count - index));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Domain.Forecasting.SkillDayCalculator")]
        private static IList<ISkillDay> GetBaseData(ISkill skill, out DateOnly startDateLocal)
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            IWorkload workload = WorkloadFactory.CreateWorkload(skill);

            startDateLocal = new DateOnly(2008, 1, 2);
            DateTime startDate = TimeZoneInfo.ConvertTimeToUtc(startDateLocal, skill.TimeZone);

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

        private List<ISkillStaffPeriod> GetPeriodlist()
        {
			var skillDay = SkillDayFactory.CreateSkillDay(_skill, DateTime.Now);

            IStaffingCalculatorService svc = _mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);

            _stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),new Percent(2)),
                                             svc);
            _stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),new Percent(2)),
                                             svc);
            _stPeriod3 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 3, 2000, 1, 4),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),new Percent(2)),
                                             svc);
            _stPeriod4 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 4, 2000, 1, 5),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),new Percent(2)),
                                             svc);

            _stPeriod5 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 5, 2000, 1, 6),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),new Percent(2)),
                                             svc);

            _stPeriod6 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 6, 2000, 1, 7),
                                             _task,
                                             ServiceAgreement.DefaultValues(), svc);

            _stPeriod7 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 7, 2000, 1, 8),
                                             _task,
                                             ServiceAgreement.DefaultValues(), svc);

			_stPeriod1.SetSkillDay(skillDay);
			_stPeriod2.SetSkillDay(skillDay);
			_stPeriod3.SetSkillDay(skillDay);
			_stPeriod4.SetSkillDay(skillDay);
			_stPeriod5.SetSkillDay(skillDay);
			_stPeriod6.SetSkillDay(skillDay);
			_stPeriod7.SetSkillDay(skillDay);

            List<ISkillStaffPeriod> periodlist = new List<ISkillStaffPeriod>
                                                     {
                                                         _stPeriod1,
                                                         _stPeriod2,
                                                         _stPeriod3,
                                                         _stPeriod4,
                                                         _stPeriod5,
                                                         _stPeriod6,
                                                         _stPeriod7
                                                     };

            using (_mocks.Record())
            {
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                    .Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                    .Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                    .Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                    .Return(4d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                    .Return(4d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                    .Return(4d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                    .Return(4d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(7);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }
            return periodlist;
        }

        [Test]
        public void VerifySomeFunctions()
        {
            IList<ISkillStaffPeriod> periods = new List<ISkillStaffPeriod>();
            double? res = SkillStaffPeriodHelper.SkillDayGridSmoothness(periods);
            Assert.IsNull(res);
            periods = GetPeriodlist();
            res = SkillStaffPeriodHelper.SkillDayGridSmoothness(periods);    
            Assert.IsNotNull(res);
            Assert.AreEqual(0, res);
            res = SkillStaffPeriodHelper.SkillDayRootMeanSquare(periods);
            Assert.IsNotNull(res);
            Assert.AreEqual(0, res);
            res = SkillStaffPeriodHelper.GetHighestIntraIntervalDeviation(periods);
            Assert.AreEqual(0, res);
            TimeSpan span = new TimeSpan(0,0,0);
            TimeSpan? resSpan = SkillStaffPeriodHelper.AbsoluteDifference(periods, false, false);
            Assert.AreEqual(span, resSpan);
            periods.Clear();
            Assert.IsNull(SkillStaffPeriodHelper.AbsoluteDifference(periods, false, false));
            Assert.IsNull(SkillStaffPeriodHelper.AbsoluteDifferenceIncoming(periods));
            Assert.IsNull(SkillStaffPeriodHelper.RelativeDifferenceIncoming(periods));
            Assert.IsNotNull(SkillStaffPeriodHelper.EstimatedServiceLevel(periods));
        }
    }
}
