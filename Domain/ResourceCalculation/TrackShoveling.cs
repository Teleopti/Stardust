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

		void IShovelingCallback.ResourcesWasMovedTo(ISkill skillToMoveTo, DateTimePeriod interval, IEnumerable<ISkill> primarySkillsMovedFrom, IEnumerable<ISkill> parallellSkills, double resources)
		{
			if (skillToMoveTo.Equals(_skillToTrack) && interval == _intervalToTrack)
			{
				_addedResources.Add(new AddedResource(primarySkillsMovedFrom, parallellSkills.Where(x => !x.Equals(_skillToTrack)), resources));
			}
		}

		void IShovelingCallback.ResourcesWasRemovedFrom(ISkill primarySkill, DateTimePeriod interval, double resources)
		{
			if (primarySkill.Equals(_skillToTrack) && interval == _intervalToTrack)
			{
				_removedResources.Add(new RemovedResource(resources));
			}
		}

		void IShovelingCallback.BeforeShoveling(IShovelResourceData shovelResourceData)
		{
			ResourcesBeforeShoveling = shovelResourceData.GetDataForInterval(_skillToTrack, _intervalToTrack).CalculatedResource;
		}
	}

	public class AddedResource
	{
		public AddedResource(IEnumerable<ISkill> fromPrimarySkills, IEnumerable<ISkill> parallellSkills, double resourcesMoved)
		{
			FromPrimarySkills = fromPrimarySkills;
			ResourcesMoved = resourcesMoved;
			ParallellSkills = parallellSkills;
		}

		public IEnumerable<ISkill> FromPrimarySkills { get; }
		public double ResourcesMoved { get; }
		public IEnumerable<ISkill> ParallellSkills { get; }
	}

	public class RemovedResource
	{
		public RemovedResource(double resourcesMoved)
		{
			ResourcesMoved = resourcesMoved;
		}

		public double ResourcesMoved { get; }
	}
}