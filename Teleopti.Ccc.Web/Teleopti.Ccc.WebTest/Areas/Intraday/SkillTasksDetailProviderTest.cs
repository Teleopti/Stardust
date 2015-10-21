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

namespace Teleopti.Ccc.WebTest.Areas.Intraday
{
	public class SkillTasksDetailProviderTest
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
				createFakeTasks(skill1.Id.Value),
				createFakeTasks(skill1.Id.Value),
				createFakeTasks(skill1.Id.Value),
				createFakeTasks(skill2.Id.Value),
				createFakeTasks(skill2.Id.Value)
			};
			skillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var target = new SkillTasksDetailProvider(skillRepository, skillDayRepository,scenarioRepository);
			var result = target.GetSkillTaskDetails();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldShouldReturnForecastedData()
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
				createFakeTasks(skill1.Id.Value),
				createFakeTasks(skill1.Id.Value),
				createFakeTasks(skill1.Id.Value),
				createFakeTasks(skill2.Id.Value),
				createFakeTasks(skill2.Id.Value)
			};
			skillDayRepository.AddFakeTemplateTaskModels(skillTaskDetailsModels);
			var target = new SkillTasksDetailProvider(skillRepository, skillDayRepository, scenarioRepository);
			var result = target.GetSkillTaskDetails();

			result.First().Value.Count.Should().Be.EqualTo(3);
			result.Second().Value.Count.Should().Be.EqualTo(2);
		}

		private SkillTaskDetailsModel createFakeTasks(Guid skillId)
		{
			return new SkillTaskDetailsModel() { SkillId = skillId };
		}
	}

	
}
