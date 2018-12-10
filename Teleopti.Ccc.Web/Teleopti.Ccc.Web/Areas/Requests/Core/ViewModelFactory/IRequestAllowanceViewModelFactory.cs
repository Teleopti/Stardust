using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public interface IRequestAllowanceViewModelFactory
	{
		IList<BudgetAbsenceAllowanceDetailViewModel> CreateBudgetAbsenceAllowanceDetailViewModels(DateOnly date, Guid? budgetGroupId);

		IList<BudgetGroupViewModel> CreateBudgetGroupViewModels();
	}
}
