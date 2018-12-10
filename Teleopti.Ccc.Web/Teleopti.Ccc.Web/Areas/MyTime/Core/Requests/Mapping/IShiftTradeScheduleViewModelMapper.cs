using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeScheduleViewModelMapper
	{
		ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data);
		ShiftTradeScheduleViewModel MapForBulletin(ShiftTradeScheduleViewModelData data);
		ShiftTradeMultiSchedulesViewModel GetMeAndPersonToSchedules(DateOnlyPeriod period, Guid personToId);
		ShiftTradeToleranceInfoViewModel GetToleranceInfo(Guid personToId);
		DateOnlyPeriod GetShiftTradeOpenPeriod(IPerson person);
	}
}