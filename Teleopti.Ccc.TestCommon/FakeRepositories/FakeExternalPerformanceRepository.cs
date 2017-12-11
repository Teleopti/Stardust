using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalPerformanceRepository : IExternalPerformanceRepository
	{
		private readonly List<IExternalPerformance> externalPerformances = new List<IExternalPerformance>();

		public IEnumerable<IExternalPerformance> FindAllExternalPerformances()
		{
			return externalPerformances;
		}

		public IExternalPerformance FindExternalPerformanceByExternalId(int externalId)
		{
			return externalPerformances.SingleOrDefault(p => p.ExternalId == externalId);
		}

		public int GetExernalPerformanceCount()
		{
			return externalPerformances.Count;
		}

		public void Add(IExternalPerformance externalPerformance)
		{
			externalPerformances.Add(externalPerformance);
		}

		public void Remove(IExternalPerformance externalPerformance)
		{
			externalPerformances.Remove(Get(externalPerformance.Id.Value));
		}

		public IExternalPerformance Get(Guid id)
		{
			return externalPerformances.FirstOrDefault(x => x.Id == id);
		}

		public IExternalPerformance Load(Guid id)
		{
			return Get(id);
		}

		public IList<IExternalPerformance> LoadAll()
		{
			return externalPerformances;
		}
	}
}