using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class ModifyInput
	{
		public DateOnly[] Days { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid WorkloadId { get; set; }
	}
}