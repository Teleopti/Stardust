﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class BpoResource
	{
		private readonly IEnumerable<ISkill> _skills;
		private readonly double _resources;
		private readonly DateTimePeriod _period;
		private readonly Lazy<ISet<IActivity>> _activities;

		public BpoResource(double resources, IEnumerable<ISkill> skills, DateTimePeriod period)
		{
			_resources = resources;
			_skills = skills;
			_period = period;
			_activities = new Lazy<ISet<IActivity>>(() => new HashSet<IActivity>(_skills.Select(x => x.Activity)));
		}

		public IPerson CreateTempAgent()
		{
			var tempAgent = new Person();
			var personPeriod = new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			_skills.ForEach(x => personPeriod.AddPersonSkill(new PersonSkill(x, new Percent(1))));
			tempAgent.AddPersonPeriod(personPeriod);
			return tempAgent;
		}
		
		public IEnumerable<ResourceLayer> CreateResourceLayers()
		{
			return _activities.Value.Select(activity => new ResourceLayer
			{
				PayloadId = activity.Id.Value,
				Period = _period,
				Resource = _resources / _activities.Value.Count
			});
		}
	}
}