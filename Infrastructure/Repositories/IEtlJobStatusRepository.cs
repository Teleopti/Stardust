using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IEtlJobStatusRepository
	{
		IEnumerable<EtlJobStatusModel> Load(DateOnly date, bool showOnlyErrors);
	}
}