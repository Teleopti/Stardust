using System;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SkillGroupConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string Skill { get; set; }
		public SkillGroup SkillGroup { get; set; }
		public string Skills { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepository = SkillRepository.DONT_USE_CTOR(currentUnitOfWork);
			var skillGroupRepository = new SkillGroupRepository(currentUnitOfWork);

			if (!string.IsNullOrEmpty(Skill))
			{
				var skill = skillRepository.LoadAll().Single(x => x.Name.Equals(Skill));
				var skillInIntraday = new SkillInIntraday() {Id = skill.Id.Value, IsDeleted = false, Name = skill.Name};
				SkillGroup = new SkillGroup { Name = Name, Skills = new Collection<SkillInIntraday> { skillInIntraday } };
			}

			if (!string.IsNullOrEmpty(Skills))
			{
				var skills = skillRepository.LoadAll();
				var skillsInIntraday = Skills
					.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries)
					.Select(s => skills.Single(x => x.Name.Equals(s)))
					.Select(x => new SkillInIntraday {Id = x.Id.Value, IsDeleted = false, Name = x.Name})
					.ToList();
				SkillGroup = new SkillGroup { Name = Name, Skills = skillsInIntraday };
			}

			skillGroupRepository.Add(SkillGroup);
		}
	}
}