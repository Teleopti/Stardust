using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	//ROBTODO: why does this exist if all of the methods are not implemented?
	public class FakeBudgetDayRepository : IBudgetDayRepository
	{
		public void Add(IBudgetDay entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IBudgetDay entity)
		{
			throw new NotImplementedException();
		}

		public IBudgetDay Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IBudgetDay> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IBudgetDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IBudgetDay> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public DateOnly FindLastDayWithStaffEmployed(IScenario scenario, IBudgetGroup budgetGroup, DateOnly lastDateToSearch)
		{
			throw new NotImplementedException();
		}

		public IList<IBudgetDay> Find(IScenario scenario, IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod)
		{
			throw new NotImplementedException();
		}
	}

}
