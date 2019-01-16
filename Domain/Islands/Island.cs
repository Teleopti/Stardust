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
			return _skillSets.SelectMany(x => x.Agents).Where(x => !x.IsExternalAgent);
		}

		public IEnumerable<Guid> SkillIds()
		{
			return _skillSets.SelectMany(s => s.Skills).Where(s => s.Id.HasValue).Select(s => s.Id.Value).ToHashSet();
		}

		public static Island Merge(Island island1, Island island2)
		{
			return new Island(island1._skillSets.Union(island2._skillSets), island1._noAgentsKnowingSkill);
		}

		public IslandModel CreateClientModel()
		{
			var ret = new IslandModel();
			var skillSetModels = new List<SkillSetModel>();
			foreach (var skillSet in _skillSets)
			{
				var skillSetModel = new SkillSetModel();
				var skillsModel = skillSet.Skills.Select(skill => new SkillModel
				{
					Name = skill.Name,
					NumberOfAgentsOnSkill = _noAgentsKnowingSkill[skill],
					ActivityName = skill.Activity?.Name
				}).ToList();
				skillsModel.Sort();
				skillSetModel.Skills = skillsModel;
				skillSetModel.NumberOfAgentsOnSkillSet = skillSet.Agents.Count();
				skillSetModels.Add(skillSetModel);
			}
			skillSetModels.Sort();
			ret.SkillSets = skillSetModels;
			ret.NumberOfAgentsOnIsland = ret.SkillSets.Sum(x => x.NumberOfAgentsOnSkillSet);
			return ret;
		}

		public IslandExtendedModel CreatExtendedClientModel()
		{
			var ret = new IslandExtendedModel();
			var skillsInIsland = new HashSet<ISkill>();
			var skillSets = new List<SkillSetExtendedModel>();
			foreach (var skillSet in _skillSets)
			{
				skillSets.Add(new SkillSetExtendedModel
				{
					AgentsInSkillSet = skillSet.Agents,
					SkillsInSkillSet = skillSet.Skills
				});
				foreach (var skill in skillSet.Skills)
				{
					skillsInIsland.Add(skill);
				}
			}
			ret.SkillsInIsland = skillsInIsland;
			ret.SkillSetsInIsland = skillSets;

			return ret;
		}
	}
}