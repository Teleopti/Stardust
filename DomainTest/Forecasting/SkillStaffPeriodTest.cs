using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillStaffPeriodTest
    {
        private SkillStaffPeriod _target;
        private DateTimePeriod _tp;
        private ITask _task;
		private readonly IStaffingCalculatorService _staffingCalculatorService = new Domain.Calculation.StaffingCalculatorService();
        private ServiceAgreement _sa;
        private DateTime _dt = new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        private MockRepository mocks;
        private IAggregateSkillStaffPeriod _aggregateSkillStaffPeriod;
        private ISkillStaffPeriod stPeriod1;
        private ISkillStaffPeriod stPeriod2;
        private ISkillStaffPeriod stPeriod3;
        private ISkillStaffPeriod stPeriod4;
        private ISkillStaffPeriod stPeriod5;
        private ISkillStaffPeriod stPeriod6;
        private ISkillStaffPeriod stPeriod7;
        private IPopulationStatisticsCalculatedValues _populationStatisticsCalculatedValues;
	    private ISkill _skill;
	    private ISkillDay _skillDay;

	    [SetUp]
        public void Setup()
        {
            _tp = new DateTimePeriod(_dt.Add(TimeSpan.FromHours(10)), _dt.Add(TimeSpan.FromHours(11)));
            _task = new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20));
            _sa = new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20), new Percent(0), new Percent(1));
            _target = new SkillStaffPeriod(_tp, _task, _sa,_staffingCalculatorService);
			
            _target.IsAvailable = true;
            _target.SetCalculatedResource65(123);
            _target.Payload.CalculatedLoggedOn = 321;
            mocks = new MockRepository();
            _aggregateSkillStaffPeriod = _target;
            _populationStatisticsCalculatedValues = new PopulationStatisticsCalculatedValues(0,0);
		    
			_skill = SkillFactory.CreateSkill("name", SkillTypeFactory.CreateSkillType(), 15);
		    _skillDay = SkillDayFactory.CreateSkillDay(_skill, DateTime.Now);
			_target.SetSkillDay(_skillDay);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_tp, _target.Period);
            Assert.AreEqual(_task, _target.Payload.TaskData);
            Assert.AreEqual(_sa, _target.Payload.ServiceAgreementData);
            Assert.AreEqual(0, _target.Payload.ForecastedIncomingDemand);
            Assert.AreEqual(0, _target.ForecastedIncomingDemand().TotalMinutes);
            Assert.AreEqual(0, _target.ForecastedIncomingDemandWithShrinkage().TotalMinutes);
            Assert.AreEqual(123, _target.CalculatedResource);
            Assert.AreEqual(123*_tp.ElapsedTime().TotalHours, _target.ScheduledHours());
            Assert.AreEqual(321, _target.CalculatedLoggedOn);
            Assert.AreEqual(0, _target.SortedSegmentCollection.Count);
            Assert.IsNull(_target.StatisticTask);
            Assert.IsNull(_target.ActiveAgentCount);
            Assert.IsTrue(_target.IsAvailable);
            Assert.IsNotNull(_target.ToString());
            Assert.IsNotNull(_target.EstimatedServiceLevel);
            Assert.IsNotNull(_target.ActualServiceLevel);

            Assert.AreEqual(MinMaxStaffBroken.Ok, _target.AggregatedMinMaxStaffAlarm);
            _target.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, _target.AggregatedMinMaxStaffAlarm);

        }

        /// <summary>
        /// When set the period distribution, the IntraInterval statistical data must be filled up too.
        /// </summary>
        [Test]
        public void VerifySetPeriodDistribution()
        {
            MockRepository mock = new MockRepository();
            IPeriodDistribution periodDistribution = mock.StrictMock<IPeriodDistribution>();
            
            using (mock.Record())
            {
            }
            using (mock.Playback())
            {
                Assert.AreEqual(0, _target.IntraIntervalDeviation);
                Assert.AreEqual(0, _target.IntraIntervalRootMeanSquare);

				_populationStatisticsCalculatedValues = new PopulationStatisticsCalculatedValues(1,2);
                _target.SetDistributionValues(_populationStatisticsCalculatedValues, periodDistribution);
                
                Assert.AreNotEqual(0, _target.IntraIntervalDeviation);
                Assert.AreNotEqual(0, _target.IntraIntervalRootMeanSquare);
            }
        }

        /// <summary>
        /// Verifies the estimated service level.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-03-05
        /// </remarks>
        [Test]
        public void VerifyEstimatedServiceLevel()
        {
            //Email and other except phone
			ISkillDay skillDayEmail =
				SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("Email", SkillTypeFactory.CreateSkillTypeEmail(), 60), new DateTime(2009, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
			_target.SetSkillDay(skillDayEmail);
            _target.PickResources65();

            //Demand is 0
            Assert.IsTrue(new Percent(1) == _target.EstimatedServiceLevel);
            double demand = 172.2;
            typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_target.Payload, demand);

            _target.PickResources65();

            //Normal non phone calculation
            Assert.IsTrue(new Percent(1) > _target.EstimatedServiceLevel);
            demand = 10.2;
            typeof(SkillStaff).GetField("_forecastedIncomingDemand", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_target.Payload, demand);

            _target.PickResources65();

            //Over 100% non phone
            Assert.AreEqual(new Percent(1), _target.EstimatedServiceLevel);

            //Phone, Erlang calc is used
            double serviceLevel =
                _staffingCalculatorService.ServiceLevelAchieved(_target.Payload.CalculatedResource,
                                                                _target.Payload.ServiceAgreementData.ServiceLevel.
                                                                    Seconds,
                                                                _target.Payload.TaskData.Tasks,
                                                                _target.Payload.TaskData.AverageTaskTime.TotalSeconds,
                                                                _target.Period.ElapsedTime(),
                                                                (int)
                                                                _target.Payload.ServiceAgreementData.ServiceLevel.
                                                                    Percent.Value *
                                                                100);
            ISkillDay skillDayPhone =
                SkillDayFactory.CreateSkillDay(SkillFactory.CreateSkill("Phone", SkillTypeFactory.CreateSkillType(), 60), new DateTime(2009, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            _target.SetSkillDay(skillDayPhone);
            Assert.AreEqual(new Percent(serviceLevel), _target.EstimatedServiceLevel);
        }

        /// <summary>
        /// Verifies the actual service level.
        /// </summary>
        /// <remarks>
        /// Created by: marias  
        /// Created date: 2011-05-18
        /// </remarks>
        [Test]
        public void VerifyActualServiceLevel()
        {
            IStatisticTask statisticTask = new StatisticTask {StatAnsweredTasksWithinSL = 2, StatCalculatedTasks = 4};
            _target.StatisticTask = statisticTask;

            Assert.AreEqual(statisticTask, _target.StatisticTask);
            Assert.AreEqual(new Percent(0.5), _target.ActualServiceLevel);
        }

        /// <summary>
        /// Verifies the actual service level.
        /// </summary>
        /// <remarks>
        /// Created by: marias  
        /// Created date: 2011-05-18
        /// </remarks>
        [Test]
        public void VerifyActualServiceLevelResultWhenOver100Percent()
        {
            IStatisticTask statisticTask = new StatisticTask { StatAnsweredTasksWithinSL = 6, StatCalculatedTasks = 4 };
            _target.StatisticTask = statisticTask;

            Assert.AreEqual(statisticTask, _target.StatisticTask);
            Assert.AreEqual(new Percent(1), _target.ActualServiceLevel);
        }

        /// <summary>
        /// Verifies the actual service level.
        /// </summary>
        /// <remarks>
        /// Created by: marias  
        /// Created date: 2011-05-18
        /// </remarks>
        [Test]
        public void VerifyActualServiceLevelAtDivisionByZero()
        {
            IStatisticTask statisticTask = new StatisticTask { StatAnsweredTasksWithinSL = 6, StatCalculatedTasks = 0 };
            _target.StatisticTask = statisticTask;

            Assert.AreEqual(statisticTask, _target.StatisticTask);
            Assert.AreEqual(double.NaN, _target.ActualServiceLevel.Value);
        }

        /// <summary>
        /// Verifies the actual service level.
        /// </summary>
        /// <remarks>
        /// Created by: marias  
        /// Created date: 2011-05-18
        /// </remarks>
        [Test]
        public void VerifyActualServiceLevelWhenStatisticTaskIsNull()
        {
            IStatisticTask statisticTask = new StatisticTask();
            _target.StatisticTask = statisticTask;

            Assert.AreEqual(statisticTask, _target.StatisticTask);
            Assert.AreEqual(double.NaN, _target.ActualServiceLevel.Value);
        }

        [Test]
        public void VerifyAfterCallWorkIsHandledCorrect()
        {
            ITask taskWithLongAfterTalk = new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(320));
            
            _sa = new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20), new Percent(0), new Percent(1));
            _target = new SkillStaffPeriod(_tp, taskWithLongAfterTalk, _sa, _staffingCalculatorService)
	            {
		            IsAvailable = true
	            };
			_target.SetSkillDay(_skillDay);
	        _target.SetCalculatedResource65(20);
            _target.Payload.CalculatedLoggedOn = 321;
            _target.Payload.Efficiency = new Percent(1);
            var serviceLevel =
               _staffingCalculatorService.ServiceLevelAchieved(_target.Payload.CalculatedResource,
                                                               _target.Payload.ServiceAgreementData.ServiceLevel.
                                                                   Seconds,
                                                               _target.Payload.TaskData.Tasks,
                                                               _target.Payload.TaskData.AverageTaskTime.TotalSeconds + _target.Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
                                                               _target.Period.ElapsedTime(),
                                                               (int)
                                                               _target.Payload.ServiceAgreementData.ServiceLevel.
                                                                   Percent.Value *
                                                               100);
            
            _target.PickResources65();
            Assert.AreEqual(new Percent(serviceLevel), _target.EstimatedServiceLevel);
        }

        [Test]
        public void VerifyCanSetStatisticTaskAndActiveAgentCount()
        {
            IStatisticTask statisticTask = new StatisticTask();
            IActiveAgentCount activeAgentCount = new ActiveAgentCount();
            _target.StatisticTask = statisticTask;
            _target.ActiveAgentCount = activeAgentCount;
            Assert.AreEqual(statisticTask,_target.StatisticTask);
            Assert.AreEqual(activeAgentCount,_target.ActiveAgentCount);
        }

        [Test]
        public void VerifyResetMakesSkillStaffPeriodUnavailable()
        {
            _target.CalculateStaff();
            Assert.Greater(_target.SortedSegmentCollection.Count,0);
            _target.Reset();
            Assert.IsFalse(_target.IsAvailable);
            Assert.AreEqual(0,_target.AbsoluteDifference);
            Assert.AreEqual(0, _target.ForecastedDistributedDemand);
            Assert.AreEqual(0, _target.ForecastedDistributedDemandWithShrinkage);
            Assert.AreEqual(0, _target.RelativeDifference);
            Assert.AreEqual(0, _target.SortedSegmentCollection.Count);
            Assert.AreEqual(0, _target.SegmentInThisCollection.Count);
        }

        [Test]
        public void VerifyDoesNotAddSegmentsForEachCalculation()
        {
            _target.CalculateStaff();
            Assert.AreEqual(1, _target.SortedSegmentCollection.Count);
            _target.CalculateStaff();
            Assert.AreEqual(1, _target.SortedSegmentCollection.Count);
        }

        [Test]
        public void VerifyDoesNotAddSegmentsInThisForEachCalculation()
        {
            _target.CalculateStaff();
            Assert.AreEqual(1, _target.SegmentInThisCollection.Count);
            _target.CalculateStaff();
            Assert.AreEqual(1, _target.SegmentInThisCollection.Count);
        }

        [Test]
        public void VerifyForecastedIncomingDemandWithShrinkage()
        {
            _target.Payload.Shrinkage = new Percent(0.2);
            _target.CalculateStaff();

            Assert.AreEqual(
                Math.Round(_target.ForecastedIncomingDemand().TotalMinutes/0.8, 3),
                Math.Round(_target.ForecastedIncomingDemandWithShrinkage().TotalMinutes,3));
        }

		[Test]
		public void ShouldCalculateForecastedIncomingDemandWhenNoManualAgents()
		{
			var svc = mocks.StrictMock<IStaffingCalculatorService>();
			var level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(2).TotalSeconds);

			stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
															_task,
															new ServiceAgreement(level1, new Percent(1), new Percent(2)),
															svc);
			stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), _task, ServiceAgreement.DefaultValues(), _staffingCalculatorService);
			IList<ISkillStaffPeriod> allSkillStaffPeriods = new List<ISkillStaffPeriod>();
			allSkillStaffPeriods.Add(stPeriod1);
			allSkillStaffPeriods.Add(stPeriod2);
			stPeriod1.SetSkillDay(_skillDay);
			using (mocks.Record())
			{
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments().Return(10d);
				Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d);
				_target.Payload.ManualAgents = null;
			}
			using (mocks.Playback())
			{
				stPeriod1.CalculateStaff(allSkillStaffPeriods);
			}
			Assert.AreEqual(10, stPeriod1.Payload.ForecastedIncomingDemand);
		}

		[Test]
		public void VerifyUsingManualAgentsInsteadOfForecastedIncomingDemand()
		{
			_target.Payload.ManualAgents = 150d; 
			_target.CalculateStaff();
			Assert.AreEqual(150d, _target.Payload.ForecastedIncomingDemand);
		}

		[Test]
		public void VerifyUsingManualAgentsWithShrinkage()
		{
			_target.Payload.ManualAgents = 150d;
			_target.Payload.UseShrinkage = true;
			_target.Payload.Shrinkage = new Percent(0.3);
			_target.CalculateStaff();
			Assert.AreEqual(150d/(1-_target.Payload.Shrinkage.Value), _target.Payload.ForecastedIncomingDemand);
		}

        [Test]
        public void VerifyForecastedDistributedDemandWithShrinkage()
        {
            _target.Payload.Shrinkage = new Percent(0.3);
			_target.CalculateStaff();

            Assert.AreEqual(
                Math.Round(_target.ForecastedDistributedDemand * 1.3, 3),
                Math.Round(_target.ForecastedDistributedDemandWithShrinkage, 3));
        }
        
        [Test]
        public void VerifyCombine()
        {
            Task task1 = new Task(200, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));
            Task task2 = new Task(50, TimeSpan.FromSeconds(240), TimeSpan.FromSeconds(40));
            ServiceAgreement sa1 = new ServiceAgreement(new ServiceLevel(new Percent(0.9), 30), new Percent(0.7), new Percent(0.95));
            ISkillStaffPeriod sp1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_tp, task1, sa1);
            ISkillStaffPeriod sp2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_tp, task2, sa1);

            List<ISkillStaffPeriod> list = new List<ISkillStaffPeriod>();
            list.Add(_target);
            list.Add(sp1);
            list.Add(sp2);

            ISkillStaffPeriod result = SkillStaffPeriod.Combine(list);

            Assert.AreEqual(350, result.Payload.TaskData.Tasks);
            Assert.AreEqual(102.857, result.Payload.TaskData.AverageTaskTime.TotalSeconds);
            Assert.AreEqual(17.143, result.Payload.TaskData.AverageAfterTaskTime.TotalSeconds);

            Assert.AreEqual(0.87, Math.Round(result.Payload.ServiceAgreementData.ServiceLevel.Percent.Value, 2));
            Assert.AreEqual(27.143, Math.Round(result.Payload.ServiceAgreementData.ServiceLevel.Seconds, 3));

            Assert.AreEqual(0.5, Math.Round(result.Payload.ServiceAgreementData.MinOccupancy.Value, 2));
            Assert.AreEqual(0.96, Math.Round(result.Payload.ServiceAgreementData.MaxOccupancy.Value, 2));
        }

		[Test]
		public void ShouldCombineForecastIncomingDemand()
		{
			const int length = 15;
			const double forecastValue1 = 1d;
			const double forecastValue2 = 2d;
			var skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc), length, forecastValue1, 10d);
			var skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, new DateTime(2013, 1, 1, 0, 0, 0, DateTimeKind.Utc), length, forecastValue2, 10d);
			var task = new Task(200, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));
			skillStaffPeriod1.Payload.TaskData = task;
			const double expected = forecastValue1 + forecastValue2;

			
			var list = new List<ISkillStaffPeriod>{skillStaffPeriod1, skillStaffPeriod2};
			var result = SkillStaffPeriod.Combine(list);
			Assert.AreEqual(expected, result.Payload.ForecastedIncomingDemand);
		}

        [Test]
        public void VerifyCombineCanHandleZeroTasks()
        {
            Task task1 = new Task(0, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));
            Task task2 = new Task(0, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));

            ServiceAgreement sa1 = new ServiceAgreement(new ServiceLevel(new Percent(0.9), 30), new Percent(0.7), new Percent(0.95));
            ISkillStaffPeriod sp1 = new SkillStaffPeriod(_tp, task1, sa1,_staffingCalculatorService);
            ISkillStaffPeriod sp2 = new SkillStaffPeriod(_tp, task2, sa1,_staffingCalculatorService);

            List<ISkillStaffPeriod> list = new List<ISkillStaffPeriod>();
            list.Add(sp1);
            list.Add(sp2);

            ISkillStaffPeriod result = SkillStaffPeriod.Combine(list);
            Assert.AreEqual(0, result.Payload.TaskData.Tasks);
            Assert.AreEqual(0, result.Payload.TaskData.AverageTaskTime.TotalSeconds);
            Assert.AreEqual(0, result.Payload.TaskData.AverageAfterTaskTime.TotalSeconds);

            Assert.AreEqual(0.8d, Math.Round(result.Payload.ServiceAgreementData.ServiceLevel.Percent.Value, 2));
            Assert.AreEqual(20, Math.Round(result.Payload.ServiceAgreementData.ServiceLevel.Seconds, 3));

            Assert.AreEqual(0.3d, Math.Round(result.Payload.ServiceAgreementData.MinOccupancy.Value, 2));
            Assert.AreEqual(0.9d, Math.Round(result.Payload.ServiceAgreementData.MaxOccupancy.Value, 2));

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyCombineWithZeroItem()
        {
            List<ISkillStaffPeriod> list = new List<ISkillStaffPeriod>();
            ISkillStaffPeriod result = SkillStaffPeriod.Combine(list);
            Assert.IsNull(result);
        }

        [Test]
        public void VerifyCombineWithOneItem()
        {
            Task task1 = new Task(70, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10));
            ServiceAgreement sa1 = new ServiceAgreement(new ServiceLevel(new Percent(0.9), 30), new Percent(0.7), new Percent(0.95));
            SkillStaffPeriod sp1 = new SkillStaffPeriod(_tp, task1, sa1,_staffingCalculatorService);

            List<ISkillStaffPeriod> list = new List<ISkillStaffPeriod>();
            list.Add(sp1);

            ISkillStaffPeriod result = SkillStaffPeriod.Combine(list);

            Assert.AreNotSame(result, sp1); //verify cloned
            Assert.AreEqual(sp1.Payload.TaskData.Tasks, result.Payload.TaskData.Tasks);
        }

        [Test]
        public void VerifyIntersectingResultWhenContainedInPeriod()
        {
            DateTimePeriod tp = new DateTimePeriod(_tp.StartDateTime.AddHours(-1), _tp.EndDateTime.AddHours(1));
            ISkillStaffPeriod skillStaffPeriod = _target.IntersectingResult(tp);
            Assert.AreEqual(_target.Payload.TaskData.Tasks, skillStaffPeriod.Payload.TaskData.Tasks);
            Assert.AreEqual(tp, skillStaffPeriod.Period);
        }

        #region CalculateStaff
        [Test]
        public void VerifyCalculateStaffDoesNotThrowExceptionToStartWith()
        {
            _target.CalculateStaff();
            Assert.AreNotEqual(0, _target.Payload.ForecastedIncomingDemand);
        }

        [Test]
        public void VerifyCalculateStaffWhenTwoSegments()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(2).TotalSeconds);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000,1,1,2000,1,2),
                                                            _task,
                                                            new ServiceAgreement(level1, new Percent(1), new Percent(2)),
                                                            svc);
            stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), _task, ServiceAgreement.DefaultValues(),_staffingCalculatorService);
            IList<ISkillStaffPeriod> allSkillStaffPeriods = new List<ISkillStaffPeriod>();
            allSkillStaffPeriods.Add(stPeriod1);
            allSkillStaffPeriods.Add(stPeriod2);
			stPeriod1.SetSkillDay(_skillDay);
			
            using(mocks.Record())
            {
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments().Return(10d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d);
            }
            using(mocks.Playback())
            {
                stPeriod1.CalculateStaff(allSkillStaffPeriods);
            }
            Assert.AreEqual(10, stPeriod1.Payload.ForecastedIncomingDemand);
            Assert.AreEqual(2, stPeriod1.SortedSegmentCollection.Count);
            Assert.AreEqual(1, stPeriod1.SegmentInThisCollection.Count);
            Assert.AreSame(stPeriod1.SortedSegmentCollection[0], stPeriod1.SegmentInThisCollection[0]);
            Assert.AreEqual(1, stPeriod2.SegmentInThisCollection.Count);
            Assert.AreSame(stPeriod1.SortedSegmentCollection[1], stPeriod2.SegmentInThisCollection[0]);
        }

		[Test]
		public void CalculateStaff_EfficiencyShouldNotAffectCalculatedOccupancy()
		{
			var calcService = MockRepository.GenerateStrictMock<IStaffingCalculatorService>();
			var dateTimePeriod = new DateTimePeriod(2013, 11, 04, 2013, 11, 04);
			var period = new SkillStaffPeriod(dateTimePeriod,
			                                  _task,
			                                  ServiceAgreement.DefaultValues(),
			                                  calcService) {Payload = {Efficiency = new Percent(0.9)}};
			period.SetSkillDay(_skillDay);
			var periods = new List<ISkillStaffPeriod> {period};

			calcService.Expect(c => c.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments().Return(100);
			calcService.Expect(c => c.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(83);
			
			period.CalculateStaff(periods);

			var args = calcService.GetArgumentsForCallsMadeOn(c => c.Utilization(1, 1, 1, TimeSpan.MinValue), s => s.IgnoreArguments());
			args[0][0].Should().Be.EqualTo(100d);
		}
       

        [Test]
        public void VerifyCalculateStaffWhenLastSegmentIsOnlyParted()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(1.5).TotalSeconds);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                                            _task,
                                                            new ServiceAgreement(level1, new Percent(1), new Percent(2)),
                                                            svc);
            stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), _task, new ServiceAgreement(),_staffingCalculatorService);
            IList<ISkillStaffPeriod> allSkillStaffPeriods = new List<ISkillStaffPeriod>();
            allSkillStaffPeriods.Add(stPeriod1);
            allSkillStaffPeriods.Add(stPeriod2);
			stPeriod1.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments().Return(6d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d);
            }
            using (mocks.Playback())
            {
                stPeriod1.CalculateStaff(allSkillStaffPeriods);
            }
            Assert.AreEqual(2, stPeriod1.SortedSegmentCollection.Count);
            Assert.AreEqual(1, stPeriod1.SegmentInThisCollection.Count);
            Assert.AreSame(stPeriod1.SortedSegmentCollection[0], stPeriod1.SegmentInThisCollection[0]);
            Assert.AreEqual(1, stPeriod2.SegmentInThisCollection.Count);
            Assert.AreSame(stPeriod1.SortedSegmentCollection[1], stPeriod2.SegmentInThisCollection[0]);

        }

        [Test]
        public void VerifyCalculateStaffWhenSegmentLengthDiffer()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(5).TotalSeconds);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 4),
                                                            _task,
                                                            new ServiceAgreement(level1, new Percent(1), new Percent(2)),
                                                            svc);
            stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 4, 2000, 1, 5),
                                                              _task, new ServiceAgreement(),_staffingCalculatorService);
            stPeriod3 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 5, 2000, 1, 7),
                                                              _task, new ServiceAgreement(),_staffingCalculatorService);
            IList<ISkillStaffPeriod> allSkillStaffPeriods = new List<ISkillStaffPeriod>();
            allSkillStaffPeriods.Add(stPeriod1);
            allSkillStaffPeriods.Add(stPeriod2);
            allSkillStaffPeriods.Add(stPeriod3);
			stPeriod1.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments().Return(5d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d);
            }
            using (mocks.Playback())
            {
                stPeriod1.CalculateStaff(allSkillStaffPeriods);
            }
            Assert.AreEqual(5, stPeriod1.Payload.ForecastedIncomingDemand);
            Assert.AreEqual(3, stPeriod1.SortedSegmentCollection.Count);
            Assert.AreEqual(1, stPeriod1.SegmentInThisCollection.Count);
            Assert.AreSame(stPeriod1.SortedSegmentCollection[0], stPeriod1.SegmentInThisCollection[0]);
            Assert.AreEqual(1, stPeriod2.SegmentInThisCollection.Count);
            Assert.AreSame(stPeriod1.SortedSegmentCollection[1], stPeriod2.SegmentInThisCollection[0]);
            Assert.AreEqual(1, stPeriod3.SegmentInThisCollection.Count);
            Assert.AreSame(stPeriod1.SortedSegmentCollection[2], stPeriod3.SegmentInThisCollection[0]);
        }

        [Test]
        public void VerifyCalculateStaffWhenTrafficIntensityIsZero()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(2).TotalSeconds);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                                            _task,
                                                            new ServiceAgreement(level1, new Percent(1), new Percent(2)),
                                                            svc);
            stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3), _task, new ServiceAgreement(),_staffingCalculatorService);
            IList<ISkillStaffPeriod> allSkillStaffPeriods = new List<ISkillStaffPeriod>();
            allSkillStaffPeriods.Add(stPeriod1);
            allSkillStaffPeriods.Add(stPeriod2);
			stPeriod1.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments().Return(0d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d);
            }
            using (mocks.Playback())
            {
                stPeriod1.CalculateStaff(allSkillStaffPeriods);
            }
            Assert.AreEqual(0, stPeriod1.Payload.ForecastedIncomingDemand);
            Assert.AreEqual(2, stPeriod1.SortedSegmentCollection.Count);
            Assert.AreEqual(1, stPeriod1.SegmentInThisCollection.Count);
        }


        [Test]
        public void VerifyAndersRowsTwoIncoming()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
            stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
            stPeriod3 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 3, 2000, 1, 4),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
            stPeriod4 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 4, 2000, 1, 5),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);

            stPeriod5 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 5, 2000, 1, 6),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
            stPeriod6 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 6, 2000, 1, 7),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			stPeriod5.SetSkillDay(_skillDay);
			stPeriod6.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                                .Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                                .Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                                .Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                                .Return(2d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                                .Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2,1)).IgnoreArguments()
                               .Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(6);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4, stPeriod5, stPeriod6 });

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(1);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(1);
                stPeriod5.SetCalculatedResource65(0);
                stPeriod6.SetCalculatedResource65(0);

                Assert.AreEqual(0, stPeriod1.Payload.CalculatedResource);
                Assert.AreEqual(1, stPeriod2.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod3.Payload.CalculatedResource);
                Assert.AreEqual(1, stPeriod4.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod5.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod6.Payload.CalculatedResource);

                Assert.AreEqual(2, stPeriod1.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod2.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(2, stPeriod3.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod4.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod5.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod6.Payload.ForecastedIncomingDemand);

                Assert.AreEqual(0.5, stPeriod1.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod2.ForecastedDistributedDemand);
                Assert.AreEqual(1, stPeriod3.ForecastedDistributedDemand);
                Assert.AreEqual(1, stPeriod4.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod5.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod6.ForecastedDistributedDemand);

                Assert.AreEqual(2, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.IncomingDifference);
                Assert.AreEqual(0, stPeriod2.IncomingDifference);
                Assert.AreEqual(-2, stPeriod3.IncomingDifference);
                Assert.AreEqual(0, stPeriod4.IncomingDifference);
                Assert.AreEqual(0, stPeriod5.IncomingDifference);
                Assert.AreEqual(0, stPeriod6.IncomingDifference);

                //Assert.AreEqual(0, stPeriod1.DistributedDifference);
                //Assert.AreEqual(0, stPeriod2.DistributedDifference);
                //Assert.AreEqual(-0.5, stPeriod3.DistributedDifference);
                //Assert.AreEqual(-0.5, stPeriod4.DistributedDifference);
                //Assert.AreEqual(-0.5, stPeriod5.DistributedDifference);
                //Assert.AreEqual(-0.5, stPeriod6.DistributedDifference);

                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(1, stPeriod2.FStaff);
                Assert.AreEqual(0.5, stPeriod3.FStaff);
                Assert.AreEqual(1.5, stPeriod4.FStaff);
                Assert.AreEqual(0.5, stPeriod5.FStaff);
                Assert.AreEqual(0.5, stPeriod6.FStaff);

            }
        }

        [Test]
        public void VerifyAndersRows()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
            stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
            stPeriod3 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 3, 2000, 1, 4),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
            stPeriod4 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 4, 2000, 1, 5),
                                                               _task,
                                                               new ServiceAgreement(level1, new Percent(1),
                                                                                    new Percent(2)),
                                                               svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);

            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                                .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                                .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                                .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                               .Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(4);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4 });

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(1);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(0);

                Assert.AreEqual(0, stPeriod1.Payload.CalculatedResource);
                Assert.AreEqual(1, stPeriod2.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod3.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod4.Payload.CalculatedResource);

                Assert.AreEqual(2, stPeriod1.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod2.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod3.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod4.Payload.ForecastedIncomingDemand);

                Assert.AreEqual(0.5, stPeriod1.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod2.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod3.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod4.ForecastedDistributedDemand);

                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                Assert.AreEqual(-1, stPeriod1.IncomingDifference);
                Assert.AreEqual(0, stPeriod2.IncomingDifference);
                Assert.AreEqual(0, stPeriod3.IncomingDifference);
                Assert.AreEqual(0, stPeriod4.IncomingDifference);

                //Assert.AreEqual(-0.25, stPeriod1.DistributedDifference);
                //Assert.AreEqual(-0.25, stPeriod2.DistributedDifference);
                //Assert.AreEqual(-0.25, stPeriod3.DistributedDifference);
                //Assert.AreEqual(-0.25, stPeriod4.DistributedDifference);

                Assert.AreEqual(0.25, stPeriod1.FStaff);
                Assert.AreEqual(1.25, stPeriod2.FStaff);
                Assert.AreEqual(0.25, stPeriod3.FStaff);
                Assert.AreEqual(0.25, stPeriod4.FStaff);

            }
        }

        [Test]
        public void VerifyScheduledAgainstIncomingForecast()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromHours(2).TotalSeconds);
            DateTimePeriod period1 = new DateTimePeriod(new DateTime(2009, 02, 02, 7, 0, 0, DateTimeKind.Utc), new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period3 = period2.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period4 = period3.MovePeriod(TimeSpan.FromHours(1));
            ServiceAgreement ag1 = new ServiceAgreement(level1, new Percent(1), new Percent(2));

            stPeriod1 = new SkillStaffPeriod(period1, _task, ag1, svc);
            stPeriod2 = new SkillStaffPeriod(period2, _task, ag1, svc);
            stPeriod3 = new SkillStaffPeriod(period3, _task, ag1, svc);
            stPeriod4 = new SkillStaffPeriod(period4, _task, ag1, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                                .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                                .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                                .Return(2d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                               .Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(4);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> {stPeriod1, stPeriod2, stPeriod3, stPeriod4});

                Assert.AreEqual(2, stPeriod1.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(2, stPeriod2.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod3.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod4.Payload.ForecastedIncomingDemand);

                Assert.AreEqual(1, stPeriod1.ForecastedDistributedDemand);
                Assert.AreEqual(2, stPeriod2.ForecastedDistributedDemand);
                Assert.AreEqual(1, stPeriod3.ForecastedDistributedDemand);
                Assert.AreEqual(0, stPeriod4.ForecastedDistributedDemand);

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(0);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(0);

                Assert.AreEqual(0, stPeriod1.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod2.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod3.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod4.Payload.CalculatedResource);

                Assert.AreEqual(0, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod2.SetCalculatedResource65(3);

                Assert.AreEqual(0, stPeriod1.Payload.CalculatedResource);
                Assert.AreEqual(3, stPeriod2.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod3.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod4.Payload.CalculatedResource);

                Assert.AreEqual(2, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(1, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod2.SetCalculatedResource65(1);
                Assert.AreEqual(1, stPeriod2.Payload.CalculatedResource);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod1).BookedResource65);
                Assert.AreEqual(1, ((SkillStaffPeriod)stPeriod2).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod3).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod4).BookedResource65);
                Assert.AreEqual(1, ((SkillStaff)stPeriod1.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod2.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod3.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod4.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod2.SetCalculatedResource65(0);

                Assert.AreEqual(0, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod1.SetCalculatedResource65(3);
                Assert.AreEqual(3, stPeriod1.Payload.CalculatedResource);
                Assert.AreEqual(3, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), Test]
        public void VerifyScheduledAgainstIncomingForecast1()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromHours(2).TotalSeconds);
            DateTimePeriod period1 = new DateTimePeriod(new DateTime(2009, 02, 02, 7, 0, 0, DateTimeKind.Utc),
                                                        new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period3 = period2.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period4 = period3.MovePeriod(TimeSpan.FromHours(1));
            ServiceAgreement ag1 = new ServiceAgreement(level1, new Percent(1), new Percent(2));

            stPeriod1 = new SkillStaffPeriod(period1, _task, ag1, svc);
            stPeriod2 = new SkillStaffPeriod(period2, _task, ag1, svc);
            stPeriod3 = new SkillStaffPeriod(period3, _task, ag1, svc);
            stPeriod4 = new SkillStaffPeriod(period4, _task, ag1, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(2d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(4);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4 });

                stPeriod1.SetCalculatedResource65(1);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod3.SetCalculatedResource65(1);
                Assert.AreEqual(1, ((SkillStaffPeriod)stPeriod1).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod2).BookedResource65);
                Assert.AreEqual(1, ((SkillStaffPeriod)stPeriod3).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod4).BookedResource65);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(1, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod3.SetCalculatedResource65(2);
                Assert.AreEqual(1, ((SkillStaffPeriod)stPeriod1).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod2).BookedResource65);
                Assert.AreEqual(2, ((SkillStaffPeriod)stPeriod3).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod4).BookedResource65);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(2, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod3.SetCalculatedResource65(3);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(2, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(1, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod1.SetCalculatedResource65(0);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod1).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod2).BookedResource65);
                Assert.AreEqual(2, ((SkillStaffPeriod)stPeriod3).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod4).BookedResource65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod1.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(2, ((SkillStaff)stPeriod2.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod3.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod4.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(2, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(1, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod2.SetCalculatedResource65(1);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod1).BookedResource65);
                Assert.AreEqual(1, ((SkillStaffPeriod)stPeriod2).BookedResource65);
                Assert.AreEqual(2, ((SkillStaffPeriod)stPeriod3).BookedResource65);
                Assert.AreEqual(0, ((SkillStaffPeriod)stPeriod4).BookedResource65);
                Assert.AreEqual(1, ((SkillStaff)stPeriod1.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(2, ((SkillStaff)stPeriod2.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod3.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, ((SkillStaff)stPeriod4.Payload).BookedAgainstIncomingDemand65);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(2, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(1, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
            }
        }

        [Test]
        public void VerifyScheduledAgainstIncomingForecast2()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromHours(2).TotalSeconds);
            DateTimePeriod period1 = new DateTimePeriod(new DateTime(2009, 02, 02, 7, 0, 0, DateTimeKind.Utc),
                                                        new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period3 = period2.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period4 = period3.MovePeriod(TimeSpan.FromHours(1));
            ServiceAgreement ag1 = new ServiceAgreement(level1, new Percent(1), new Percent(2));

            stPeriod1 = new SkillStaffPeriod(period1, _task, ag1, svc);
            stPeriod2 = new SkillStaffPeriod(period2, _task, ag1, svc);
            stPeriod3 = new SkillStaffPeriod(period3, _task, ag1, svc);
            stPeriod4 = new SkillStaffPeriod(period4, _task, ag1, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(1d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(2d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(4);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4 });

                stPeriod3.SetCalculatedResource65(3.5);
                Assert.AreEqual(0, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(2, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(1.5, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                stPeriod2.SetCalculatedResource65(1);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(2, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(1.5, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
            }
        }

        [Test]
        public void VerifyScheduledAgainstIncomingForecast3()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromHours(3).TotalSeconds);
            DateTimePeriod period1 = new DateTimePeriod(new DateTime(2009, 02, 02, 7, 0, 0, DateTimeKind.Utc),
                                                        new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period3 = period2.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period4 = period3.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period5 = period4.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period6 = period5.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period7 = period6.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period8 = period7.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period9 = period8.MovePeriod(TimeSpan.FromHours(1));

            ServiceAgreement ag1 = new ServiceAgreement(level1, new Percent(1), new Percent(2));

            stPeriod1 = new SkillStaffPeriod(period1, _task, ag1, svc);
            stPeriod2 = new SkillStaffPeriod(period2, _task, ag1, svc);
            stPeriod3 = new SkillStaffPeriod(period3, _task, ag1, svc);
            stPeriod4 = new SkillStaffPeriod(period4, _task, ag1, svc);
            stPeriod5 = new SkillStaffPeriod(period5, _task, ag1, svc);
            stPeriod6 = new SkillStaffPeriod(period6, _task, ag1, svc);
            stPeriod7 = new SkillStaffPeriod(period7, _task, ag1, svc);
			
            ISkillStaffPeriod stPeriod8 = new SkillStaffPeriod(period8, _task, ag1, svc);
            ISkillStaffPeriod stPeriod9 = new SkillStaffPeriod(period9, _task, ag1, svc);

			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			stPeriod5.SetSkillDay(_skillDay);
			stPeriod6.SetSkillDay(_skillDay);
			stPeriod7.SetSkillDay(_skillDay);
			stPeriod8.SetSkillDay(_skillDay);
			stPeriod9.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(3d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(6d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(6d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(3d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(6d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(6d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(9); 
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4, stPeriod5, stPeriod6, stPeriod7, stPeriod8, stPeriod9 });

                stPeriod3.SetCalculatedResource65(6);
                Assert.AreEqual(6, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod8.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod9.ScheduledAgentsIncoming);

                stPeriod2.SetCalculatedResource65(1);
                Assert.AreEqual(6, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(1, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod8.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod9.ScheduledAgentsIncoming);

                stPeriod4.SetCalculatedResource65(11);
                Assert.AreEqual(6, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(3, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(3, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod8.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod9.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.IncomingDifference);
                Assert.AreEqual(0, stPeriod2.IncomingDifference);
                Assert.AreEqual(0, stPeriod3.IncomingDifference);
                Assert.AreEqual(3, stPeriod4.IncomingDifference);
                Assert.AreEqual(-6, stPeriod5.IncomingDifference);
                Assert.AreEqual(-6, stPeriod6.IncomingDifference);
                Assert.AreEqual(-3, stPeriod7.IncomingDifference);
                Assert.AreEqual(0, stPeriod8.IncomingDifference);
                Assert.AreEqual(0, stPeriod9.IncomingDifference);

                //Assert.AreEqual(0, stPeriod1.DistributedDifference);
                //Assert.AreEqual(0, stPeriod2.DistributedDifference);
                //Assert.AreEqual(0, stPeriod3.DistributedDifference);
                //Assert.AreEqual(1, stPeriod4.DistributedDifference);
                //Assert.AreEqual(-1, stPeriod5.DistributedDifference);
                //Assert.AreEqual(-3, stPeriod6.DistributedDifference);
                //Assert.AreEqual(-5, stPeriod7.DistributedDifference);
                //Assert.AreEqual(-3, stPeriod8.DistributedDifference);
                //Assert.AreEqual(-1, stPeriod9.DistributedDifference);

                stPeriod3.SetCalculatedResource65(0);
                Assert.AreEqual(1, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(3, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(2, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod8.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod9.ScheduledAgentsIncoming);

                stPeriod2.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(0);
                Assert.AreEqual(0, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod8.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod9.ScheduledAgentsIncoming);

            }
        }

        [Test]
        public void VerifyFStaff1()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);
            ServiceAgreement sa = new ServiceAgreement(level1, new Percent(1),
                                                       new Percent(2));
            DateTimePeriod dtp = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);

            stPeriod1 = new SkillStaffPeriod(dtp, _task, sa, svc);
            stPeriod2 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(1)), _task, sa, svc);
            stPeriod3 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(2)), _task, sa, svc);
            stPeriod4 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(3)), _task, sa, svc);
            stPeriod5 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(4)), _task, sa, svc);
            stPeriod6 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(5)), _task, sa, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			stPeriod5.SetSkillDay(_skillDay);
			stPeriod6.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2,1)).IgnoreArguments().Return(0d);
                Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2,1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(6);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4, stPeriod5, stPeriod6 });

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(0);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(0);
                stPeriod5.SetCalculatedResource65(0);
                stPeriod6.SetCalculatedResource65(0);

                Assert.AreEqual(0.5, stPeriod1.FStaff);
                Assert.AreEqual(0.5, stPeriod2.FStaff);
                Assert.AreEqual(0.5, stPeriod3.FStaff);
                Assert.AreEqual(0.5, stPeriod4.FStaff);

                stPeriod3.SetCalculatedResource65(1);
                Assert.AreEqual(0.25, stPeriod1.FStaff);
                Assert.AreEqual(0.25, stPeriod2.FStaff);
                Assert.AreEqual(1.25, stPeriod3.FStaff);
                Assert.AreEqual(0.25, stPeriod4.FStaff);

                stPeriod3.SetCalculatedResource65(2);
                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(0, stPeriod2.FStaff);
                Assert.AreEqual(2, stPeriod3.FStaff);
                Assert.AreEqual(0, stPeriod4.FStaff);

                stPeriod3.SetCalculatedResource65(10);
                stPeriod3.SetCalculatedResource65(10);
                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(0, stPeriod2.FStaff);
                Assert.AreEqual(2, stPeriod3.FStaff);
                Assert.AreEqual(0, stPeriod4.FStaff);
            }
        }

        [Test]
        public void VerifyAndersRowsWhenOverstaffed()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);
            ServiceAgreement sa = new ServiceAgreement(level1, new Percent(1),
                                                       new Percent(2));
            DateTimePeriod dtp = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);

            stPeriod1 = new SkillStaffPeriod(dtp, _task, sa, svc);
            stPeriod2 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(1)), _task, sa, svc);
            stPeriod3 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(2)), _task, sa, svc);
            stPeriod4 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(3)), _task, sa, svc);
            stPeriod5 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(4)), _task, sa, svc);
            stPeriod6 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(5)), _task, sa, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			stPeriod5.SetSkillDay(_skillDay);
			stPeriod6.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(6);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4, stPeriod5, stPeriod6 });

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(0);
                stPeriod3.SetCalculatedResource65(3);
                stPeriod4.SetCalculatedResource65(0);
                stPeriod5.SetCalculatedResource65(0);
                stPeriod6.SetCalculatedResource65(0);

                Assert.AreEqual(0, stPeriod1.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod2.Payload.CalculatedResource);
                Assert.AreEqual(3, stPeriod3.Payload.CalculatedResource);
                Assert.AreEqual(0, stPeriod4.Payload.CalculatedResource);

                Assert.AreEqual(2, stPeriod1.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod2.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod3.Payload.ForecastedIncomingDemand);
                Assert.AreEqual(0, stPeriod4.Payload.ForecastedIncomingDemand);

                Assert.AreEqual(0.5, stPeriod1.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod2.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod3.ForecastedDistributedDemand);
                Assert.AreEqual(0.5, stPeriod4.ForecastedDistributedDemand);

                Assert.AreEqual(2, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(1, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.IncomingDifference);
                Assert.AreEqual(0, stPeriod2.IncomingDifference);
                Assert.AreEqual(1, stPeriod3.IncomingDifference);
                Assert.AreEqual(0, stPeriod4.IncomingDifference);

                //Assert.AreEqual(0, stPeriod1.DistributedDifference);
                //Assert.AreEqual(0, stPeriod2.DistributedDifference);
                //Assert.AreEqual(0.25, stPeriod3.DistributedDifference);
                //Assert.AreEqual(0.25, stPeriod4.DistributedDifference);
                //Assert.AreEqual(0.25, stPeriod5.DistributedDifference);
                //Assert.AreEqual(0.25, stPeriod6.DistributedDifference);

                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(0, stPeriod2.FStaff);
                Assert.AreEqual(2, stPeriod3.FStaff);
                Assert.AreEqual(0, stPeriod4.FStaff);

            }
        }

        [Test]
        public void VerifyAbsoluteAndRelativeDifference()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);
            ServiceAgreement sa = new ServiceAgreement(level1, new Percent(1),
                                                       new Percent(2));
            DateTimePeriod dtp = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);

            stPeriod1 = new SkillStaffPeriod(dtp, _task, sa, svc);
            stPeriod2 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(1)), _task, sa, svc);
            stPeriod3 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(2)), _task, sa, svc);
            stPeriod4 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(3)), _task, sa, svc);
            stPeriod5 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(4)), _task, sa, svc);
            stPeriod6 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(5)), _task, sa, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			stPeriod5.SetSkillDay(_skillDay);
			stPeriod6.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(4d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(6);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.
                    Any().Return(7);
            }
            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod>
                                   {stPeriod1, stPeriod2, stPeriod3, stPeriod4, stPeriod5, stPeriod6});

                
                Assert.AreEqual(1, stPeriod1.FStaff);
                Assert.AreEqual(0, stPeriod1.Payload.CalculatedResource);
                Assert.AreEqual(-1, stPeriod1.AbsoluteDifference);
                Assert.AreEqual(-1, stPeriod1.RelativeDifference);
                Assert.AreEqual(0, stPeriod5.RelativeDifference);
                Assert.AreEqual(0, stPeriod5.AbsoluteDifference);

                stPeriod5.SetCalculatedResource65(1);
                Assert.AreEqual(double.NaN, stPeriod5.RelativeDifference);
                Assert.AreEqual(1, stPeriod5.AbsoluteDifference);
            }
        }

        [Test]
        public void VerifyAbsoluteAndRelativeDifferenceIncoming()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);
            ServiceAgreement sa = new ServiceAgreement(level1, new Percent(1),
                                                       new Percent(2));
            DateTimePeriod dtp = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);

            stPeriod1 = new SkillStaffPeriod(dtp, _task, sa, svc);
            stPeriod2 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(1)), _task, sa, svc);
            stPeriod3 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(2)), _task, sa, svc);
            stPeriod4 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(3)), _task, sa, svc);
            stPeriod5 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(4)), _task, sa, svc);
            stPeriod6 = new SkillStaffPeriod(dtp.MovePeriod(TimeSpan.FromDays(5)), _task, sa, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			stPeriod5.SetSkillDay(_skillDay);
			stPeriod6.SetSkillDay(_skillDay);
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(4d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, TimeSpan.Zero, 2, 2, 1)).IgnoreArguments().Return(4d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(6);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }
            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4, stPeriod5, stPeriod6 });

                stPeriod1.SetCalculatedResource65(6);
                Assert.AreEqual(2, stPeriod1.IncomingDifference);
                Assert.AreEqual(-4, stPeriod2.IncomingDifference);
                Assert.AreEqual(0, stPeriod6.IncomingDifference);
                Assert.AreEqual(2d/4d, stPeriod1.RelativeDifferenceIncoming);
                Assert.AreEqual(-4/4, stPeriod2.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod6.RelativeDifferenceIncoming);

                stPeriod6.SetCalculatedResource65(1);
                Assert.AreEqual(1, stPeriod6.IncomingDifference);
                Assert.IsNaN(stPeriod6.RelativeDifferenceIncoming);

            }
        }

        [Test]
        public void VerifyOverstaff1()
        {
            List<ISkillStaffPeriod> periodlist = GetPeriodlistWith7Periods();
            using (mocks.Playback())
            {
                calculateStaff(periodlist);

                Assert.AreEqual(1, stPeriod1.ForecastedDistributedDemand);
                Assert.AreEqual(2, stPeriod2.ForecastedDistributedDemand);
                Assert.AreEqual(3, stPeriod3.ForecastedDistributedDemand);
                Assert.AreEqual(4, stPeriod4.ForecastedDistributedDemand);
                Assert.AreEqual(3, stPeriod5.ForecastedDistributedDemand);
                Assert.AreEqual(2, stPeriod6.ForecastedDistributedDemand);
                Assert.AreEqual(1, stPeriod7.ForecastedDistributedDemand);

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(8);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(0);
                stPeriod5.SetCalculatedResource65(0);
                stPeriod6.SetCalculatedResource65(0);
                stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, stPeriod1.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(4, stPeriod2.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, stPeriod3.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, stPeriod4.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, stPeriod5.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, stPeriod6.Payload.BookedAgainstIncomingDemand65);
                Assert.AreEqual(0, stPeriod7.Payload.BookedAgainstIncomingDemand65);

                Assert.AreEqual(4, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(4, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.IncomingDifference);
                Assert.AreEqual(0, stPeriod2.IncomingDifference);
                Assert.AreEqual(-4, stPeriod3.IncomingDifference);
                Assert.AreEqual(-4, stPeriod4.IncomingDifference);
                Assert.AreEqual(0, stPeriod5.IncomingDifference);
                Assert.AreEqual(0, stPeriod6.IncomingDifference);
                Assert.AreEqual(0, stPeriod7.IncomingDifference);

                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(8, stPeriod2.FStaff);
                Assert.AreEqual(1, stPeriod3.FStaff);
                Assert.AreEqual(2, stPeriod4.FStaff);
                Assert.AreEqual(2, stPeriod5.FStaff);
                Assert.AreEqual(2, stPeriod6.FStaff);
                Assert.AreEqual(1, stPeriod7.FStaff);

                Assert.AreEqual(0, stPeriod1.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod2.RelativeDifferenceIncoming);
                Assert.AreEqual(-1, stPeriod3.RelativeDifferenceIncoming);
                Assert.AreEqual(-1, stPeriod4.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod5.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod6.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod7.RelativeDifferenceIncoming);

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(10);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(0);
                stPeriod5.SetCalculatedResource65(0);
                stPeriod6.SetCalculatedResource65(0);
                stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.IncomingDifference);
                Assert.AreEqual(2, stPeriod2.IncomingDifference);
                Assert.AreEqual(-4, stPeriod3.IncomingDifference);
                Assert.AreEqual(-4, stPeriod4.IncomingDifference);
                Assert.AreEqual(0, stPeriod5.IncomingDifference);
                Assert.AreEqual(0, stPeriod6.IncomingDifference);
                Assert.AreEqual(0, stPeriod7.IncomingDifference);

                Assert.AreEqual(0, stPeriod1.SortedSegmentCollection[0].FStaff());
                Assert.AreEqual(4, stPeriod1.SortedSegmentCollection[1].FStaff());
                Assert.AreEqual(0, stPeriod1.SortedSegmentCollection[2].FStaff());
                Assert.AreEqual(0, stPeriod1.SortedSegmentCollection[3].FStaff());

                double devider = stPeriod2.Payload.BookedAgainstIncomingDemand65;
                Assert.AreEqual(4, devider);

                devider = stPeriod2.SortedSegmentCollection[0].BelongsTo.Payload.BookedAgainstIncomingDemand65;
                Assert.AreEqual(4, devider);

                Assert.AreEqual(4, stPeriod2.SortedSegmentCollection[0].BookedResource65);

                Assert.AreEqual(4, stPeriod2.SortedSegmentCollection[0].FStaff());
                Assert.AreEqual(0, stPeriod2.SortedSegmentCollection[1].FStaff());
                Assert.AreEqual(0, stPeriod2.SortedSegmentCollection[2].FStaff());
                Assert.AreEqual(0, stPeriod2.SortedSegmentCollection[3].FStaff());

                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(8, stPeriod2.FStaff);
                Assert.AreEqual(1, stPeriod3.FStaff);
                Assert.AreEqual(2, stPeriod4.FStaff);
                Assert.AreEqual(2, stPeriod5.FStaff);
                Assert.AreEqual(2, stPeriod6.FStaff);
                Assert.AreEqual(1, stPeriod7.FStaff);

                Assert.AreEqual(0, stPeriod1.RelativeDifferenceIncoming);
                Assert.AreEqual(0.5, stPeriod2.RelativeDifferenceIncoming);
                Assert.AreEqual(-1, stPeriod3.RelativeDifferenceIncoming);
                Assert.AreEqual(-1, stPeriod4.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod5.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod6.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod7.RelativeDifferenceIncoming);

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(10);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(8);
                stPeriod5.SetCalculatedResource65(0);
                stPeriod6.SetCalculatedResource65(0);
                stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(4, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(4, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(8, stPeriod2.FStaff);
                Assert.AreEqual(0, stPeriod3.FStaff);
                Assert.AreEqual(8, stPeriod4.FStaff);
                Assert.AreEqual(0, stPeriod5.FStaff);
                Assert.AreEqual(0, stPeriod6.FStaff);
                Assert.AreEqual(0, stPeriod7.FStaff);

                Assert.AreEqual(0, stPeriod1.RelativeDifferenceIncoming);
                Assert.AreEqual(0.5, stPeriod2.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod3.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod4.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod5.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod6.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod7.RelativeDifferenceIncoming);

                stPeriod1.SetCalculatedResource65(0);
                stPeriod2.SetCalculatedResource65(10);
                stPeriod3.SetCalculatedResource65(0);
                stPeriod4.SetCalculatedResource65(8);
                stPeriod5.SetCalculatedResource65(4);
                stPeriod6.SetCalculatedResource65(0);
                stPeriod7.SetCalculatedResource65(0);

                Assert.AreEqual(4, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(6, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(4, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(4, stPeriod4.ScheduledAgentsIncoming);
                Assert.AreEqual(4, stPeriod5.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod6.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod7.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.FStaff);
                Assert.AreEqual(8, stPeriod2.FStaff);
                Assert.AreEqual(0, stPeriod3.FStaff);
                Assert.AreEqual(8, stPeriod4.FStaff);
                Assert.AreEqual(0, stPeriod5.FStaff);
                Assert.AreEqual(0, stPeriod6.FStaff);
                Assert.AreEqual(0, stPeriod7.FStaff);

                TimeSpan expectedCalculatedResource = new TimeSpan(10, 0, 0, 0); 
                Assert.AreEqual(expectedCalculatedResource.TotalHours, stPeriod2.ScheduledHours());

                TimeSpan expextedFStaffTime = new TimeSpan(8, 0, 0, 0);
                Assert.AreEqual(expextedFStaffTime, stPeriod2.FStaffTime());
                
                Assert.AreEqual(0, stPeriod2.CalculatedLoggedOn);

                Assert.AreEqual(0, stPeriod1.RelativeDifferenceIncoming);
                Assert.AreEqual(0.5, stPeriod2.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod3.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod4.RelativeDifferenceIncoming);
                Assert.IsNaN(stPeriod5.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod6.RelativeDifferenceIncoming);
                Assert.AreEqual(0, stPeriod7.RelativeDifferenceIncoming);

            }
        }

        [Test]
        public void VerifyAbsoluteDifferenceScheduledAndForecasted()
        {
            List<ISkillStaffPeriod> periodlist = GetPeriodlistWith7Periods();
            using (mocks.Playback())
            {
                calculateStaff(periodlist);
                stPeriod2.SetCalculatedResource65(7);
                double fStaff = stPeriod2.FStaff;
                double scheduled = 7;
                double expected = scheduled - fStaff;
                Assert.AreEqual(expected, stPeriod2.AbsoluteDifference);
            }
        }

        [Test]
        public void VerifyAbsoluteDifferenceMinMaxBoosted()
        {
            List<ISkillStaffPeriod> periodlist = GetPeriodlistWith7Periods();
            using (mocks.Playback())
            {
                calculateStaff(periodlist);
                stPeriod2.SetCalculatedResource65(7);
                SkillPersonData skillPersonData = new SkillPersonData(3, 4);
                stPeriod2.Payload.SkillPersonData = skillPersonData;
                stPeriod2.Payload.CalculatedLoggedOn = 1;
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceMaxStaffBoosted());
                Assert.AreEqual(-20000 - 0.25, stPeriod2.AbsoluteDifferenceMinStaffBoosted());
                Assert.AreEqual(-20000 - 0.25, stPeriod2.AbsoluteDifferenceBoosted());
                stPeriod2.Payload.CalculatedLoggedOn = 3;
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceMaxStaffBoosted());
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceMinStaffBoosted());
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceBoosted());
                stPeriod2.Payload.CalculatedLoggedOn = 4;
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceMaxStaffBoosted());
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceMinStaffBoosted());
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceBoosted());
                stPeriod2.Payload.CalculatedLoggedOn = 5;
                Assert.AreEqual(10000 - 0.25, stPeriod2.AbsoluteDifferenceMaxStaffBoosted());
                Assert.AreEqual(0 - 0.25, stPeriod2.AbsoluteDifferenceMinStaffBoosted());
                Assert.AreEqual(10000 - 0.25, stPeriod2.AbsoluteDifferenceBoosted());

                
            }
        }

        [Test]
        public void VerifyAbsoluteDifferenceScheduledHeadsAndMinMaxHeads()
        {
            List<ISkillStaffPeriod> periodlist = GetPeriodlistWith7Periods();
            using (mocks.Playback())
            {
                calculateStaff(periodlist);
                stPeriod2.SetCalculatedResource65(7);
                SkillPersonData skillPersonData = new SkillPersonData(3,4);
                stPeriod2.Payload.SkillPersonData = skillPersonData;
                stPeriod2.Payload.CalculatedLoggedOn = 1;
                Assert.AreEqual(-2, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));
                stPeriod2.Payload.CalculatedLoggedOn = 3;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));
                stPeriod2.Payload.CalculatedLoggedOn = 4;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));
                stPeriod2.Payload.CalculatedLoggedOn = 5;
                Assert.AreEqual(1, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));

                skillPersonData = new SkillPersonData(0, 0);
                stPeriod2.Payload.SkillPersonData = skillPersonData;
                stPeriod2.Payload.CalculatedLoggedOn = 1;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));
                stPeriod2.Payload.CalculatedLoggedOn = 3;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));
                stPeriod2.Payload.CalculatedLoggedOn = 4;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));
                stPeriod2.Payload.CalculatedLoggedOn = 5;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(false));
            }
        }

        [Test]
        public void VerifyAbsoluteDifferenceScheduledHeadsAndMinMaxHeadsForShiftValueCalculator()
        {
            List<ISkillStaffPeriod> periodlist = GetPeriodlistWith7Periods();
            using (mocks.Playback())
            {
                calculateStaff(periodlist);
                stPeriod2.SetCalculatedResource65(7);
                SkillPersonData skillPersonData = new SkillPersonData(3, 4);
                stPeriod2.Payload.SkillPersonData = skillPersonData;
                stPeriod2.Payload.CalculatedLoggedOn = 1;
                Assert.AreEqual(-2, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));
                stPeriod2.Payload.CalculatedLoggedOn = 3;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));
                stPeriod2.Payload.CalculatedLoggedOn = 4;
                Assert.AreEqual(1, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));
                stPeriod2.Payload.CalculatedLoggedOn = 5;
                Assert.AreEqual(2, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));

                skillPersonData = new SkillPersonData(0, 0);
                stPeriod2.Payload.SkillPersonData = skillPersonData;
                stPeriod2.Payload.CalculatedLoggedOn = 1;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));
                stPeriod2.Payload.CalculatedLoggedOn = 3;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));
                stPeriod2.Payload.CalculatedLoggedOn = 4;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));
                stPeriod2.Payload.CalculatedLoggedOn = 5;
                Assert.AreEqual(0, stPeriod2.AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(true));
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyIExistsInList()
        {
            _target.CalculateStaff(new List<ISkillStaffPeriod>());
        }



        [Test]
        public void VerifyIncomingAndDistributedDifference()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromHours(2).TotalSeconds);
            DateTimePeriod period1 = new DateTimePeriod(new DateTime(2009, 02, 02, 7, 0, 0, DateTimeKind.Utc),
                                                        new DateTime(2009, 02, 02, 8, 0, 0, DateTimeKind.Utc));
            DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period3 = period2.MovePeriod(TimeSpan.FromHours(1));
            DateTimePeriod period4 = period3.MovePeriod(TimeSpan.FromHours(1));
            ServiceAgreement ag1 = new ServiceAgreement(level1, new Percent(1), new Percent(2));

            stPeriod1 = new SkillStaffPeriod(period1, _task, ag1, svc);
            stPeriod2 = new SkillStaffPeriod(period2, _task, ag1, svc);
            stPeriod3 = new SkillStaffPeriod(period3, _task, ag1, svc);
            stPeriod4 = new SkillStaffPeriod(period4, _task, ag1, svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			
            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(2.5d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(2d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(2d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(4);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }

            using (mocks.Playback())
            {
                calculateStaff(new List<ISkillStaffPeriod> { stPeriod1, stPeriod2, stPeriod3, stPeriod4 });

                stPeriod2.SetCalculatedResource65(5.5);
                Assert.AreEqual(2, stPeriod1.ScheduledAgentsIncoming);
                Assert.AreEqual(3.5, stPeriod2.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod3.ScheduledAgentsIncoming);
                Assert.AreEqual(0, stPeriod4.ScheduledAgentsIncoming);

                Assert.AreEqual(0, stPeriod1.IncomingDifference);
                Assert.AreEqual(1.5, stPeriod2.IncomingDifference);
                Assert.AreEqual(-2.5, stPeriod3.IncomingDifference);
                Assert.AreEqual(0, stPeriod4.IncomingDifference);

                //Assert.AreEqual(0, stPeriod1.DistributedDifference);
                //Assert.AreEqual(0.75, stPeriod2.DistributedDifference);
                //Assert.AreEqual(-0.5, stPeriod3.DistributedDifference);
                //Assert.AreEqual(-1.25, stPeriod4.DistributedDifference);
            }
        }

        #endregion


        [Test]
        public void VerifyAggregateProperties()
        {
            _aggregateSkillStaffPeriod.IsAggregate = true;
            _aggregateSkillStaffPeriod.AggregatedFStaff = 4.200d;
            _aggregateSkillStaffPeriod.AggregatedCalculatedResource = 6.300d;
            Assert.IsTrue(_aggregateSkillStaffPeriod.IsAggregate);
            Assert.AreEqual(4.2d, _aggregateSkillStaffPeriod.AggregatedFStaff);
            Assert.AreEqual(4.2d, _target.FStaff);
            Assert.IsNaN(_aggregateSkillStaffPeriod.AggregatedCalculatedLoggedOn);
            Assert.IsNaN(_target.CalculatedLoggedOn);
            Assert.AreEqual(6.3d, _target.CalculatedResource);
            Assert.AreEqual(6.3d, _aggregateSkillStaffPeriod.AggregatedCalculatedResource);
            Assert.AreEqual(2.1d, _target.AbsoluteDifference, 0.00001);
            Assert.AreEqual(0.5d, _target.RelativeDifference, 0.00001);
        }

        [Test]
        public void VerifyCombineAggregatedSkillStaffPeriod()
        {
            _aggregateSkillStaffPeriod.IsAggregate = true;
            ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            IAggregateSkillStaffPeriod aggregateSkillStaffPeriod2 = (IAggregateSkillStaffPeriod)target2;
            aggregateSkillStaffPeriod2.IsAggregate = true;
            _aggregateSkillStaffPeriod.AggregatedFStaff = 4;
            _aggregateSkillStaffPeriod.AggregatedCalculatedResource = 6;
            _aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand = 10;
            aggregateSkillStaffPeriod2.AggregatedFStaff = 1;
            aggregateSkillStaffPeriod2.AggregatedCalculatedResource = 2;
            aggregateSkillStaffPeriod2.AggregatedForecastedIncomingDemand = 12;
            _aggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(aggregateSkillStaffPeriod2);
            Assert.AreEqual(5, _aggregateSkillStaffPeriod.AggregatedFStaff);
            Assert.AreEqual(8, _aggregateSkillStaffPeriod.AggregatedCalculatedResource);
            Assert.AreEqual(22, _aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand);
        }

        [Test]
        public void VerifyCombineAggregatedEstimatedServiceLevel()
        {
            ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            ISkillStaffPeriod target3 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            IAggregateSkillStaffPeriod aggregateSkillStaffPeriod2 = (IAggregateSkillStaffPeriod)target2;
            IAggregateSkillStaffPeriod aggregateSkillStaffPeriod3 = (IAggregateSkillStaffPeriod)target3;
            _aggregateSkillStaffPeriod.IsAggregate = true;
            aggregateSkillStaffPeriod2.IsAggregate = true;
            aggregateSkillStaffPeriod3.IsAggregate = true;
            _aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand = 100;
            aggregateSkillStaffPeriod2.AggregatedForecastedIncomingDemand = 50;
            aggregateSkillStaffPeriod3.AggregatedForecastedIncomingDemand = 10;
            aggregateSkillStaffPeriod2.AggregatedEstimatedServiceLevel = new Percent(1);
            aggregateSkillStaffPeriod3.AggregatedEstimatedServiceLevel = new Percent(0);
            _aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevel = new Percent(0.5);
            _aggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(aggregateSkillStaffPeriod2);

            Assert.AreEqual(new Percent(0.66666666667).Value, _aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevel.Value, 0.00001);
            _aggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(aggregateSkillStaffPeriod3);

            Assert.AreEqual(new Percent(0.625).Value, _aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevel.Value, 0.00001);

        }

        [Test]
        public void VerifyCombineStaffingThreshold()
        {
            stPeriod1 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            stPeriod2 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            var agg = (IAggregateSkillStaffPeriod) stPeriod1;
            var period = (IAggregateSkillStaffPeriod) stPeriod2;
            agg.IsAggregate = true;
            period.IsAggregate = true;
            agg.AggregatedStaffingThreshold = StaffingThreshold.Ok;
            period.AggregatedStaffingThreshold = StaffingThreshold.Ok;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(StaffingThreshold.Ok, agg.AggregatedStaffingThreshold);

            agg.AggregatedStaffingThreshold = StaffingThreshold.Overstaffing;
            period.AggregatedStaffingThreshold = StaffingThreshold.CriticalUnderstaffing;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(StaffingThreshold.CriticalUnderstaffing, agg.AggregatedStaffingThreshold);
        }

        [Test]
        public void VerifyCombineMinMaxStaffingAlarm()
        {
            stPeriod1 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            stPeriod2 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            var agg = (IAggregateSkillStaffPeriod) stPeriod1;
            var period = (IAggregateSkillStaffPeriod)stPeriod2;
            agg.IsAggregate = true;
            period.IsAggregate = true;

            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.Ok, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.MinStaffBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.MaxStaffBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);

            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.MinStaffBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.MinStaffBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);

            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.MaxStaffBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.MaxStaffBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);

            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.Ok;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MinStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.MaxStaffBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);
            agg.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            period.AggregatedMinMaxStaffAlarm = MinMaxStaffBroken.BothBroken;
            agg.CombineAggregatedSkillStaffPeriod(period);
            Assert.AreEqual(MinMaxStaffBroken.BothBroken, agg.AggregatedMinMaxStaffAlarm);
        }

        [Test, ExpectedException(typeof(InvalidCastException))]
        public void VerifyCombineAggregatedSkillStaffPeriodThrowsExceptionIfInstanceIsNotAggregate()
        {
            ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            IAggregateSkillStaffPeriod aggregateSkillStaffPeriod2 = (IAggregateSkillStaffPeriod)target2;
            aggregateSkillStaffPeriod2.CombineAggregatedSkillStaffPeriod(_aggregateSkillStaffPeriod);
        }

        [Test, ExpectedException(typeof(InvalidCastException))]
        public void VerifyCombineAggregatedSkillStaffPeriodThrowsExceptionIfParameterIsNotAggregate()
        {
            ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa, _staffingCalculatorService);
            IAggregateSkillStaffPeriod aggregateSkillStaffPeriod2 = (IAggregateSkillStaffPeriod)target2;
            _aggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(aggregateSkillStaffPeriod2);
        }

        private static void calculateStaff(List<ISkillStaffPeriod> list)
        {
            for (int index = list.Count - 1; index >= 0; index--)
            {
                list[index].CalculateStaff(list.GetRange(index, list.Count - index));
            }
        }

        private List<ISkillStaffPeriod> GetPeriodlistWith7Periods()
        {
            IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromDays(4).TotalSeconds);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             svc);
            stPeriod2 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 2, 2000, 1, 3),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             svc);
            stPeriod3 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 3, 2000, 1, 4),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             svc);
            stPeriod4 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 4, 2000, 1, 5),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             svc);

            stPeriod5 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 5, 2000, 1, 6),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             svc);

            stPeriod6 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 6, 2000, 1, 7),
                                             _task,
                                             ServiceAgreement.DefaultValues(), svc);

            stPeriod7 = new SkillStaffPeriod(new DateTimePeriod(2000, 1, 7, 2000, 1, 8),
                                             _task,
                                             ServiceAgreement.DefaultValues(), svc);
			stPeriod1.SetSkillDay(_skillDay);
			stPeriod2.SetSkillDay(_skillDay);
			stPeriod3.SetSkillDay(_skillDay);
			stPeriod4.SetSkillDay(_skillDay);
			stPeriod5.SetSkillDay(_skillDay);
			stPeriod6.SetSkillDay(_skillDay);
			stPeriod7.SetSkillDay(_skillDay);

            List<ISkillStaffPeriod> periodlist = new List<ISkillStaffPeriod>
                                                     {
                                                         stPeriod1,
                                                         stPeriod2,
                                                         stPeriod3,
                                                         stPeriod4,
                                                         stPeriod5,
                                                         stPeriod6,
                                                         stPeriod7
                                                     };

            using (mocks.Record())
            {
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(0d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(4d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(4d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(4d);
				Expect.Call(svc.AgentsUseOccupancy(1, 1, 1, 1, new TimeSpan(), 2, 2, 1)).IgnoreArguments()
                    .Return(4d);
                Expect.Call(svc.Utilization(1, 1, 1, TimeSpan.MinValue)).IgnoreArguments().Return(1d).Repeat.Times(7);
                Expect.Call(svc.ServiceLevelAchieved(1, 1, 1, 1, TimeSpan.FromMinutes(1), 1)).IgnoreArguments().Repeat.Any().Return(7);
            }
            return periodlist;
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifySplitSkillStaffPeriodWithLowerPeriodLength()
        {
            // try to split 30 minutes in 60, can't be done
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromMinutes(4).TotalSeconds);
            DateTime start = new DateTime(2009, 02, 02, 09, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 30, 0, DateTimeKind.Utc);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(start, end),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             _staffingCalculatorService);

            stPeriod1.Split(new TimeSpan(1, 0, 0));
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifySplitSkillStaffPeriodWithUnevenPeriodLengths()
        {
            // try to split 35 minutes in 15 can't be done
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromMinutes(4).TotalSeconds);
            DateTime start = new DateTime(2009, 02, 02, 09, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 35, 0, DateTimeKind.Utc);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(start, end),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             _staffingCalculatorService);

            stPeriod1.Split(new TimeSpan(0, 15, 0));
        }

        [Test, Ignore("Payload have no longer a setter")]
        public void VerifySplitSkillStaffPeriod()
        {
            //IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromMinutes(4).TotalSeconds);
            // create a period that is 30 minutes and expect to be splitted in two 15 minutes
            DateTime start = new DateTime(2009,02,02,09,0,0,DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 30, 0, DateTimeKind.Utc);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(start, end),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             _staffingCalculatorService);
            ISkillStaff skillStaff = mocks.StrictMock<ISkillStaff>();
            

            using (mocks.Record())
            {
                Expect.Call(skillStaff.CalculatedResource).Return(5).Repeat.AtLeastOnce();
                Expect.Call(skillStaff.CalculatedTrafficIntensityWithShrinkage).Return(7).Repeat.AtLeastOnce();
                Expect.Call(skillStaff.ForecastedIncomingDemand).Return(9).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
							//Payload have no longer a setter
                //stPeriod1.Payload = skillStaff;
                IList<ISkillStaffPeriodView> views = stPeriod1.Split(new TimeSpan(0, 15, 0));
                Assert.AreEqual(2, views.Count);
                Assert.AreEqual(5, views[0].CalculatedResource);
                Assert.AreEqual(7, views[0].ForecastedIncomingDemandWithShrinkage);
                Assert.AreEqual(9, views[0].ForecastedIncomingDemand);
                Assert.AreEqual(15,views[0].Period.ElapsedTime().TotalMinutes);
                Assert.AreEqual(start, views[0].Period.StartDateTime);
                Assert.AreEqual(5, views[1].CalculatedResource);
                Assert.AreEqual(7, views[1].ForecastedIncomingDemandWithShrinkage);
                Assert.AreEqual(9, views[1].ForecastedIncomingDemand);
                Assert.AreEqual(15, views[1].Period.ElapsedTime().TotalMinutes);
                Assert.AreEqual(start.AddMinutes(15), views[1].Period.StartDateTime);
            }
            
        }

				[Test, Ignore("Payload have no longer a setter")]
        public void VerifySplitSkillStaffPeriodWhenSameLength()
        {
            //IStaffingCalculatorService svc = mocks.StrictMock<IStaffingCalculatorService>();
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromMinutes(4).TotalSeconds);
            // create a period that is 30 minutes and expect to be "splitted" in one 30 minutes
            DateTime start = new DateTime(2009, 02, 02, 09, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 30, 0, DateTimeKind.Utc);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(start, end),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)),
                                             _staffingCalculatorService);
            ISkillStaff skillStaff = mocks.StrictMock<ISkillStaff>();

            using (mocks.Record())
            {
                Expect.Call(skillStaff.CalculatedResource).Return(5).Repeat.AtLeastOnce();
                Expect.Call(skillStaff.CalculatedTrafficIntensityWithShrinkage).Return(5).Repeat.AtLeastOnce();
                Expect.Call(skillStaff.ForecastedIncomingDemand).Return(9).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
							//Payload have no longer a setter
                //stPeriod1.Payload = skillStaff;
                IList<ISkillStaffPeriodView> views = stPeriod1.Split(new TimeSpan(0, 30, 0));
                Assert.AreEqual(1, views.Count);
                Assert.AreEqual(5, views[0].CalculatedResource);
            }

        }
    }


    
}
