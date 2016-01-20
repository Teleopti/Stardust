﻿using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public interface IRequestsShiftTradeScheduleViewModelFactory
	{
		ShiftTradeScheduleViewModel CreateViewModel(ShiftTradeScheduleViewModelData inputData);
	}
}