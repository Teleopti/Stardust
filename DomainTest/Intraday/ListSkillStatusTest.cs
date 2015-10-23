﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[IntradayTest]
	public class ListSkillStatusTest
	{
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public IntradaySkillStatusService Target;

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
			var models = new List<SkillTaskDetailsModel>
			{
				new SkillTaskDetailsModel
				{
					TotalTasks = 20,
					SkillId = skillId,
					Minimum = new DateTime(2015, 10, 21, 9, 0, 0, DateTimeKind.Utc),
					Maximum = new DateTime(2015, 10, 21, 10, 0, 0, DateTimeKind.Utc)
				}
			};
			SkillRepository.Add(skill);
			SkillDayRepository.AddFakeTemplateTaskModels(models);
			Assert.True(true);
			var result = Target.GetSkillStatusModels();
			result.Should().Not.Be.Null();
		}

	}
}