using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Cascading.TrackShoveling
{
	public class TrackShovelingOneSkill : IShovelingCallback
	{
		private readonly ISkill _skillToTrack;
		private readonly DateTimePeriod _intervalToTrack;
		private readonly ICollection<AddedResource> _addedResources;
		private readonly IDictionary<IEnumerable<CascadingSkillSet>, RemovedResource> _removedResources;

		public TrackShovelingOneSkill(ISkill skillToTrack, DateTimePeriod intervalToTrack)
		{
			_skillToTrack = skillToTrack;
			_intervalToTrack = intervalToTrack;
			_addedResources = new List<AddedResource>();
			_removedResources = new Dictionary<IEnumerable<CascadingSkillSet>, RemovedResource>();
		}
		public IEnumerable<AddedResource> AddedResources => _addedResources;
		public IEnumerable<RemovedResource> RemovedResources => _removedResources.Values;
		public double ResourcesBeforeShoveling { get; private set; }

		void IShovelingCallback.ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, CascadingSkillSet fromSkillSet, double resources)
		{
			if (interval == _intervalToTrack)
			{
				if (skillToMoveTo.Equals(_skillToTrack))
				{
					_addedResources.Add(new AddedResource(fromSkillSet, resources));
				}
				if (fromSkillSet.PrimarySkills.Contains(_skillToTrack))
				{
					removedResource(skillGroups).ToSubskills.Add(skillToMoveTo);
				}
			}
		}

		void IShovelingCallback.ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, IEnumerable<CascadingSkillSet> skillGroups, double resources)
		{
			if (primarySkill.Equals(_skillToTrack) && interval == _intervalToTrack)
			{
				removedResource(skillGroups).ResourcesMoved = resources;
			}
		}

		void IShovelingCallback.BeforeShoveling(IShovelResourceData shovelResourceData)
		{
			ResourcesBeforeShoveling = shovelResourceData.GetDataForInterval(_skillToTrack, _intervalToTrack).CalculatedResource;
		}

		private RemovedResource removedResource(IEnumerable<CascadingSkillSet> skillGroups)
		{
			RemovedResource removedResource;
			if (_removedResources.TryGetValue(skillGroups, out removedResource))
				return removedResource;
			removedResource = new RemovedResource();
			_removedResources[skillGroups] = removedResource;
			return removedResource;
		}
	}
}