using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeKpiRepository : IKpiRepository
	{
		private readonly List<IKeyPerformanceIndicator> _kpis = new List<IKeyPerformanceIndicator>();

		public void Add(IKeyPerformanceIndicator root)
		{
			_kpis.Add(root);
		}

		public void Remove(IKeyPerformanceIndicator root)
		{
			_kpis.Remove(root);
		}

		public IKeyPerformanceIndicator Get(Guid id)
		{
			return _kpis.FirstOrDefault(x => x.Id.GetValueOrDefault() == id);
		}

		public IList<IKeyPerformanceIndicator> LoadAll()
		{
			return _kpis;
		}

		public IKeyPerformanceIndicator Load(Guid id)
		{
			return _kpis.First(x => x.Id.GetValueOrDefault() == id);
		}

		public IUnitOfWork UnitOfWork { get; }
	}
}