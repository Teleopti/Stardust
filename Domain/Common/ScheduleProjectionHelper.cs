using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Common
{
	public class ScheduleProjectionHelper : IScheduleProjectionHelper
	{
		public IList<Guid> GetMatchedShiftLayerIds(IScheduleDay scheduleDay, IVisualLayer layer, bool isOvertime = false)
		{
			var matchedLayerIds = new List<Guid>();
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayersList = new List<IShiftLayer>();
			if (personAssignment != null && personAssignment.ShiftLayers.Any())
			{
				shiftLayersList = personAssignment.ShiftLayers.ToList();
			}
			foreach (var shiftLayer in shiftLayersList)
			{
				if (isOvertime && !(shiftLayer is OvertimeShiftLayer))
				{
					continue;
				}

				if (layer.Payload.Id.GetValueOrDefault() == shiftLayer.Payload.Id.GetValueOrDefault() && layer.Period.Intersect(shiftLayer.Period))
				{
					matchedLayerIds.Add(shiftLayer.Id.GetValueOrDefault());
				}
			}
			return matchedLayerIds;
		}

		public IList<IPersonalShiftLayer> GetMatchedPersonalShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayers = new List<IPersonalShiftLayer>();
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayersList = new List<IShiftLayer>();
			if (personAssignment != null && personAssignment.ShiftLayers.Any())
			{
				shiftLayersList = personAssignment.ShiftLayers.ToList();
			}
			foreach (var shiftLayer in shiftLayersList)
			{
				var isPersonalLayer = shiftLayer is IPersonalShiftLayer;
				if (layer.Payload.Id.GetValueOrDefault() == shiftLayer.Payload.Id.GetValueOrDefault() && (layer.Period.Intersect(shiftLayer.Period)) && isPersonalLayer)
				{
					matchedLayers.Add(shiftLayer as IPersonalShiftLayer);
				}
			}
			return matchedLayers;
		}

		public IList<Guid> GetMatchedAbsenceLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayerIds = new List<Guid>();
			var personAbsences = scheduleDay.PersonAbsenceCollection().ToList();

			foreach (var personAbs in personAbsences)
			{
				if (layer.Payload.Id.GetValueOrDefault() == personAbs.Layer.Payload.Id.GetValueOrDefault() && (layer.Period.Contains(personAbs.Period) || personAbs.Period.Contains(layer.Period)))
				{
					matchedLayerIds.Add(personAbs.Id.GetValueOrDefault());
				}
			}
			return matchedLayerIds;
		}
		public IList<IShiftLayer> GetMatchedMainShiftLayers(IScheduleDay scheduleDay, IVisualLayer layer)
		{
			var matchedLayers = new List<IShiftLayer>();
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayersList = new List<IShiftLayer>();
			if (personAssignment != null && personAssignment.ShiftLayers.Any())
			{
				shiftLayersList = personAssignment.ShiftLayers.ToList();
			}
			foreach (var shiftLayer in shiftLayersList)
			{
				if (layer.Payload.Id.GetValueOrDefault() == shiftLayer.Payload.Id.GetValueOrDefault() && layer.Period.Intersect(shiftLayer.Period))
				{
					matchedLayers.Add(shiftLayer);
				}
			}
			return matchedLayers;
		}
	}
}