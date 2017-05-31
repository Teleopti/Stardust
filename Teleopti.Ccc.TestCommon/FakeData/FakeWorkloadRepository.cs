using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeWorkloadRepository : IWorkloadRepository
	{
		private readonly IList<IWorkload> _workloads;

		public FakeWorkloadRepository()
		{
			_workloads = new List<IWorkload>();
		}

		public void Add(IWorkload workload)
		{
			if (!workload.Id.HasValue)
				workload.SetId(Guid.NewGuid());
			_workloads.Add(workload);
		}

		public void Remove(IWorkload entity)
		{
			throw new NotImplementedException();
		}

		public IWorkload Get(Guid id)
		{
			return _workloads.FirstOrDefault(x => x.Id == id);
		}

		public IList<IWorkload> LoadAll()
		{
			return _workloads;
		}

		public IWorkload Load(Guid id)
		{
			throw new NotImplementedException();
		}
	}
}
