using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface ILayerViewModelReworkedMapper
	{
		TeamScheduleLayerViewModel[] Map(IEnumerable<SimpleLayer> sourceLayers);
		TeamScheduleLayerViewModel Map(SimpleLayer sourceLayer);
	}
}