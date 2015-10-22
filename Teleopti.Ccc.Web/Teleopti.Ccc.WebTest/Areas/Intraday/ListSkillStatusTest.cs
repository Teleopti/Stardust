using System;
using System.Collections.Generic;
using Castle.Components.DictionaryAdapter;
using SharpTestsEx;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Intraday;

namespace Teleopti.Ccc.WebTest.Areas.Intraday
{
	[IntradayTest]
	public class ListSkillStatusTest
	{
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public ISkillTasksDetailProvider SkillTasksDetailProvider;
		public IntradaySkillStatusController Target;

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldListAllSkillsWithCalls()
		{
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("Skill", TimeZoneInfo.Utc);
			skill.SetId(skillId);
			var models = new List<SkillTaskDetailsModel> {new SkillTaskDetailsModel {TotalTasks = 20,SkillId = skillId}};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(models);

			var result = Target.GetSkillStatus().Content;
			result.Should().Not.Be.Null();
		}

	}
}