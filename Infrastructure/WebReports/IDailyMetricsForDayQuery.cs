
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public interface IDailyMetricsForDayQuery
	{
		DailyMetricsForDayResult Execute(DateOnly date);
	}
}