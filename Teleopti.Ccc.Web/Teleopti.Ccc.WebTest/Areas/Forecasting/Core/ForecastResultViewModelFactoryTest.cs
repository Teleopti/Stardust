using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Core
{
	public class ForecastResultViewModelFactoryTest
	{
		IWorkload _workload;
		IWorkloadRepository _workloadRepository;
		Scenario _scenario;
		DateOnlyPeriod _futurePeriod;
		ISkillDayRepository _skillDayRepository;
		IFutureData _futureData;
		DateOnly _theDate;

		[Test]
		public void ShouldCreateForecastResultViewModel()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(), 12d,
				TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(400));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.Date.Should().Be.EqualTo(_theDate);
			Math.Round(firstDay.Tasks, 1).Should().Be.EqualTo(Math.Round(8.1d, 1));
			Math.Round(firstDay.TotalTasks, 1).Should().Be.EqualTo(Math.Round(12d, 1));
			firstDay.AverageTaskTime.Should().Be.EqualTo(100d);
			firstDay.TotalAverageTaskTime.Should().Be.EqualTo(300d);
			firstDay.AverageAfterTaskTime.Should().Be.EqualTo(200d);
			firstDay.TotalAverageAfterTaskTime.Should().Be.EqualTo(400d);
		}

		[Test]
		public void ShouldReturnViewModelsByDate()
		{
			stubForecastDataFor2Days(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(), 12d,
				TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(400));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.Date.Should().Be.EqualTo(_theDate);

			var secondDay = result.ForecastDays.Second();
			((object)secondDay.Date).Should().Be.EqualTo(_theDate.AddDays(1));
		}

		[Test]
		public void ShouldCreateViewModelWithCampaign()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(50), null, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.HasCampaign.Should().Be.True();
		}

		[Test]
		public void ShouldCreateViewModelWithNegativeCampaign()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(-50), null, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.HasCampaign.Should().Be.True();
		}

		[Test]
		public void ShouldCreateViewModelWithOverrideCalls()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(0), 500d, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.False();
		}

		[Test]
		public void ShouldCreateViewModelWithOverrideTalkTime()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(0), null, TimeSpan.FromSeconds(90), null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.False();
		}

		[Test]
		public void ShouldCreateViewModelWithOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(0), null, null, TimeSpan.FromSeconds(90));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.False();
		}

		[Test]
		public void ShouldCreateViewModelWhereCampaignIsMissing()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(75),
				400d, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.True();
		}

		[Test]
		public void ShouldCreateViewModelWhenHaveBothCampaignCallsAndOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), null, null, TimeSpan.FromSeconds(120));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.True();
		}

		[Test]
		public void ShouldCreateViewModelWhenHaveBothCampaignCallsAndOverrideCalls()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), 20.1d, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.True();
		}

		[Test]
		public void ShouldNotHaveOverrideInViewModelWhenHaveBothCampaignCallsAndOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), null, null, TimeSpan.FromSeconds(120));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);

			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);
			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.True();
		}

		[Test]
		public void ShouldNotHaveVcampaignInViewModelWhenHaveBothCampaignCallsAndOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), null, null, TimeSpan.FromSeconds(120));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);
			var firstDay = result.ForecastDays.First();
			firstDay.HasOverride.Should().Be.True();
			firstDay.HasCampaign.Should().Be.True();
		}

		private void stubForecastDataFor2Days(Task task, Percent campaignTasks, double? overrideTasks,
			TimeSpan? overrideTaskTime, TimeSpan? overrideAfterTaskTime)
		{
			_theDate = new DateOnly(2014, 3, 1);
			_scenario = new Scenario("s1").WithId();
			_futurePeriod = new DateOnlyPeriod(_theDate, _theDate);
			var skillDays = new List<ISkillDay>();

			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			_workload = skill.WorkloadCollection.Single();
			_workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			_workloadRepository.Stub(x => x.Get(_workload.Id.Value)).Return(_workload);
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_skillDayRepository.Stub(x => x.FindRange(_futurePeriod, skill, _scenario)).Return(skillDays);
			_futureData = MockRepository.GenerateMock<IFutureData>();

			var workload = new Workload(skill);
			var workloadDay1 = createWorkloadDay(_theDate, workload, task, campaignTasks, overrideTasks,
				overrideTaskTime, overrideAfterTaskTime);
			var workloadDay2 = createWorkloadDay(_theDate.AddDays(1), workload, task, campaignTasks, overrideTasks,
				overrideTaskTime, overrideAfterTaskTime);
			_futureData.Stub(x => x.Fetch(_workload, skillDays, _futurePeriod)).Return(new[] {workloadDay2, workloadDay1});
		}

		private void stubForecastDataForOneDay(Task task, Percent campaignTasks, double? overrideTasks, TimeSpan? overrideTaskTime, TimeSpan? overrideAfterTaskTime)
		{
			_theDate = new DateOnly(2014, 3, 1);
			_scenario = new Scenario("s1").WithId();
			_futurePeriod = new DateOnlyPeriod(_theDate, _theDate);
			var skillDays = new List<ISkillDay>();

			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			_workload = skill.WorkloadCollection.Single();
			_workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			_workloadRepository.Stub(x => x.Get(_workload.Id.Value)).Return(_workload);
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_skillDayRepository.Stub(x => x.FindRange(_futurePeriod, skill, _scenario)).Return(skillDays);
			_futureData = MockRepository.GenerateMock<IFutureData>();

			var workloadDay1 = createWorkloadDay(_theDate, new Workload(skill), task, campaignTasks, overrideTasks,
				overrideTaskTime, overrideAfterTaskTime);
			_futureData.Stub(x => x.Fetch(_workload, skillDays, _futurePeriod)).Return(new[] { workloadDay1 });
		}

		private static WorkloadDay createWorkloadDay(DateOnly date, Workload workload, Task task, Percent campaignTasks, double? overrideTasks,
			TimeSpan? overrideTaskTime, TimeSpan? overrideAfterTaskTime)
		{
			var workloadDay = new WorkloadDay();
			workloadDay.Create(date, workload, new List<TimePeriod>());
			workloadDay.MakeOpen24Hours();
			workloadDay.Tasks = task.Tasks;
			workloadDay.AverageTaskTime = task.AverageTaskTime;
			workloadDay.AverageAfterTaskTime = task.AverageAfterTaskTime;
			workloadDay.CampaignTasks = campaignTasks;
			workloadDay.SetOverrideTasks(overrideTasks, null);
			workloadDay.OverrideAverageTaskTime = overrideTaskTime;
			workloadDay.OverrideAverageAfterTaskTime = overrideAfterTaskTime;

			return workloadDay;
		}
	}
}