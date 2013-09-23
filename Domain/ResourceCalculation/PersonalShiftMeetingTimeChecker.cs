﻿using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonalShiftMeetingTimeChecker
	{
		bool CheckTimeMeeting(IEditableShift mainShift, IVisualLayerCollection mainShiftProjection, ReadOnlyCollection<IPersonMeeting> meetings);
		bool CheckTimePersonAssignment(IEditableShift mainShift, IVisualLayerCollection mainShiftProjection, IPersonAssignment personAssignment);
	}

	public class PersonalShiftMeetingTimeChecker : IPersonalShiftMeetingTimeChecker
	{
		public bool CheckTimeMeeting(IEditableShift mainShift, IVisualLayerCollection mainShiftProjection, ReadOnlyCollection<IPersonMeeting> meetings)
		{
			var worktime = mainShiftProjection.WorkTime();
			var contractTime = mainShiftProjection.ContractTime();
			var clone = mainShift.MakeCopy();
			
			if(!mainShiftProjection.Period().HasValue)
				return false;

			var period = mainShiftProjection.Period().Value;

			foreach (var personMeeting in meetings)
			{
				if (!personMeeting.Period.Intersect(period))
					return false;

				foreach (var mainShiftLayer in mainShift.LayerCollection)
				{
					if (!mainShiftLayer.Payload.AllowOverwrite && mainShiftLayer.Period.Intersect(personMeeting.Period))
						return false;
				}

				var layer = new EditableShiftLayer(personMeeting.BelongsToMeeting.Activity, personMeeting.Period);
				clone.LayerCollection.Add(layer);	
			}

			var mainWithMeetingProjection = clone.ProjectionService().CreateProjection();
			var mainWithMeetingWorkTime = mainWithMeetingProjection.WorkTime();
			var mainWithMeetingContractTime = mainWithMeetingProjection.ContractTime();

			return (worktime == mainWithMeetingWorkTime) && (contractTime == mainWithMeetingContractTime);
		}

		public bool CheckTimePersonAssignment(IEditableShift mainShift, IVisualLayerCollection mainShiftProjection,
		                                      IPersonAssignment personAssignment)
		{
			var worktime = mainShiftProjection.WorkTime();
			var contractTime = mainShiftProjection.ContractTime();
			var clone = mainShift.MakeCopy();

			if (!mainShiftProjection.Period().HasValue)
				return false;

			var period = mainShiftProjection.Period().Value;

			if (!personAssignment.Period.Intersect(period))
				return false;

			foreach (var mainShiftLayer in mainShift.LayerCollection)
			{
				if (mainShiftLayer.Payload.AllowOverwrite)
					continue;
				foreach (var personalLayer in personAssignment.PersonalLayers())
				{
					if (mainShiftLayer.Period.Intersect(personalLayer.Period))
						return false;
				}
			}

			foreach (var personalLayer in personAssignment.PersonalLayers())
			{
				var layer = new EditableShiftLayer(personalLayer.Payload, personalLayer.Period);
				clone.LayerCollection.Add(layer);
			}

			var mainWithPersonalShiftProjection = clone.ProjectionService().CreateProjection();
			var mainWithPersonalShiftWorkTime = mainWithPersonalShiftProjection.WorkTime();
			var mainWithPersonalShiftContractTime = mainWithPersonalShiftProjection.ContractTime();

			return (worktime == mainWithPersonalShiftWorkTime) && (contractTime == mainWithPersonalShiftContractTime);
		}
	}
}
