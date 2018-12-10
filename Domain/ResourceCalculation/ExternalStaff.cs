using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ExternalStaff
	{
		private readonly IEnumerable<ISkill> _skills;
		private readonly double _resources;
		private readonly DateTimePeriod _period;
		private readonly Lazy<ISet<IActivity>> _activities;

		public ExternalStaff(double resources, IEnumerable<ISkill> skills, DateTimePeriod period)
		{
			_resources = resources;
			_skills = skills;
			_period = period;
			_activities = new Lazy<ISet<IActivity>>(() => new HashSet<IActivity>(_skills.Select(x => x.Activity)));
		}

		public IPerson CreateExternalAgent()
		{
			var tempAgent = new ExternalAgent();
			var personPeriod = new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			_skills.ForEach(x => personPeriod.AddPersonSkill(new PersonSkill(x, new Percent(1))));
			tempAgent.AddPersonPeriod(personPeriod);
			return tempAgent;
		}
		
		public IEnumerable<ResourceLayer> CreateResourceLayers(TimeSpan minSkillResolution)
		{
			var resourceOnEachLayer = _resources / _activities.Value.Count;
			var currentStartTime = _period.StartDateTime;
			while (currentStartTime < _period.EndDateTime)
			{
				var layerPeriod = new DateTimePeriod(currentStartTime, currentStartTime.Add(minSkillResolution));
				foreach (var activity in _activities.Value)
				{
					yield return new ResourceLayer
					{
						PayloadId = activity.Id.Value,
						Period = layerPeriod,
						Resource = resourceOnEachLayer
					};
				}
				currentStartTime += minSkillResolution;
			}
		}
	}
}