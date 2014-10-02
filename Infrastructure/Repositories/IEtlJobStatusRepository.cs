using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IEtlJobStatusRepository
	{
		IEnumerable<EtlJobStatusModel> Load(DateOnly date, bool showOnlyErrors);
	}
}