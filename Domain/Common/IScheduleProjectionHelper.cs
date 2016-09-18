using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IScheduleProjectionHelper
	{
		IList<Guid> GetMatchedShiftLayerIds(IScheduleDay scheduleDay, IVisualLayer layer);
		IList<IShiftLayer> GetMatchedMainShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer);
		IList<Guid> GetMatchedAbsenceLayers(IScheduleDay scheduleDay, IVisualLayer layer);
		IList<IPersonalShiftLayer> GetMatchedPersonalShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer);
	}
}