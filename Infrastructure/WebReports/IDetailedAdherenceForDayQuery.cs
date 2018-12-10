using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public interface IDetailedAdherenceForDayQuery
	{
		ICollection<DetailedAdherenceForDayResult> Execute(DateOnly date);
	}
}