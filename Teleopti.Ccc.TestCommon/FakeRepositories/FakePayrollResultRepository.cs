using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePayrollResultRepository : IPayrollResultRepository
	{
		private readonly IList<IPayrollResult> _storage = new List<IPayrollResult>(); 

		public void Add(IPayrollResult root)
		{
			_storage.Add(root);
		}

		public void Remove(IPayrollResult root)
		{
			throw new NotImplementedException();
		}

		public IPayrollResult Get(Guid id)
		{
			return _storage.FirstOrDefault(p => id == p.Id);
		}

		public IList<IPayrollResult> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPayrollResult Load(Guid id)
		{
			return Get(id);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public ICollection<IPayrollResult> GetPayrollResultsByPayrollExport(IPayrollExport payrollExport)
		{
			throw new NotImplementedException();
		}
	}
}