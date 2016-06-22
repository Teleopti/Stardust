using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SkillConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Activity { get; set; }
		public string TimeZone { get; set; }
		public ISkill Skill { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillType = SkillTypeFactory.CreateSkillType();
			var skillTypeRepository = new SkillTypeRepository(currentUnitOfWork);
			skillTypeRepository.Add(skillType);
			
			if (string.IsNullOrEmpty(TimeZone))
			{
				Skill = SkillFactory.CreateSkill(Name);
			}
			else
			{
				var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
				Skill = SkillFactory.CreateSkill(Name, timeZone);
			}


			Skill.SkillType = skillType;

			var activityRepository = new ActivityRepository(currentUnitOfWork);
			Skill.Activity = activityRepository.LoadAll().Single(b => b.Description.Name == Activity);

			var skillRepository = new SkillRepository(currentUnitOfWork);
			skillRepository.Add(Skill);
		}
	}
}