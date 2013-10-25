using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class SkillConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Activity { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var skillType = SkillTypeFactory.CreateSkillType();
			var skillTypeRepository = new SkillTypeRepository(uow);
			skillTypeRepository.Add(skillType);

			var skill = SkillFactory.CreateSkill(Name);
			skill.SkillType = skillType;

			var activityRepository = new ActivityRepository(uow);
			skill.Activity = activityRepository.LoadAll().Single(b => b.Description.Name == Activity);

			var skillRepository = new SkillRepository(uow);
			skillRepository.Add(skill);
		}
	}
}