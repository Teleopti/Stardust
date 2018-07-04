using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IScheduleProjectionHelper
	{
		IList<Guid> GetMatchedShiftLayerIds(IScheduleDay scheduleDay, IVisualLayer layer, bool isOvertime = false);
		IList<ShiftLayer> GetMatchedMainShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer);
		IList<Guid> GetMatchedAbsenceLayers(IScheduleDay scheduleDay, IVisualLayer layer);
		IList<PersonalShiftLayer> GetMatchedPersonalShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer);
		IList<ShiftLayer> GetMatchedShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer, bool isOvertime);

		Guid? GetTopShiftLayerId<T>(IList<T> shiftLayers, IVisualLayer layer) where T : ShiftLayer;
	}
}