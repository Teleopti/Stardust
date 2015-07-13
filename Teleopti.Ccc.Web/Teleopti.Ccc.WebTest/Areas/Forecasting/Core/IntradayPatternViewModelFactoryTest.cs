using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Core
{
	public class IntradayPatternViewModelFactoryTest
	{
		[Test]
		public void ShouldCreateViewModel()
		{
			
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var input = new IntradayPatternInput
			{
				WorkloadId = workload.Id.Value
			};
			workloadRepository.Stub(x => x.Get(input.WorkloadId)).Return(workload);
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			var templatePeriod = new DateOnlyPeriod(2015, 1, 1, 2015, 3, 31);
			historicalPeriodProvider.Stub(x => x.AvailableIntradayTemplatePeriod(workload)).Return(templatePeriod);
			var intradayForecaster = MockRepository.GenerateMock<IIntradayForecaster>();
			var dateTimePeriod = new DateTimePeriod(new DateTime(1800, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(1800, 1, 1, 8, 15, 0, DateTimeKind.Utc));
			intradayForecaster.Stub(x => x.CalculatePattern(workload, templatePeriod))
				.Return(new Dictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>>
				{
					{DayOfWeek.Monday, new List<ITemplateTaskPeriod>(){new TemplateTaskPeriod(new Task(150,new TimeSpan(0,0,15), new TimeSpan(0,0,4)),dateTimePeriod )}},
					{DayOfWeek.Tuesday, new List<ITemplateTaskPeriod>()},
					{DayOfWeek.Wednesday, new List<ITemplateTaskPeriod>()},
					{DayOfWeek.Thursday, new List<ITemplateTaskPeriod>()},
					{DayOfWeek.Friday, new List<ITemplateTaskPeriod>()},
					{DayOfWeek.Saturday, new List<ITemplateTaskPeriod>()},
					{DayOfWeek.Sunday, new List<ITemplateTaskPeriod>()}
				});
			var target = new IntradayPatternViewModelFactory(intradayForecaster, workloadRepository, historicalPeriodProvider);
			
			var result = target.Create(input);

			result.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.WeekDays.Second().DayOfWeek.Should().Be.EqualTo(DayOfWeek.Monday);
			result.WeekDays.Second().Tasks.First().Should().Be.EqualTo(150);
			var localPeriod = dateTimePeriod.TimePeriod(workload.Skill.TimeZone);
			result.WeekDays.Second().Periods.First().Should().Be.EqualTo(localPeriod.ToShortTimeString());

		}
	}
}