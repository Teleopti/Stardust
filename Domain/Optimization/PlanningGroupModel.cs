using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupModel
	{
		public PlanningGroupModel()
		{
			Filters = new List<FilterModel>();
		}

		public Guid Id { get; set; }
		public string Name { get; set; }
		public IList<FilterModel> Filters { get; set; }
		public int AgentCount { get; set; }
		public double PreferenceValue { get; set; }
	}
}