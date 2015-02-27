using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface ILayerViewModelReworkedMapper
	{
		IEnumerable<LayerViewModelReworked> Map(IEnumerable<SimpleLayer> sourceLayers);
		LayerViewModelReworked Map(SimpleLayer sourceLayer);
	}
}