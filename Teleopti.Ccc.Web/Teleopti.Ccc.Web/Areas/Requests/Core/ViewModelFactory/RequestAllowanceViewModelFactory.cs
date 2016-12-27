using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestAllowanceViewModelFactory : IRequestAllowanceViewModelFactory
	{
		private readonly IRequestAllowanceProvider _requestAllowanceProvider;
		private readonly IBudgetGroupRepository _budgetGroupRepository;

		public RequestAllowanceViewModelFactory(IRequestAllowanceProvider requestAllowanceProvider, IBudgetGroupRepository budgetGroupRepository)
		{
			_requestAllowanceProvider = requestAllowanceProvider;
			_budgetGroupRepository = budgetGroupRepository;
		}

		public BudgetAbsenceAllowanceViewModel CreateBudgetAbsenceAllowanceViewModel(DateOnlyPeriod period, Guid? budgetGroupId)
		{
			throw new NotImplementedException();
		}

		public IList<BudgetGroupViewModel> CreateBudgetGroupViewModels()
		{
			var list = _budgetGroupRepository.LoadAll();
			if (list != null)
			{
				return list.OrderBy(b => b.Name).Select(b => new BudgetGroupViewModel
				{
					Id = b.Id.GetValueOrDefault(),
					Name = b.Name
				}).ToList();
			}
			return new List<BudgetGroupViewModel>();
		}
	}
}