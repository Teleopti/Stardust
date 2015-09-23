using System;
using System.Collections.Generic;
using System.Linq;
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
		[Test]
		public void ShouldCreateForecastResultViewModel()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workload = skill.WorkloadCollection.Single();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			workloadRepository.Stub(x => x.Get(workload.Id.Value)).Return(workload);
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			var scenario = new Scenario("s1");
			var futurePeriod = new DateOnlyPeriod(2014, 3, 1, 2014, 3, 1);
			var skillDays = new List<ISkillDay>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, skill, scenario)).Return(skillDays);
			var futureData = MockRepository.GenerateMock<IFutureData>();

			var date1 = new DateOnly(2014, 3, 1);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(date1, new Workload(skill), new List<TimePeriod>());
			workloadDay1.MakeOpen24Hours();
			workloadDay1.Tasks = 8.1d;

			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod)).Return(new[] { workloadDay1 });
			var target = new ForecastResultViewModelFactory(workloadRepository, skillDayRepository, futureData);
			var result = target.Create(workload.Id.Value, futurePeriod, scenario);
			dynamic firstDay = result.Days.First();

			result.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			((object)firstDay.date).Should().Be.EqualTo(date1.Date);
			(Math.Round((double) firstDay.vc, 1)).Should().Be.EqualTo(Math.Round(8.1d, 1));
			(Math.Round((double) firstDay.vtc, 1)).Should().Be.EqualTo(Math.Round(8.1d, 1));
		}
	}
}