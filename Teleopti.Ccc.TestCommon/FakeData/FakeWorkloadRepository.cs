using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeWorkloadRepository : IWorkloadRepository
	{
		private IList<IWorkload> _workloads;

		public FakeWorkloadRepository()
		{
			_workloads = new List<IWorkload>();
		}

		public void Add(IWorkload workload)
		{
			workload.SetId(Guid.NewGuid());
			_workloads.Add(workload);
		}

		public void Remove(IWorkload entity)
		{
			throw new NotImplementedException();
		}

		public IWorkload Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IWorkload> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IWorkload Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IWorkload> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
	}
}
