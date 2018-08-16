using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class QueueStatisticsModel
	{
		public DateOnly Date { get; set; }
		public double OriginalTasks { get; set; }
		public double ValidatedTasks { get; set; }
	}
}