﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastControllerTest
	{
		[Test]
		public void ShouldMeasureForecast()
		{
			var quickForecastEvaluator = MockRepository.GenerateMock<IQuickForecastEvaluator>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] { skill1 });
			var workload = skill1.WorkloadCollection.Single();
			quickForecastEvaluator.Stub(x => x.MeasureForecastForAllSkills())
				.Return(new[]
				{
					new SkillAccuracy
					{
						Id = skill1.Id.Value,
						Name = skill1.Name,
						Workloads = new []
						{
							new WorkloadAccuracy {
								Id = workload.Id.Value, 
								Name = workload.Name, 
								Accuracies = new []
							{
								new MethodAccuracy {Number = 90.2, MethodId = ForecastMethodType.TeleoptiClassic}
							}}
						}
					}
				});
			var target = new ForecastController(quickForecastEvaluator, null);

			var skills = target.MeasureForecastMethod();

			skills.Result.Single().Id.Should().Be.EqualTo(skill1.Id.Value);
			skills.Result.Single().Name.Should().Be.EqualTo(skill1.Name);
			skills.Result.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			skills.Result.Single().Workloads.Single().Name.Should().Be.EqualTo(workload.Name);

			skills.Result.Single().Workloads.Single().Accuracies.Single().Number.Should().Be.EqualTo(90.2);
			skills.Result.Single().Workloads.Single().Accuracies.Single().MethodId.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassic);
		}

		[Test]
		public void ShouldForecast()
		{
			var now = new Now();
			var expectedFuturePeriod = new DateOnlyPeriod(new DateOnly(now.UtcDateTime()), new DateOnly(now.UtcDateTime().AddYears(1)));
			var quickForecastCreator = MockRepository.GenerateMock<IQuickForecastCreator>();
			
			var target = new ForecastController(null, quickForecastCreator);
			var workloads = new[] {new ForecastWorkloadInput {WorkloadId = Guid.NewGuid()}};
			var result = target.Forecast(new QuickForecastInputModel
			{
				ForecastStart = expectedFuturePeriod.StartDate.Date,
				ForecastEnd = expectedFuturePeriod.EndDate.Date,
				Workloads = workloads
			});
			result.Result.Should().Be.True();
			quickForecastCreator.AssertWasCalled(x => x.CreateForecastForWorkloads(expectedFuturePeriod, workloads));
		}
	}

}
