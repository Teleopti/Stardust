using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class TrackShoveling : IShovelingCallback
	{
		private readonly ISkill _skillToTrack;
		private readonly DateTimePeriod _intervalToTrack;
		private readonly ICollection<AddedResource> _addedResources;

		public TrackShoveling(ISkill skillToTrack, DateTimePeriod intervalToTrack)
		{
			_skillToTrack = skillToTrack;
			_intervalToTrack = intervalToTrack;
			_addedResources = new List<AddedResource>();
		}

		public void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<ISkill> primarySkillsMovedFrom, double resources)
		{
			if (skillToMoveTo.Equals(_skillToTrack) && interval == _intervalToTrack)
			{
				_addedResources.Add(new AddedResource(primarySkillsMovedFrom, resources));
			}
		}
		public IEnumerable<AddedResource> AddedResources => _addedResources;
	}

	public class AddedResource
	{
		public AddedResource(IEnumerable<ISkill> fromPrimarySkills, double resourcesMoved)
		{
			FromPrimarySkills = fromPrimarySkills;
			ResourcesMoved = resourcesMoved;
		}

		public IEnumerable<ISkill> FromPrimarySkills { get; }
		public double ResourcesMoved { get; }
	}
}