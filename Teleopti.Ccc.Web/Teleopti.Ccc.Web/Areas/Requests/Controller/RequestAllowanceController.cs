using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.Requests.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebRequests)]
	public class RequestAllowanceController : ApiController
	{
		private readonly IRequestAllowanceViewModelFactory _requestAllowanceViewModelFactory;

		public RequestAllowanceController(IRequestAllowanceViewModelFactory requestAllowanceViewModelFactory)
		{
			_requestAllowanceViewModelFactory = requestAllowanceViewModelFactory;
		}

		[HttpGet, Route("api/RequestAllowance/budgetGroups"), UnitOfWork]
		public virtual IList<BudgetGroupViewModel> GetBudgetGroups()
		{
			return _requestAllowanceViewModelFactory.CreateBudgetGroupViewModels();
		}

		[HttpGet, Route("api/RequestAllowance/allowances"), UnitOfWork]
		public virtual IList<BudgetAbsenceAllowanceDetailViewModel> GetAllowances(DateTime date, Guid? budgetGroupId)
		{
			return _requestAllowanceViewModelFactory.CreateBudgetAbsenceAllowanceDetailViewModels(new DateOnly(date), budgetGroupId);
		}
	}
}
