using System.Collections.Generic;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class TrackShoveling : IShovelingCallback
	{
		private readonly ISkill _skillToTrack;
		private readonly DateTimePeriod _intervalToTrack;
		private readonly ICollection<AddedResource> _addedResources;
		private readonly ICollection<RemovedResource> _removedResources;

		public TrackShoveling(ISkill skillToTrack, DateTimePeriod intervalToTrack)
		{
			_skillToTrack = skillToTrack;
			_intervalToTrack = intervalToTrack;
			_addedResources = new List<AddedResource>();
			_removedResources = new List<RemovedResource>();
		}
		public IEnumerable<AddedResource> AddedResources => _addedResources;
		public IEnumerable<RemovedResource> RemovedResources => _removedResources;
		public double ResourcesBeforeShoveling { get; private set; }

		public void ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<ISkill> primarySkillsMovedFrom, double resources)
		{
			if (skillToMoveTo.Equals(_skillToTrack) && interval == _intervalToTrack)
			{
				_addedResources.Add(new AddedResource(primarySkillsMovedFrom, resources));
			}
		}

		public void ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, double resources)
		{
			if (primarySkill.Equals(_skillToTrack) && interval == _intervalToTrack)
			{
				_removedResources.Add(new RemovedResource(resources));
			}
		}

		public void BeforeShoveling(IShovelResourceData shovelResourceData)
		{
			ResourcesBeforeShoveling = shovelResourceData.GetDataForInterval(_skillToTrack, _intervalToTrack).CalculatedResource;
		}
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

	public class RemovedResource
	{
		public RemovedResource(double resources)
		{
			Resources = resources;
		}

		public double Resources { get; }
	}
}