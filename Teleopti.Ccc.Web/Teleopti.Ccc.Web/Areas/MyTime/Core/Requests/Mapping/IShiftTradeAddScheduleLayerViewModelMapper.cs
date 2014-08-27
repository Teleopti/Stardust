using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeAddScheduleLayerViewModelMapper
	{
		IEnumerable<ShiftTradeAddScheduleLayerViewModel> Map(IEnumerable<SimpleLayer> sourceLayers, bool isMySchedule = false);
	}
}