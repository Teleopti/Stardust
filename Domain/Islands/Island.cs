using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands.ClientModel;

namespace Teleopti.Ccc.Domain.Islands
{
	public class Island
	{
		private readonly IEnumerable<SkillSet> _skillSets;
		private readonly IDictionary<ISkill, int> _noAgentsKnowingSkill;

		public Island(IEnumerable<SkillSet> skillSets, IDictionary<ISkill, int> noAgentsKnowingSkill)
		{
			_skillSets = skillSets;
			_noAgentsKnowingSkill = noAgentsKnowingSkill;
		}

		public IEnumerable<IPerson> AgentsInIsland()
		{
			return _skillSets.SelectMany(x => x.Agents);
		}

		public IEnumerable<Guid> SkillIds()
		{
			var ret = new HashSet<Guid>();
			foreach (var skillSet in _skillSets)
			{
				foreach (var skillSetSkill in skillSet.Skills)
				{
					if (skillSetSkill.Id.HasValue)
					{
						ret.Add(skillSetSkill.Id.Value);
					}
				}
			}
			return ret;
		}

		public IslandModel CreateClientModel()
		{
			var ret = new IslandModel();
			var skillGroupModels = new List<SkillSetModel>();
			foreach (var skillGroup in _skillSets)
			{
				var skillGroupModel = new SkillSetModel();
				var skillsModel = skillGroup.Skills.Select(skill => new SkillModel
				{
					Name = skill.Name,
					NumberOfAgentsOnSkill = _noAgentsKnowingSkill[skill],
					ActivityName = skill.Activity?.Name
				}).ToList();
				skillsModel.Sort();
				skillGroupModel.Skills = skillsModel;
				skillGroupModel.NumberOfAgentsOnSkillSet = skillGroup.Agents.Count();
				skillGroupModels.Add(skillGroupModel);
			}
			skillGroupModels.Sort();
			ret.SkillSets = skillGroupModels;
			ret.NumberOfAgentsOnIsland = ret.SkillSets.Sum(x => x.NumberOfAgentsOnSkillSet);
			return ret;
		}

		public IslandExtendedModel CreatExtendedClientModel()
		{
			var ret = new IslandExtendedModel();
			var skillsInIsland = new HashSet<ISkill>();
			var skillGroups = new List<SkillSetExtendedModel>();
			foreach (var skillGroup in _skillSets)
			{
				skillGroups.Add(new SkillSetExtendedModel
				{
					AgentsInSkillSet = skillGroup.Agents,
					SkillsInSkillSet = skillGroup.Skills
				});
				foreach (var skill in skillGroup.Skills)
				{
					skillsInIsland.Add(skill);
				}
			}
			ret.SkillsInIsland = skillsInIsland;
			ret.SkillSetsInIsland = skillGroups;

			return ret;
		}
	}
}