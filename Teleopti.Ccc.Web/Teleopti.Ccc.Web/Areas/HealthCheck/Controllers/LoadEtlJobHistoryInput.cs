using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Controllers
{
	public struct LoadEtlJobHistoryInput
	{
		public DateOnly? Date { get; set; }
		public bool ShowOnlyErrors { get; set; }
	}
}