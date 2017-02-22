using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public class OldIsland
	{
		private readonly HashSet<string> _skillGuidStrings;
		private readonly IList<string> _groupKeys;
		private readonly VirtualSkillGroupsCreatorResult _skillGroupsCreatorResult;

		public OldIsland(IEnumerable<string> skillGuidStrings, IList<string> groupKeys, VirtualSkillGroupsCreatorResult skillGroupsCreatorResult)
		{
			_skillGuidStrings = new HashSet<string>(skillGuidStrings);
			_groupKeys = groupKeys;
			_skillGroupsCreatorResult = skillGroupsCreatorResult;
		}

		public IList<string> SkillGuidStrings
		{
			get { return _skillGuidStrings.ToList(); }
		}

		public IList<string> GroupKeys
		{
			get { return _groupKeys; }
		}

		public IEnumerable<IPerson> AgentsInIsland()
		{
			var result = new List<IPerson>();
			foreach (var groupKey in _groupKeys)
			{
				result.AddRange(_skillGroupsCreatorResult.GetPersonsForSkillGroupKey(groupKey));
			}

			return result;
		}

		public IEnumerable<Guid> SkillIds()
		{
			return SkillGuidStrings.Select(Guid.Parse);
		}
	}
}
