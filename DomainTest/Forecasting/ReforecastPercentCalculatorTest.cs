using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class ReforecastPercentCalculatorTest
	{
		private ReforecastPercentCalculator _target;
		private DateTime _date = new DateTime(2012, 12, 14, 10, 0, 0, DateTimeKind.Utc);
		private DateTimePeriod _period;
		private IWorkloadDay _workloadDay;
		private ITemplateTaskPeriod _period1;
		private ITemplateTaskPeriod _period2;
		private ITemplateTaskPeriod _period3;

		[SetUp]
		public void Setup()
		{
			_period = new DateTimePeriod(_date, _date.AddMinutes(15));
			_workloadDay = MockRepository.GenerateMock<IWorkloadDay>();
			_period1 = MockRepository.GenerateStrictMock<ITemplateTaskPeriod>();
			_period2 = MockRepository.GenerateStrictMock<ITemplateTaskPeriod>();
			_period3 = MockRepository.GenerateStrictMock<ITemplateTaskPeriod>();
			_target = new ReforecastPercentCalculator();
		}

		[Test]
		public void ShouldCompareStatisticTaskAndForecasted()
		{
			var statTask = new StatisticTask { StatOfferedTasks = 20 };
			_workloadDay.Stub(x => x.SortedTaskPeriodList).Return(
				new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { _period1, _period2, _period3 }));
			_period1.Stub(x => x.Period).Return(_period);
			_period2.Stub(x => x.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			_period3.Stub(x => x.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(30)));
			_period1.Stub(x => x.TotalTasks).Return(10);
			_period2.Stub(x => x.TotalTasks).Return(10);
			_period3.Stub(x => x.TotalTasks).Return(10);
			_period1.Stub(x => x.StatisticTask).Return(statTask);
			_period2.Stub(x => x.StatisticTask).Return(statTask);
			_period3.Stub(x => x.StatisticTask).Return(statTask);

			var result = _target.Calculate(_workloadDay, _date.AddMinutes(45));
			Assert.That(result, Is.EqualTo(2.0));
		}

		[Test]
		public void ShouldCompareStatisticTaskAndForecastedAgain()
		{
			var statTask = new StatisticTask { StatOfferedTasks = 10 };
			_workloadDay.Stub(x => x.SortedTaskPeriodList).Return(
				new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { _period1, _period2, _period3 }));
			_period1.Stub(x => x.Period).Return(_period).Repeat.AtLeastOnce();
			_period2.Stub(x => x.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(15))).Repeat.AtLeastOnce();
			_period3.Stub(x => x.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(30))).Repeat.AtLeastOnce();

			_period1.Stub(x => x.TotalTasks).Return(20);
			_period2.Stub(x => x.TotalTasks).Return(20);
			_period3.Stub(x => x.TotalTasks).Return(20);
			_period1.Stub(x => x.StatisticTask).Return(statTask);
			_period2.Stub(x => x.StatisticTask).Return(statTask);
			_period3.Stub(x => x.StatisticTask).Return(statTask);
			var result = _target.Calculate(_workloadDay, _date.AddMinutes(45));
			Assert.That(result, Is.EqualTo(0.5));
		}
	}
}