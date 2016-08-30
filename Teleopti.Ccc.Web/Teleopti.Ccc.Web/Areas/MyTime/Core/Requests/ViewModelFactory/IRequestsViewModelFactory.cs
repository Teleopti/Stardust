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

		IEnumerable<RequestViewModel> CreatePagingViewModel(Paging paging, bool hideOldRequest = false);

		RequestViewModel CreateRequestViewModel(Guid id);

		ShiftTradeRequestsPeriodViewModel CreateShiftTradePeriodViewModel(Guid? id = null);

		ShiftTradeScheduleViewModel CreateShiftTradeScheduleViewModel(ShiftTradeScheduleViewModelData data);

		IList<ShiftTradeSwapDetailsViewModel> CreateShiftTradeRequestSwapDetails(Guid id);

		string CreateShiftTradeMyTeamSimpleViewModel(DateOnly selectedDate);

		AbsenceAccountViewModel GetAbsenceAccountViewModel(Guid absenceId, DateOnly date);
	}
}