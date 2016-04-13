using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePayrollExportRepository : IPayrollExportRepository
	{
		private readonly IList<IPayrollExport> _exports = new List<IPayrollExport>(); 

		public void Add(IPayrollExport root)
		{
			_exports.Add(root);
		}

		public void Remove(IPayrollExport root)
		{
			throw new NotImplementedException();
		}

		public IPayrollExport Get(Guid id)
		{
			return _exports.FirstOrDefault(e => id == e.Id);
		}

		public IList<IPayrollExport> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPayrollExport Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPayrollExport> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; set; }
	}
}