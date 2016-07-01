using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class AvailableResourcesToMoveOnSkill
	{
		private const int highValueForClosedSkill = int.MaxValue;
		private readonly IDictionary<ISkill, double> _resourcesOnSkill;

		public AvailableResourcesToMoveOnSkill(IDictionary<ISkill, double> resources, bool allPrimarySkillsAreClosed)
		{
			_resourcesOnSkill = resources;
			if (allPrimarySkillsAreClosed)
			{
				RemainingTotalResources = highValueForClosedSkill;
				TotalResourcesAtStart = highValueForClosedSkill;
			}
			else
			{
				setTotalResources();
				TotalResourcesAtStart = RemainingTotalResources;
			}
		}

		public IDictionary<ISkill, double> ResourcesAvailableForPrimarySkill()
		{
			return _resourcesOnSkill;
		}

		public double TotalResourcesAtStart { get; private set; }
		public double RemainingTotalResources { get; set; }

		private void setTotalResources()
		{
			foreach (var value in _resourcesOnSkill.Values)
			{
				RemainingTotalResources += value;
			}
		}
	}
}