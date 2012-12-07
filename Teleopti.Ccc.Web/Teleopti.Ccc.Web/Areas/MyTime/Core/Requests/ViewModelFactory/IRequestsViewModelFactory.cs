using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public interface IRequestsViewModelFactory
	{
		RequestsViewModel CreatePageViewModel();
		IEnumerable<RequestViewModel> CreatePagingViewModel(Paging paging);
		RequestViewModel CreateRequestViewModel(Guid id);
		ShiftTradeRequestsPreparationViewModel CreateShiftTradePreparationViewModel(DateOnly selectedDate);
	}
}