using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastControllerTest
	{
		[Test]
		public void ShouldGetSkillsAndWorkloads()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill1});
			var target = new ForecastController(null, skillRepository, null, null);
			var skills = target.Skills();
			skills.Single().Id.Should().Be.EqualTo(skill1.Id.Value);
			skills.Single().Name.Should().Be.EqualTo(skill1.Name);
			var workload = skill1.WorkloadCollection.Single();
			skills.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			skills.Single().Workloads.Single().Name.Should().Be.EqualTo(workload.Name);
		}

		[Test]
		public void ShouldEvaluate()
		{
			var forecastEvaluator = MockRepository.GenerateMock<IForecastEvaluator>();
			var evaluateInput = new EvaluateInput();
			var workloadForecastingViewModel = new WorkloadForecastViewModel();
			forecastEvaluator.Stub(x => x.Evaluate(evaluateInput)).Return(workloadForecastingViewModel);
			var target = new ForecastController(null, null, forecastEvaluator, null);

			var result = target.Evaluate(evaluateInput);

			result.Result.Should().Be.EqualTo(workloadForecastingViewModel);
		}


		[Test]
		public void ShouldGetQueueStatistics()
		{
			var forecastEvaluator = MockRepository.GenerateMock<IForecastEvaluator>();
			var queueStatisticsInput = new QueueStatisticsInput();
			var workloadQueueStatisticsViewModel = new WorkloadQueueStatisticsViewModel();
			forecastEvaluator.Stub(x => x.QueueStatistics(queueStatisticsInput)).Return(workloadQueueStatisticsViewModel);
			var target = new ForecastController(null, null, forecastEvaluator, null);

			var result = target.QueueStatistics(queueStatisticsInput);

			result.Result.Should().Be.EqualTo(workloadQueueStatisticsViewModel);
		}

		[Test]
		public void ShouldGetForecastResult()
		{
			var workloadId = Guid.NewGuid();
			var forecastStart = new DateTime(2014,4,1);
			var forecastEnd = new DateTime(2014,4,29);
			var forecastResultViewModelFactory = MockRepository.GenerateMock<IForecastResultViewModelFactory>();
			var workloadForecastResultViewModel = new WorkloadForecastResultViewModel();
			forecastResultViewModelFactory.Stub(
				x => x.Create(workloadId, new DateOnlyPeriod(new DateOnly(forecastStart), new DateOnly(forecastEnd))))
				.Return(workloadForecastResultViewModel);
			var target = new ForecastController(null, null, null, forecastResultViewModelFactory);

			var forecastResultInput = new ForecastResultInput
			{
				WorkloadId = workloadId,
				ForecastStart = forecastStart,
				ForecastEnd = forecastEnd
			};
			var result = target.ForecastResult(forecastResultInput);
			result.Result.Should().Be.EqualTo(workloadForecastResultViewModel);
		}
	}

	
}
