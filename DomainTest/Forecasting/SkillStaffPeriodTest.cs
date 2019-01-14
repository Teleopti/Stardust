using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class SkillStaffPeriodTest
    {
        private SkillStaffPeriod _target;
        private DateTimePeriod _tp;
        private ITask _task;
        private ServiceAgreement _sa;
        private DateTime _dt = new DateTime(2008, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        private IAggregateSkillStaffPeriod _aggregateSkillStaffPeriod;
        private ISkillStaffPeriod stPeriod1;
        private ISkillStaffPeriod stPeriod2;
        private PopulationStatisticsCalculatedValues _populationStatisticsCalculatedValues;
	    private ISkill _skill;
	    private ISkillDay _skillDay;
	    private IList<int> _intraIntervalSamples;
	    private double _intraIntervalValue;
			
		[SetUp]
        public void Setup()
        {
            _tp = new DateTimePeriod(_dt.Add(TimeSpan.FromHours(10)), _dt.Add(TimeSpan.FromHours(11)));
            _task = new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20));
            _sa = new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20), new Percent(0), new Percent(1));
            _target = new SkillStaffPeriod(_tp, _task, _sa);
			
            _target.IsAvailable = true;
            _target.SetCalculatedResource65(123);
            _target.Payload.CalculatedLoggedOn = 321;
            _aggregateSkillStaffPeriod = _target;
            _populationStatisticsCalculatedValues = new PopulationStatisticsCalculatedValues(0,0);
		    
			_skill = SkillFactory.CreateSkill("name", SkillTypeFactory.CreateSkillTypePhone(), 15);
		    _skillDay = SkillDayFactory.CreateSkillDay(_skill, DateOnly.Today);
			_target.SetSkillDay(_skillDay);

			_intraIntervalSamples = new List<int> {1, 2};
			_intraIntervalValue = 0.5;
			_target.IntraIntervalSamples = _intraIntervalSamples;
			_target.IntraIntervalValue = _intraIntervalValue;
			_target.HasIntraIntervalIssue = true;

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
            Assert.IsNotNull(_target.EstimatedServiceLevelShrinkage);
            Assert.IsNotNull(_target.ActualServiceLevel);
			Assert.AreEqual(_intraIntervalSamples, _target.IntraIntervalSamples);
			Assert.AreEqual(_intraIntervalValue, _target.IntraIntervalValue);
			Assert.IsTrue(_target.HasIntraIntervalIssue);

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

				_populationStatisticsCalculatedValues = new PopulationStatisticsCalculatedValues(1,2);
                _target.SetDistributionValues(_populationStatisticsCalculatedValues, periodDistribution);
                
                Assert.AreNotEqual(0, _target.IntraIntervalDeviation);
            }
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
            
            _sa = new ServiceAgreement(new ServiceLevel(new Percent(0.8), 20), new Percent(0), new Percent(100));
            _target = new SkillStaffPeriod(_tp, taskWithLongAfterTalk, _sa)
	            {
		            IsAvailable = true
	            };
			_target.SetSkillDay(_skillDay);
	        _target.SetCalculatedResource65(20);
            _target.Payload.CalculatedLoggedOn = 321;
            _target.Payload.Efficiency = new Percent(1);

	        var serviceLevel = _target.StaffingCalculatorService.ServiceLevelAchievedOcc(_target.Payload.CalculatedResource,
		        _target.Payload.ServiceAgreementData.ServiceLevel.Seconds,
		        _target.Payload.TaskData.Tasks,
		        _target.Payload.TaskData.AverageTaskTime.TotalSeconds +
		        _target.Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
		        _target.Period.ElapsedTime(),
				(int)_target.Payload.ServiceAgreementData.ServiceLevel.Percent.Value * 100,
		        _target.Payload.ForecastedIncomingDemandWithoutShrinkage, 1,
		        0);
			
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
            ISkillStaffPeriod sp1 = new SkillStaffPeriod(_tp, task1, sa1);
            ISkillStaffPeriod sp2 = new SkillStaffPeriod(_tp, task2, sa1);

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
            SkillStaffPeriod sp1 = new SkillStaffPeriod(_tp, task1, sa1);

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
            ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa);
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
		public void VerifyCombineAggregatedEstimatedServiceLevelShrinkage()
		{
			ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa);
			ISkillStaffPeriod target3 = new SkillStaffPeriod(_tp, _task, _sa);
			IAggregateSkillStaffPeriod aggregateSkillStaffPeriod2 = (IAggregateSkillStaffPeriod)target2;
			IAggregateSkillStaffPeriod aggregateSkillStaffPeriod3 = (IAggregateSkillStaffPeriod)target3;
			_aggregateSkillStaffPeriod.IsAggregate = true;
			aggregateSkillStaffPeriod2.IsAggregate = true;
			aggregateSkillStaffPeriod3.IsAggregate = true;
			_aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand = 100;
			aggregateSkillStaffPeriod2.AggregatedForecastedIncomingDemand = 50;
			aggregateSkillStaffPeriod3.AggregatedForecastedIncomingDemand = 10;
			aggregateSkillStaffPeriod2.AggregatedEstimatedServiceLevelShrinkage = new Percent(1);
			aggregateSkillStaffPeriod3.AggregatedEstimatedServiceLevelShrinkage = new Percent(0);
			_aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevelShrinkage = new Percent(0.5);
			_aggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(aggregateSkillStaffPeriod2);

			Assert.AreEqual(new Percent(0.66666666667).Value, _aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevelShrinkage.Value, 0.00001);
			_aggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(aggregateSkillStaffPeriod3);

			Assert.AreEqual(new Percent(0.625).Value, _aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevelShrinkage.Value, 0.00001);

		}

        [Test]
        public void VerifyCombineStaffingThreshold()
        {
            stPeriod1 = new SkillStaffPeriod(_tp, _task, _sa);
            stPeriod2 = new SkillStaffPeriod(_tp, _task, _sa);
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
            stPeriod1 = new SkillStaffPeriod(_tp, _task, _sa);
            stPeriod2 = new SkillStaffPeriod(_tp, _task, _sa);
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

        [Test]
        public void VerifyCombineAggregatedSkillStaffPeriodThrowsExceptionIfInstanceIsNotAggregate()
        {
            ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa);
            IAggregateSkillStaffPeriod aggregateSkillStaffPeriod2 = (IAggregateSkillStaffPeriod)target2;
			Assert.Throws<InvalidCastException>(() => aggregateSkillStaffPeriod2.CombineAggregatedSkillStaffPeriod(_aggregateSkillStaffPeriod));
        }

        [Test]
        public void VerifyCombineAggregatedSkillStaffPeriodThrowsExceptionIfParameterIsNotAggregate()
        {
            ISkillStaffPeriod target2 = new SkillStaffPeriod(_tp, _task, _sa);
            IAggregateSkillStaffPeriod aggregateSkillStaffPeriod2 = (IAggregateSkillStaffPeriod)target2;
			Assert.Throws<InvalidCastException>(() => _aggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(aggregateSkillStaffPeriod2));
        }

        
        [Test]
        public void VerifySplitSkillStaffPeriodWithLowerPeriodLength()
        {
            // try to split 30 minutes in 60, can't be done
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromMinutes(4).TotalSeconds);
            DateTime start = new DateTime(2009, 02, 02, 09, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 30, 0, DateTimeKind.Utc);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(start, end),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)));

			Assert.Throws<ArgumentOutOfRangeException>(() => stPeriod1.Split(new TimeSpan(1, 0, 0)));
        }

        [Test]
        public void VerifySplitSkillStaffPeriodWithUnevenPeriodLengths()
        {
            // try to split 35 minutes in 15 can't be done
            ServiceLevel level1 = new ServiceLevel(new Percent(1), TimeSpan.FromMinutes(4).TotalSeconds);
            DateTime start = new DateTime(2009, 02, 02, 09, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2009, 02, 02, 09, 35, 0, DateTimeKind.Utc);

            stPeriod1 = new SkillStaffPeriod(new DateTimePeriod(start, end),
                                             _task,
                                             new ServiceAgreement(level1, new Percent(1),
                                                                  new Percent(2)));

			Assert.Throws<ArgumentOutOfRangeException>(() => stPeriod1.Split(new TimeSpan(0, 15, 0)));
        }
    }
}
