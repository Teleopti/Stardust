using System;
using System.Collections.Generic;

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
		public int PreferencePercent { get; set; }
		public IEnumerable<PlanningGroupSettingsModel> Settings { get; set; }
	}
}