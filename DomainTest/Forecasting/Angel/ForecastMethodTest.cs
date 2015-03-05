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
	public class IndexVolumesFactory
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

		public static IVolumeYear[] Create()
		{
			return new[] {CreateMonthOfYear(), CreateWeekOfMonth(), CreateDayOfWeek()};
		}
	}

	[TestFixture]
	public class ForecastMethodTest
	{
        private ForecastMethod target;
        private TaskOwnerPeriod historicalData;
		private double _averageTasks;

        [SetUp]
        public void Setup()
        {
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);
			historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

	        var indexVolumes = MockRepository.GenerateMock<IIndexVolumes>();
			var volumes = IndexVolumesFactory.Create();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			_averageTasks = historicalData.TotalStatisticCalculatedTasks / historicalData.TaskOwnerDayCollection.Count;

			target = new ForecastMethod(indexVolumes);
		}

		[Test]
		public void ShouldForecastTasksUsingIndexesCorrectly()
		{
			const double indexMonth = 1d;
			const double indexWeek = 1.1d;
			const double indexDay = 1.2d;

			const double totalIndex = indexMonth * indexWeek * indexDay;
			var tasks = totalIndex * _averageTasks;

			var result = target.Forecast(historicalData, new DateOnlyPeriod(new DateOnly(2014, 1, 1), new DateOnly(2014, 1, 1)));
			result.Single().Tasks.Should().Be.EqualTo(Math.Round(tasks, 4));
		}
	}
}