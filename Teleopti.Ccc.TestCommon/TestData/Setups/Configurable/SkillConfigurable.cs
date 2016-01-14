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

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillType = SkillTypeFactory.CreateSkillType();
			var skillTypeRepository = new SkillTypeRepository(currentUnitOfWork);
			skillTypeRepository.Add(skillType);

			ISkill skill;

			if (string.IsNullOrEmpty(TimeZone))
			{
				skill = SkillFactory.CreateSkill(Name);
			}
			else
			{
				skill = SkillFactory.CreateSkill(Name, TimeZoneInfo.FindSystemTimeZoneById(TimeZone));
			}

		
			skill.SkillType = skillType;

			var activityRepository = new ActivityRepository(currentUnitOfWork);
			skill.Activity = activityRepository.LoadAll().Single(b => b.Description.Name == Activity);

			var skillRepository = new SkillRepository(currentUnitOfWork);
			skillRepository.Add(skill);
		}
	}
}