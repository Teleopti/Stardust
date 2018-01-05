﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class CreateIslands
	{
		private readonly CreateSkillSets _createSkillSets;
		private readonly NumberOfAgentsKnowingSkill _numberOfAgentsKnowingSkill;
		private readonly MoveSkillSetToCorrectIsland _moveSkillSetToCorrectIsland;
		private readonly ReduceSkillSets _reduceSkillSets;
		private readonly IAllStaff _allStaff;

		public CreateIslands(CreateSkillSets createSkillSets, 
								NumberOfAgentsKnowingSkill numberOfAgentsKnowingSkill,
								MoveSkillSetToCorrectIsland moveSkillSetToCorrectIsland,
								ReduceSkillSets reduceSkillSets,
								IAllStaff allStaff)
		{
			_createSkillSets = createSkillSets;
			_numberOfAgentsKnowingSkill = numberOfAgentsKnowingSkill;
			_moveSkillSetToCorrectIsland = moveSkillSetToCorrectIsland;
			_reduceSkillSets = reduceSkillSets;
			_allStaff = allStaff;
		}

		public IEnumerable<Island> Create(DateOnlyPeriod period, ICreateIslandsCallback passedCallback)
		{
			var callback = passedCallback ?? new NullCreateIslandsCallback();
			var allSkillSets = _createSkillSets.Create(_allStaff.Agents(period), period).ToList();
			var noAgentsKnowingSkill = _numberOfAgentsKnowingSkill.Execute(allSkillSets);
			while (true)
			{
				var skillSetsInIsland = allSkillSets.Select(skillSet => new List<SkillSet> { skillSet }).ToList();
				callback.BasicIslandsCreated(skillSetsInIsland, noAgentsKnowingSkill);
				while (_moveSkillSetToCorrectIsland.Execute(allSkillSets, skillSetsInIsland))
				{
					removeEmptyIslands(skillSetsInIsland);
				}

				if (!_reduceSkillSets.Execute(skillSetsInIsland, noAgentsKnowingSkill))
				{
					var islands = skillSetsInIsland.Select(skillSetInIsland => new Island(skillSetInIsland, noAgentsKnowingSkill)).ToArray();
					callback.AfterExtendingDueToReducing(islands);
					return islands;
				}
			}
		}

		private static void removeEmptyIslands(ICollection<List<SkillSet>> skillSetsInIsland)
		{
			skillSetsInIsland.Where(x => x.Count == 0).ToArray().ForEach(x => skillSetsInIsland.Remove(x));
		}
	}
}