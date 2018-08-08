using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Models
{
	public class QueueStatisticsModel
	{
		public DateOnly Date { get; set; }
		public double Tasks { get; set; }
		public double OutlierTasks { get; set; }
	}
}