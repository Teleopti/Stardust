using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalPerformanceRepository : IExternalPerformanceRepository
	{
		private readonly List<ExternalPerformance> externalPerformances = new List<ExternalPerformance>();

		public void Add(ExternalPerformance externalPerformance)
		{
			externalPerformances.Add(externalPerformance);
		}

		public IEnumerable<IExternalPerformance> FindAllExternalPerformances()
		{
			return externalPerformances;
		}

		public IExternalPerformance FindExternalPerformanceByExternalId(int externalId)
		{
			return externalPerformances.SingleOrDefault(p => p.ExternalId == externalId);
		}
	}
}