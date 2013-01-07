using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class ReforecastPercentCalculatorTest
	{
		private MockRepository _mocks;
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
			_mocks = new MockRepository();

			 _period = new DateTimePeriod(_date, _date.AddMinutes(15));
			 _workloadDay = _mocks.DynamicMock<IWorkloadDay>();
			 _period1 = _mocks.StrictMock<ITemplateTaskPeriod>();
			 _period2 = _mocks.StrictMock<ITemplateTaskPeriod>();
			 _period3 = _mocks.StrictMock<ITemplateTaskPeriod>();
			_target = new ReforecastPercentCalculator();
		}

		[Test]
		public void ShouldCompareStatisticTaskAndForecasted()
		{
			var statTask = new StatisticTask{StatOfferedTasks = 20};
			Expect.Call(_workloadDay.SortedTaskPeriodList).Return(
				new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> {_period1, _period2, _period3}));
			Expect.Call(_period1.Period).Return(_period);
			Expect.Call(_period2.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(15)));
			Expect.Call(_period3.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(30)));
			Expect.Call(_period1.Tasks).Return(10);
			Expect.Call(_period2.Tasks).Return(10);
			Expect.Call(_period3.Tasks).Return(10);
			Expect.Call(_period1.StatisticTask).Return(statTask);
			Expect.Call(_period2.StatisticTask).Return(statTask);
			Expect.Call(_period3.StatisticTask).Return(statTask);
			_mocks.ReplayAll();

			var result = _target.Calculate(_workloadDay,_date.AddMinutes(45));
			Assert.That(result,Is.EqualTo(2.0));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCompareStatisticTaskAndForecastedAgain()
		{
			
			var statTask = new StatisticTask { StatOfferedTasks = 10 };
			Expect.Call(_workloadDay.SortedTaskPeriodList).Return(
				new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { _period1, _period2, _period3 }));
			Expect.Call(_period1.Period).Return(_period).Repeat.AtLeastOnce();
			Expect.Call(_period2.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(15))).Repeat.AtLeastOnce();
			Expect.Call(_period3.Period).Return(_period.ChangeEndTime(TimeSpan.FromMinutes(30))).Repeat.AtLeastOnce();
			
			Expect.Call(_period1.Tasks).Return(20);
			Expect.Call(_period2.Tasks).Return(20);
			Expect.Call(_period3.Tasks).Return(20);
			Expect.Call(_period1.StatisticTask).Return(statTask);
			Expect.Call(_period2.StatisticTask).Return(statTask);
			Expect.Call(_period3.StatisticTask).Return(statTask);
			_mocks.ReplayAll();

			var result = _target.Calculate(_workloadDay, _date.AddMinutes(45));
			Assert.That(result, Is.EqualTo(0.5));
			_mocks.VerifyAll();
		}
	}

	
}