using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class TrackShoveling : IShovelingCallback
	{
		private readonly ISkill _skillToTrack;
		private readonly DateTimePeriod _intervalToTrack;
		private readonly ICollection<AddedResource> _addedResources;
		private readonly IDictionary<IEnumerable<CascadingSkillGroup>, RemovedResource> _removedResources;

		public TrackShoveling(ISkill skillToTrack, DateTimePeriod intervalToTrack)
		{
			_skillToTrack = skillToTrack;
			_intervalToTrack = intervalToTrack;
			_addedResources = new List<AddedResource>();
			_removedResources = new Dictionary<IEnumerable<CascadingSkillGroup>, RemovedResource>();
		}
		public IEnumerable<AddedResource> AddedResources => _addedResources;
		public IEnumerable<RemovedResource> RemovedResources => _removedResources.Values;
		public double ResourcesBeforeShoveling { get; private set; }

		void IShovelingCallback.ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroups, CascadingSkillGroup fromSkillGroup, double resources)
		{
			if (interval == _intervalToTrack)
			{
				if (skillToMoveTo.Equals(_skillToTrack))
				{
					_addedResources.Add(new AddedResource(fromSkillGroup, resources));
				}
				if (fromSkillGroup.PrimarySkills.Contains(_skillToTrack))
				{
					removedResource(skillGroups).ToSubskills.Add(skillToMoveTo);
				}
			}
		}

		void IShovelingCallback.ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, IEnumerable<CascadingSkillGroup> skillGroups, double resources)
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

		private RemovedResource removedResource(IEnumerable<CascadingSkillGroup> skillGroups)
		{
			RemovedResource removedResource;
			if (_removedResources.TryGetValue(skillGroups, out removedResource))
				return removedResource;
			removedResource = new RemovedResource();
			_removedResources[skillGroups] = removedResource;
			return removedResource;
		}
	}

	public class AddedResource
	{
		public AddedResource(CascadingSkillGroup fromSkillGroup, double resourcesMoved)
		{
			FromSkillGroup = fromSkillGroup;
			ResourcesMoved = resourcesMoved;
		}
		public CascadingSkillGroup FromSkillGroup { get; }
		public double ResourcesMoved { get; }
	}

	public class RemovedResource
	{
		public RemovedResource()
		{
			ToSubskills = new List<ISkill>();
		}

		public double ResourcesMoved { get; set; }
		public ICollection<ISkill> ToSubskills { get; }
	}
}