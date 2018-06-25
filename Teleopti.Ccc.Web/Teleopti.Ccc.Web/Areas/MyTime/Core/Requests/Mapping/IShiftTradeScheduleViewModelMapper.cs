using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeScheduleViewModelMapper
	{
		ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data);
		ShiftTradeScheduleViewModel MapForBulletin(ShiftTradeScheduleViewModelData data);
		IEnumerable<ShiftTradeAddPersonScheduleViewModel> GetMeAndPersonToSchedules(DateOnlyPeriod period, Guid personToId);
	}
}