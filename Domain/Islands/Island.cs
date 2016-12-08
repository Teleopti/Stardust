using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class Island : IIsland
	{
		private readonly IEnumerable<SkillGroup> _skillGroups;
		private readonly IDictionary<ISkill, int> _noAgentsKnowingSkill;

		public Island(IEnumerable<SkillGroup> skillGroups, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			_skillGroups = skillGroups;
			_noAgentsKnowingSkill = noAgentsKnowingSkill;
		}

		public IEnumerable<IPerson> AgentsInIsland()
		{
			return _skillGroups.SelectMany(x => x.Agents);
		}

		public IEnumerable<Guid> SkillIds()
		{
			var ret = new HashSet<Guid>();
			foreach (var skillGroup in _skillGroups)
			{
				foreach (var skillGroupSkill in skillGroup.Skills)
				{
					if (skillGroupSkill.Id.HasValue)
					{
						ret.Add(skillGroupSkill.Id.Value);
					}
				}
			}
			return ret;
		}

		public IslandModel CreateClientModel()
		{
			var ret = new IslandModel();
			var skillGroupModels = new List<SkillGroupModel>();
			foreach (var skillGroup in _skillGroups)
			{
				var skillGroupModel = new SkillGroupModel();
				var skillsModel = skillGroup.Skills.Select(skill => new SkillModel
				{
					Name = skill.Name,
					NumberOfAgentsOnSkill = _noAgentsKnowingSkill[skill],
					ActivityName = skill.Activity?.Name
				}).ToList();
				skillsModel.Sort();
				skillGroupModel.Skills = skillsModel;
				skillGroupModel.NumberOfAgentsOnSkillGroup = skillGroup.Agents.Count();
				skillGroupModels.Add(skillGroupModel);
			}
			skillGroupModels.Sort();
			ret.SkillGroups = skillGroupModels;
			ret.NumberOfAgentsOnIsland = ret.SkillGroups.Sum(x => x.NumberOfAgentsOnSkillGroup);
			return ret;
		}
	}
}