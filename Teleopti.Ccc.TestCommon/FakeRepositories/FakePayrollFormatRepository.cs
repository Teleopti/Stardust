using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePayrollFormatRepository : IPayrollFormatRepository
	{
		private readonly IList<IPayrollFormat> internalStore = new List<IPayrollFormat>();

		public void Add(IPayrollFormat root)
		{
			internalStore.Add(root);
		}

		public void Remove(IPayrollFormat root)
		{
			throw new NotImplementedException();
		}

		public IPayrollFormat Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPayrollFormat> LoadAll()
		{
			return internalStore;
		}

		public IPayrollFormat Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
	}
}