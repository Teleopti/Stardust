using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public interface IRequestsViewModelFactory
	{
		RequestsViewModel CreatePageViewModel();

		IEnumerable<RequestViewModel> CreatePagingViewModel(Paging paging, RequestListFilter filter);

		RequestViewModel CreateRequestViewModel(Guid id);

		ShiftTradeRequestsPeriodViewModel CreateShiftTradePeriodViewModel(Guid? id = null);

		ShiftTradeScheduleViewModel CreateShiftTradeScheduleViewModel(ShiftTradeScheduleViewModelData data);

		IList<ShiftTradeSwapDetailsViewModel> CreateShiftTradeRequestSwapDetails(Guid id);

		string CreateShiftTradeMyTeamSimpleViewModel(DateOnly selectedDate);

		string CreateShiftTradeMySiteIdViewModel(DateOnly selectedDate);

		AbsenceAccountViewModel GetAbsenceAccountViewModel(Guid absenceId, DateOnly date);

		ShiftTradeMultiSchedulesViewModel CreateShiftTradeMultiSchedulesViewModel(ShiftTradeMultiSchedulesForm input);
		ShiftTradeToleranceInfoViewModel CreateShiftTradeToleranceViewModel(Guid personToId);
	}
}