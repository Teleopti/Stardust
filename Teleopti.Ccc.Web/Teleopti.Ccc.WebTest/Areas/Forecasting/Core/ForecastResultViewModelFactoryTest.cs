using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
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
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(), 12d, TimeSpan.FromSeconds(300), TimeSpan.FromSeconds(400));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			((object)firstDay.date).Should().Be.EqualTo(_theDate.Date);
			(Math.Round((double) firstDay.vc, 1)).Should().Be.EqualTo(Math.Round(8.1d, 1));
			(Math.Round((double) firstDay.vtc, 1)).Should().Be.EqualTo(Math.Round(12d, 1));
			((double)firstDay.vtt).Should().Be.EqualTo(100d);
			((double)firstDay.vttt).Should().Be.EqualTo(300d);
			((double)firstDay.vacw).Should().Be.EqualTo(200d);
			((double)firstDay.vtacw).Should().Be.EqualTo(400d);
		}

		[Test]
		public void ShouldCreateViewModelWithCampaign()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(50), null, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double) firstDay.vcampaign, 2)).Should().Be.EqualTo(-1d);
		}

		[Test]
		public void ShouldCreateViewModelWithNegativeCampaign()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(-50), null, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.vcampaign, 2)).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCreateViewModelWithOverrideCalls()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(0), 500d, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.voverride, 2)).Should().Be.EqualTo(-1d);
		}

		[Test]
		public void ShouldCreateViewModelWithOverrideTalkTime()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(0), null, TimeSpan.FromSeconds(90), null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.voverride, 2)).Should().Be.EqualTo(-1d);
		}

		[Test]
		public void ShouldCreateViewModelWithOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(0), null, null, TimeSpan.FromSeconds(90));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.voverride, 2)).Should().Be.EqualTo(-1d);
		}

		[Test, ExpectedException(typeof(RuntimeBinderException))]
		public void ShouldCreateViewModelWhereCampaignIsMissing()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(75), 400d, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.voverride, 2)).Should().Be.EqualTo(-1d);
			(Math.Round((double)firstDay.vcampaign, 2)).Should().Be.EqualTo(75d);
		}

		[Test]
		public void ShouldCreateViewModelWhenHaveBothCampaignCallsAndOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), null, null, TimeSpan.FromSeconds(120));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.vcombo, 2)).Should().Be.EqualTo(-1d);
		}

		[Test]
		public void ShouldCreateViewModelWhenHaveBothCampaignCallsAndOverrideCalls()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), 20.1d, null, null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.vcombo, 2)).Should().Be.EqualTo(-1d);
		}

		[Test, ExpectedException(typeof(RuntimeBinderException))]
		public void ShouldNotHaveVoverrideInViewModelWhenHaveBothCampaignCallsAndOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), null, null, TimeSpan.FromSeconds(120));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.voverride, 2)).Should().Be.EqualTo(null);
		}

		[Test, ExpectedException(typeof(RuntimeBinderException))]
		public void ShouldNotHaveVcampaignInViewModelWhenHaveBothCampaignCallsAndOverrideAcw()
		{
			stubForecastDataForOneDay(new Task(8.1d, TimeSpan.FromSeconds(100), TimeSpan.FromSeconds(200)), new Percent(150), null, null, TimeSpan.FromSeconds(120));

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.vcampaign, 2)).Should().Be.EqualTo(new Percent(150).Value);
		}

		private void stubForecastDataForOneDay(Task task, Percent campaignTasks, double? overrideTasks, TimeSpan? overrideTaskTime, TimeSpan? overrideAfterTaskTime)
		{
			_theDate = new DateOnly(2014, 3, 1);
			_scenario = new Scenario("s1");
			_futurePeriod = new DateOnlyPeriod(_theDate, _theDate);
			var skillDays = new List<ISkillDay>();

			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			_workload = skill.WorkloadCollection.Single();
			_workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			_workloadRepository.Stub(x => x.Get(_workload.Id.Value)).Return(_workload);
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_skillDayRepository.Stub(x => x.FindRange(_futurePeriod, skill, _scenario)).Return(skillDays);
			_futureData = MockRepository.GenerateMock<IFutureData>();

			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(_theDate, new Workload(skill), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.Tasks = task.Tasks;
			workloadDay1.AverageTaskTime = task.AverageTaskTime;
			workloadDay1.AverageAfterTaskTime = task.AverageAfterTaskTime;
			workloadDay1.CampaignTasks = campaignTasks;
			workloadDay1.SetOverrideTasks(overrideTasks, null);
			workloadDay1.OverrideAverageTaskTime = overrideTaskTime;
			workloadDay1.OverrideAverageAfterTaskTime = overrideAfterTaskTime;

			_futureData.Stub(x => x.Fetch(_workload, skillDays, _futurePeriod)).Return(new[] { workloadDay1 });
		}
	}
}