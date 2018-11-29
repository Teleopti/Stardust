using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading.TrackShoveling
{
	public class TrackShoveling : IShovelingCallback
	{
		private readonly IDictionary<ISkill, TrackShovelingOneSkill> _trackShovelingOneSkills;

		public TrackShoveling(IEnumerable<ISkill> skills, DateTimePeriod interval)
		{
			_trackShovelingOneSkills = new Dictionary<ISkill, TrackShovelingOneSkill>();
			foreach (var skill in skills)
			{
				_trackShovelingOneSkills[skill] = new TrackShovelingOneSkill(skill, interval);
			}
		}

		public TrackShovelingOneSkill For(ISkill skill)
		{
			return _trackShovelingOneSkills[skill];
		}

		void IShovelingCallback.ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, CascadingSkillSet fromSkillSet, double resources)
		{
			foreach (IShovelingCallback trackShovelingOneSkill in _trackShovelingOneSkills.Values)
			{
				trackShovelingOneSkill.ResourcesWasMovedTo(skillToMoveTo, interval, skillGroups, fromSkillSet, resources);
			}
		}

		void IShovelingCallback.ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, double resources)
		{
			foreach (IShovelingCallback trackShovelingOneSkill in _trackShovelingOneSkills.Values)
			{
				trackShovelingOneSkill.ResourcesWasRemovedFrom(primarySkill, interval, skillGroups, resources);
			}
		}

		void IShovelingCallback.BeforeShoveling(IShovelResourceData shovelResourceData)
		{
			foreach (IShovelingCallback trackShovelingOneSkill in _trackShovelingOneSkills.Values)
			{
				trackShovelingOneSkill.BeforeShoveling(shovelResourceData);
			}
		}
	}
}