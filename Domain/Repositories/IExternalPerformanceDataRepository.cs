using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IExternalPerformanceDataRepository : IRepository<IExternalPerformanceData>
	{
		ICollection<IExternalPerformanceData> FindByPeriod(DateTimePeriod period);
	}
}
