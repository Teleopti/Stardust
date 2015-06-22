using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	[TestFixture]
	public class TeleoptiClassicUpdatedBaseTest
	{
		[Test]
		public void ShouldForecastAhtAndAcwCorrectly()
		{
			var skill = SkillFactory.CreateSkill("testSkill");
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var workload = WorkloadFactory.CreateWorkload(skill);

			var historicalDate = new DateOnly(2006, 1, 1);
			var periodForHelper = SkillDayFactory.GenerateMockedStatistics(historicalDate, workload);
			var historicalData = new TaskOwnerPeriod(historicalDate, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);

			var indexVolumes = MockRepository.GenerateMock<IIndexVolumes>();
			var volumes = IndexVolumesFactory.CreateDayWeekMonthIndexVolumes();
			indexVolumes.Stub(x => x.Create(historicalData)).Return(volumes);

			var simpleAhtAndAcwCalculator = MockRepository.GenerateMock<IAhtAndAcwCalculator>();
			var acw = new TimeSpan(502);
			var aht = new TimeSpan(503);
			simpleAhtAndAcwCalculator.Stub(x => x.Recent3MonthsAverage(historicalData)).Return(new AhtAndAcw
			{
				Acw = acw,
				Aht = aht
			});
			var target = new TeleoptiClassicUpdatedBaseFake(indexVolumes, simpleAhtAndAcwCalculator);

			var result = target.Forecast(historicalData, new DateOnlyPeriod(new DateOnly(2014, 1, 1), new DateOnly(2014, 1, 1)));
			result.ForecastingTargets.Single().AverageAfterTaskTime.Should().Be.EqualTo(acw);
			result.ForecastingTargets.Single().AverageTaskTime.Should().Be.EqualTo(aht);
		}
	}
}