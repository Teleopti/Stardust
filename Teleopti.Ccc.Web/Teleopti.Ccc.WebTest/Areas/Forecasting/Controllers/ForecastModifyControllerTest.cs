using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;


namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastModifyControllerTest : IExtendSystem
	{
		public ForecastController Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeStatisticRepository StatisticRepository;
		public FakeWorkloadRepository WorkloadRepository;
 		private const double tolerance = 0.000001d;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ForecastController>();
		}

		[Test]
		public void ShouldAddOverride()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 100d,
						TotalAverageTaskTime = 30d,
						TotalAverageAfterTaskTime = 10d
					},
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 5),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 100d,
						TotalAverageTaskTime = 30d,
						TotalAverageAfterTaskTime = 10d
					}
				},
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 50d,
				OverrideAverageAfterTaskTime = 20d,
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			var lastForecastDay = ((List<ForecastDayModel>)result.ForecastDays).Last();

			forecastDay.TotalTasks
				.Should().Be(200d);
			forecastDay.TotalAverageTaskTime
				.Should().Be(50d);
			forecastDay.TotalAverageAfterTaskTime
				.Should().Be(20d);
			forecastDay.OverrideTasks
				.Should().Be(200d);
			forecastDay.HasOverride
				.Should().Be(true);
			forecastDay.IsInModification
				.Should().Be(true);

			lastForecastDay.TotalTasks
				.Should().Be(100d);
			lastForecastDay.TotalAverageTaskTime
				.Should().Be(30d);
			lastForecastDay.TotalAverageAfterTaskTime
				.Should().Be(10d);
			lastForecastDay.OverrideTasks
				.Should().Be(null);
			lastForecastDay.HasOverride
				.Should().Be(false);
			lastForecastDay.IsInModification
				.Should().Be(false);
			((string)result.WarningMessage).Should().Be.Empty();
		}

		[Test]
		public void ShouldClearOverrideValues()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 150d,
						TotalAverageTaskTime = 50d,
						TotalAverageAfterTaskTime = 20d
					}
				},
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			forecastDay.TotalTasks
				.Should().Be(forecastDay.Tasks);
			forecastDay.TotalAverageTaskTime
				.Should().Be(forecastDay.AverageTaskTime);
			forecastDay.TotalAverageAfterTaskTime
				.Should().Be(forecastDay.AverageAfterTaskTime);
			((string)result.WarningMessage).Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAddOverrideOnClosedDay()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = false,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 100d,
						TotalAverageTaskTime = 30d,
						TotalAverageAfterTaskTime = 10d
					}
				},
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 50d,
				OverrideAverageAfterTaskTime = 20d,
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			forecastDay.TotalTasks
				.Should().Be(100d);
			forecastDay.TotalAverageTaskTime
				.Should().Be(30d);
			forecastDay.TotalAverageAfterTaskTime
				.Should().Be(10d);
			forecastDay.HasOverride
				.Should().Be(false);
		}

		[Test]
		public void ShouldClearExistingCampaignWhenAddOverride()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						CampaignTasksPercentage = 0.5d,
						HasCampaign = true
					}
				},
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true,
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 50d,
				OverrideAverageAfterTaskTime = 20d,
			};

			dynamic data = Target.AddOverride(model);
			var result = data.Content;

			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			forecastDay.HasOverride.Should().Be(true);
			forecastDay.TotalTasks.Should().Be(200d);
			forecastDay.TotalAverageTaskTime.Should().Be(50d);
			forecastDay.TotalAverageAfterTaskTime.Should().Be(20d);

			forecastDay.HasCampaign.Should().Be(false);
			forecastDay.CampaignTasksPercentage.Should().Be(0);
			((string)result.WarningMessage).Should().Be(Resources.ClearCampaignWIthOverride);
		}

		[Test]
		public void ShouldUseExistingCampaignWhenClearingOverride()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						CampaignTasksPercentage = 0.5d,
						TotalTasks = 200d,
						TotalAverageTaskTime = 40d,
						TotalAverageAfterTaskTime = 20d
					}
				},
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			forecastDay.TotalTasks.Should().Be(150d);
			forecastDay.TotalAverageTaskTime.Should().Be(30d);
			forecastDay.TotalAverageAfterTaskTime.Should().Be(10d);
			forecastDay.HasCampaign.Should().Be(true);
			forecastDay.HasOverride.Should().Be(false);
		}

		[Test]
		public void ShouldAddCampaign()
		{
			var model = new CampaignInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				CampaignTasksPercent = 0.5d,
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						TotalTasks = 100d
					},
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 5),
						IsOpen = true,
						Tasks = 100d,
						TotalTasks = 100d
					}
				}
			};

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var firstForecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			var secondForecastDay = ((List<ForecastDayModel>)result.ForecastDays).Last();

			firstForecastDay.HasCampaign.Should().Be.True();
			firstForecastDay.IsInModification.Should().Be.True();
			firstForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(model.CampaignTasksPercent);
			firstForecastDay.Tasks.Should().Be(100d);
			firstForecastDay.TotalTasks.Should().Be(150d);

			secondForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(0);
			secondForecastDay.HasCampaign.Should().Be.False();
			secondForecastDay.IsInModification.Should().Be.False();
			secondForecastDay.Tasks.Should().Be(100d);
			secondForecastDay.TotalTasks.Should().Be(100d);
		}


		[Test]
		public void ShouldAddNegativeCampaign()
		{
			var model = new CampaignInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				CampaignTasksPercent = -0.5d,
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						TotalTasks = 100d
					},
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 5),
						IsOpen = true,
						Tasks = 100d,
						TotalTasks = 100d
					}
				}
			};

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var firstForecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			var secondForecastDay = ((List<ForecastDayModel>)result.ForecastDays).Last();

			firstForecastDay.HasCampaign.Should().Be.True();
			firstForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(model.CampaignTasksPercent);
			firstForecastDay.Tasks.Should().Be(100d);
			firstForecastDay.TotalTasks.Should().Be(50d);

			secondForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(0);
			secondForecastDay.HasCampaign.Should().Be.False();
			secondForecastDay.Tasks.Should().Be(100d);
			secondForecastDay.TotalTasks.Should().Be(100d);
		}

		[Test]
		public void ShouldNotAddCampaignWhenExistsOverride()
		{
			var forecast = new ForecastDayModel
			{
				Date = new DateOnly(2018, 5, 4),
				IsOpen = true,
				Tasks = 100d,
				AverageTaskTime = 30d,
				AverageAfterTaskTime = 10d,
				TotalTasks = 200d,
				TotalAverageTaskTime = 40d,
				TotalAverageAfterTaskTime = 20d,
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 40d,
				OverrideAverageAfterTaskTime = 20d,
				HasOverride = true
			};
			var model = new CampaignInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				ForecastDays = new List<ForecastDayModel> { forecast },
				CampaignTasksPercent = 0.5d
			};

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			Assert.That(forecastDay.TotalTasks, Is.EqualTo(200d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(40d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(20d).Within(tolerance));
			((string)result.WarningMessage).Should().Be(Resources.CampaignNotAppliedWIthExistingOverride);
		}

		[Test]
		public void ShouldNotAddCampaignForClosedDay()
		{
			var model = new CampaignInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				CampaignTasksPercent = 0.5d,
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = false,
						Tasks = 100d
					}
				}
			};

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var firstForecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			firstForecastDay.Tasks
				.Should().Be(100d);
		}
	}
}