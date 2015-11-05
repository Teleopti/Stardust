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
			stubForecastDataForOneDay(8.1d, new TimeSpan(0, 0, 100), new TimeSpan(0, 0, 200), new Percent(), null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			((object)firstDay.date).Should().Be.EqualTo(_theDate.Date);
			(Math.Round((double) firstDay.vc, 1)).Should().Be.EqualTo(Math.Round(8.1d, 1));
			(Math.Round((double) firstDay.vtc, 1)).Should().Be.EqualTo(Math.Round(8.1d, 1));
			((double)firstDay.vtt).Should().Be.EqualTo(100d);
			((double)firstDay.vacw).Should().Be.EqualTo(200d);
		}

		[Test]
		public void ShouldCreateViewModelWithCampaign()
		{
			stubForecastDataForOneDay(8.1d, new TimeSpan(0, 0, 100), new TimeSpan(0, 0, 200), new Percent(50), null);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double) firstDay.vcampaign, 2)).Should().Be.EqualTo(50d);
		}

		[Test]
		public void ShouldCreateViewModelWithOverride()
		{
			stubForecastDataForOneDay(8.1d, new TimeSpan(0, 0, 100), new TimeSpan(0, 0, 200), new Percent(0), 500d);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.voverride, 2)).Should().Be.EqualTo(500d);
		}

		[Test, ExpectedException(typeof(RuntimeBinderException))]
		public void ShouldCreateViewModelWhereCampaignIsOverridden()
		{
			stubForecastDataForOneDay(8.1d, new TimeSpan(0, 0, 100), new TimeSpan(0, 0, 200), new Percent(75), 400d);

			var target = new ForecastResultViewModelFactory(_workloadRepository, _skillDayRepository, _futureData);
			var result = target.Create(_workload.Id.Value, _futurePeriod, _scenario);

			result.WorkloadId.Should().Be.EqualTo(_workload.Id.Value);

			dynamic firstDay = result.Days.First();
			(Math.Round((double)firstDay.voverride, 2)).Should().Be.EqualTo(400d);
			(Math.Round((double)firstDay.vcampaign, 2)).Should().Be.EqualTo(75d);
		}

		private void stubForecastDataForOneDay(double numberOfTasks, TimeSpan taskTime, TimeSpan afterTaskTime, Percent campaignTasks, double? overrideTasks)
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
			workloadDay1.Tasks = numberOfTasks;
			workloadDay1.AverageTaskTime = taskTime;
			workloadDay1.AverageAfterTaskTime = afterTaskTime;
			workloadDay1.CampaignTasks = campaignTasks;
			workloadDay1.OverrideTasks = overrideTasks;

			_futureData.Stub(x => x.Fetch(_workload, skillDays, _futurePeriod)).Return(new[] { workloadDay1 });
		}
	}
}