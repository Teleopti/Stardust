using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public interface IQueueMetricsForDayQuery
	{
		ICollection<QueueMetricsForDayResult> Execute(DateOnly date);
	}
}