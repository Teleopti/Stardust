using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBudgetDayRepository : IBudgetDayRepository
	{
		private readonly ICollection<IBudgetDay> _budgetDays = new List<IBudgetDay>();

		public void Add(IBudgetDay entity)
		{
			_budgetDays.Add(entity);
		}

		public void Remove(IBudgetDay entity)
		{
			throw new NotImplementedException();
		}

		public IBudgetDay Get(Guid id)
		{
			return _budgetDays.FirstOrDefault(b => b.Id == id);
		}

		public IEnumerable<IBudgetDay> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IBudgetDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public DateOnly FindLastDayWithStaffEmployed(IScenario scenario, IBudgetGroup budgetGroup, DateOnly lastDateToSearch)
		{
			throw new NotImplementedException();
		}

		public IList<IBudgetDay> Find(IScenario scenario, IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod, bool noLock = false)
		{
			return _budgetDays.Where(b => b.Scenario == scenario
			&& b.BudgetGroup == budgetGroup
			&& b.Day >= dateOnlyPeriod.StartDate
			&& b.Day <= dateOnlyPeriod.EndDate
			).ToList();
		}
	}

}
