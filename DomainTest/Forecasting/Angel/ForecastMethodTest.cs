using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class IndexFactory
	{
		public static IVolumeYear CreateMonthOfYear()
		{
			var monthOfYear = MockRepository.GenerateMock<IVolumeYear>();
			monthOfYear.Stub(x => x.TaskIndex(Arg<DateOnly>.Is.Anything)).Return(1d);
			monthOfYear.Stub(x => x.AfterTalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1d);
			monthOfYear.Stub(x => x.TalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1d);
			return monthOfYear;
		}

		public static IVolumeYear CreateWeekOfMonth()
		{
			var weekOfMonth = MockRepository.GenerateMock<IVolumeYear>();
			weekOfMonth.Stub(x => x.TaskIndex(Arg<DateOnly>.Is.Anything)).Return(1.1d);
			weekOfMonth.Stub(x => x.AfterTalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.1d);
			weekOfMonth.Stub(x => x.TalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.1d);
			return weekOfMonth;
		}

		public static IVolumeYear CreateDayOfWeek()
		{
			var dayOfWeek = MockRepository.GenerateMock<IVolumeYear>();
			dayOfWeek.Stub(x => x.TaskIndex(Arg<DateOnly>.Is.Anything)).Return(1.2d);
			dayOfWeek.Stub(x => x.AfterTalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.2d);
			dayOfWeek.Stub(x => x.TalkTimeIndex(Arg<DateOnly>.Is.Anything)).Return(1.2d);
			return dayOfWeek;
		}
	}

	[TestFixture]
	public class ForecastMethodTest
	{
        private ForecastMethod target;
        private double _averageTasks;
        private IWorkload workload;
        private TaskOwnerPeriod historicalData;
        private readonly TimeZoneInfo _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
        private MockRepository mocks;
		private IList<IVolumeYear> volumes;
		private readonly IList<IForecastingTarget> _taskOwnerCollection = new List<IForecastingTarget>();

		//[SetUp]
		//public void Setup()
		//{
		//	ISkill skill = SkillFactory.CreateSkill("testSkill");
		//	skill.TimeZone = _timeZone;
		//	workload = WorkloadFactory.CreateWorkload(skill);
		//	mocks = new MockRepository();

		//	var date = new DateOnly(2008, 3, 31);
		//	var historicalDate = new DateOnly(2006, 1, 1);

		//	var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);

		//	var monthOfYear = IndexFactory.CreateMonthOfYear(date);
		//	var weekOfMonth = IndexFactory.CreateWeekOfMonth(date);
		//	var dayOfWeek = IndexFactory.CreateDayOfWeek(date);

		//	volumes = new List<IVolumeYear> {monthOfYear, weekOfMonth, dayOfWeek};
		//	var indexVolumes = MockRepository.GenerateMock<IIndexVolumes>();
		//	historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);
		//	indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

		//	_averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;

		//	target = new ForecastMethod(indexVolumes);
		//	target.Forecast(historicalData, new DateOnlyPeriod());
		//}
		
        [SetUp]
        public void Setup()
        {
			ISkill skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = _timeZone;
			workload = WorkloadFactory.CreateWorkload(skill);
			mocks = new MockRepository();

			var historicalDate = new DateOnly(2006, 1, 1);

			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);

			var monthOfYear = IndexFactory.CreateMonthOfYear();
			var weekOfMonth = IndexFactory.CreateWeekOfMonth();
			var dayOfWeek = IndexFactory.CreateDayOfWeek();
			
			volumes = new List<IVolumeYear> { monthOfYear, weekOfMonth, dayOfWeek };
			var indexVolumes = MockRepository.GenerateMock<IIndexVolumes>();
			historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			_averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;

			target = new ForecastMethod(indexVolumes);
		}

		[Test]
		public void Should()
		{
			var result = target.Forecast(historicalData, new DateOnlyPeriod(new DateOnly(2014, 1, 1), new DateOnly(2014, 1, 1)));
			result.Should().Not.Be.Null();
		}

		//[Test]
		//public void VerifyCreateWithoutHistoricDepth()
		//{
		//	historicalData = new TaskOwnerPeriod(historicalData.CurrentDate, new List<ITaskOwner>(), historicalData.TypeOfTaskOwnerPeriod);
		//	target.Forecast(historicalData, new DateOnlyPeriod());

		//	Assert.AreEqual(0, target.WorkloadDayCollection[2].Tasks);
		//	Assert.AreEqual(target.WorkloadDayCollection.Count, target.TotalDayItemCollection.Count);
		//}
	}
}