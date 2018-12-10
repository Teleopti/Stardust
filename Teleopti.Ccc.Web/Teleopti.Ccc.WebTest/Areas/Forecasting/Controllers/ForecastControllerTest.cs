using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;


namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastControllerTest : IExtendSystem
	{
		public ForecastController Target;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeWorkloadRepository WorkloadRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeStatisticRepository StatisticRepository;
		public ForecastProvider ForecastProvider;
		public FullPermission FullPermission;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ForecastController>();
		}
		
		[Test, Ignore("Not ready for release yet. Look at this later.")]
		public void ShouldLoadQueueStatistics()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var statsDate = new DateOnly(2018, 05, 04);

			WorkloadRepository.Add(workload);

			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = statsDate.Date.AddHours(10),
					StatOfferedTasks = 10
				},
				new StatisticTask
				{
					Interval = statsDate.AddDays(1).Date.AddHours(10).AddMinutes(15),
					StatOfferedTasks = 20
				}
			});
			
			var result = (OkNegotiatedContentResult<WorkloadQueueStatisticsViewModel>)Target.QueueStatistics(workload.Id.Value);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.QueueStatisticsDays.Count.Should().Be.EqualTo(2);
			result.Content.QueueStatisticsDays.First().Date.Should().Be.EqualTo(statsDate);
			result.Content.QueueStatisticsDays.First().OriginalTasks.Should().Be.EqualTo(10);
			result.Content.QueueStatisticsDays.First().ValidatedTasks.Should().Be.EqualTo(10);
			result.Content.QueueStatisticsDays.Last().Date.Should().Be.EqualTo(statsDate.AddDays(1));
			result.Content.QueueStatisticsDays.Last().OriginalTasks.Should().Be.EqualTo(20);
			result.Content.QueueStatisticsDays.Last().ValidatedTasks.Should().Be.EqualTo(20);
		}
		
		[Test]
		public void ShouldGetSkillsAndWorkloads()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var workloadName = skill.Name + " - " + workload.Name;

			SkillRepository.Has(skill);

			var target = new ForecastController(null, SkillRepository, null, null, null, FullPermission,
				null, null, null, null, null, null);

			dynamic data = target.Skills();
			var result = data.Content;

			IEnumerable<dynamic> skills = result.Skills;
			Assert.AreEqual(skills.Single().Id, skill.Id.Value);
			Assert.AreEqual(skills.Single().SkillType, skill.SkillType.Description.Name);

			IEnumerable<dynamic> workloads = skills.Single().Workloads;
			Assert.AreEqual(workloads.Single().Id, workload.Id.Value);
			Assert.AreEqual(workloads.Single().Name, workloadName);
		}

		[Test]
		public void ShouldHavePermissionForModifySkill()
		{
			var target = new ForecastController(null, SkillRepository, null, null, null, FullPermission,
				null, null, null, null, null, null);

			dynamic data = target.Skills();
			var result = data.Content;
			Assert.IsTrue(result.IsPermittedToModifySkill);
		}
	}
}
