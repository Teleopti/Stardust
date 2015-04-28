using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class PlanningPeriodModel
	{
		public Guid Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public IEnumerable<MissingForecastModel> Skills { get; set; }
		public IEnumerable<string> SuggestedPeriods { get; set; } 
	}
}