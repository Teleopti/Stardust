using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Intraday
{
	public class SkillForecastedTasksProviderTest
	{
		[Test]
		public void ShouldReturnCorrectNumberOfSkills()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());
			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);
			skillRepository.Add(skill2);

			IScenarioRepository scenarioRepository = new FakeScenarioRepository();
			var skillDayRepository = new FakeSkillDayRepository();

			IEnumerable<SkillTaskDetailsModel> skillTaskDetailsModels = new List<SkillTaskDetailsModel>()
			{
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value, 5, 0.5)
			};
			skillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var target = new SkillForecastedTasksProvider(skillRepository, skillDayRepository,scenarioRepository);
			var result = target.GetForecastedTasks();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnForecastedData()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());

			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);
			skillRepository.Add(skill2);

			IScenarioRepository scenarioRepository = new FakeScenarioRepository();
			var skillDayRepository = new FakeSkillDayRepository();
			IEnumerable<SkillTaskDetailsModel> skillTaskDetailsModels = new List<SkillTaskDetailsModel>()
			{
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value, 5, 0.5)
			};
			skillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var target = new SkillForecastedTasksProvider(skillRepository, skillDayRepository, scenarioRepository);
			var result = target.GetForecastedTasks();

			result.First().Value.Count.Should().Be.EqualTo(3);
			result.Second().Value.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void VerifyThatForecastedTasksAreCalculatedCorrectly()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			skill1.SetId(Guid.NewGuid());
			var skill2 = SkillFactory.CreateSkill("skill2");
			skill2.SetId(Guid.NewGuid());

			var skillRepository = new FakeSkillRepository();
			skillRepository.Add(skill1);
			skillRepository.Add(skill2);

			IScenarioRepository scenarioRepository = new FakeScenarioRepository();
			var skillDayRepository = new FakeSkillDayRepository();
			IEnumerable<SkillTaskDetailsModel> skillTaskDetailsModels = new List<SkillTaskDetailsModel>()
			{
				createFakeTasks(skill1.Id.Value, 5, 0.5),
				createFakeTasks(skill2.Id.Value,10,0.25)
			};
			skillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var target = new SkillForecastedTasksProvider(skillRepository, skillDayRepository, scenarioRepository);
			var result = target.GetForecastedTasks();

			result.First().Value.First().Task.Should().Be.EqualTo(7.5);
			result.Second().Value.First().Task.Should().Be.EqualTo(12.5);
		}

		private SkillTaskDetailsModel createFakeTasks(Guid skillId, int tasks, double campaign)
		{
			return new SkillTaskDetailsModel()
			{
				SkillId = skillId,
				Tasks = tasks,
				CampaignTasks = campaign,
				TotalTasks = tasks + (tasks*campaign),
				Minimum = new DateTime(2015,10,21,9,0,0,DateTimeKind.Utc),
				Maximum = new DateTime(2015,10,21,10,0,0,DateTimeKind.Utc)
			};
		}
	}

	//public class SkillActualTasksProviderTest
	//{
	//	[Test]
	//	public void ShouldReturnCorrectNumberOfSkills()
	//	{
	//		var target = new SkillActualTasksProvider();
	//		var skill1 = SkillFactory.CreateSkill("skill1");
	//		skill1.SetId(Guid.NewGuid());
	//		var skill2 = SkillFactory.CreateSkill("skill2");
	//		skill2.SetId(Guid.NewGuid());
	//		var skillRepository = new FakeSkillRepository();
	//		skillRepository.Add(skill1);
	//		skillRepository.Add(skill2);

	//		IScenarioRepository scenarioRepository = new FakeScenarioRepository();
	//		var skillDayRepository = new FakeSkillDayRepository();
	//		var result = target.GetActaulTasks(TODO, TODO);
	//	}

	//}

	//public class SkillActualTasksProvider
	//{
	//	private readonly IStatisticRepository _staticticRepository;

	//	public SkillActualTasksProvider(IStatisticRepository staticticRepository)
	//	{
	//		_staticticRepository = staticticRepository;
	//	}

	//	public IList<> GetActaulTasks(ICollection<IQueueSource> sources, DateTimePeriod period)
	//	{
	//		_staticticRepository.LoadSpecificDates(sources, period);
	//	}
	//}


}
