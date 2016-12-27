using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public interface IRequestAllowanceViewModelFactory
	{
		BudgetAbsenceAllowanceViewModel CreateBudgetAbsenceAllowanceViewModel(DateOnlyPeriod period, Guid? budgetGroupId);

		IList<BudgetGroupViewModel> CreateBudgetGroupViewModels();
	}
}
