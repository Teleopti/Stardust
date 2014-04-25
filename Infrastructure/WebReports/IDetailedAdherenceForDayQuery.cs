using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public interface IDetailedAdherenceForDayQuery
	{
		ICollection<DetailedAdherenceForDayResult> Execute(DateOnly date);
	}
}