using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonalShiftMeetingTimeChecker
	{
		bool CheckTimeMeeting(IEditableShift mainShift, IPersonMeeting[] meetings);
		bool CheckTimePersonAssignment(IEditableShift mainShift, IPersonAssignment personAssignment);
	}

	public class PersonalShiftMeetingTimeChecker : IPersonalShiftMeetingTimeChecker
	{
		public bool CheckTimeMeeting(IEditableShift mainShift, IPersonMeeting[] meetings)
		{
			var mainShiftProjection = mainShift.ProjectionService().CreateProjection();
			var worktime = mainShiftProjection.WorkTime();
			var contractTime = mainShiftProjection.ContractTime();
			var clone = mainShift.MakeCopy();

			var mainShiftPeriod = mainShiftProjection.Period();
			if(!mainShiftPeriod.HasValue)
				return false;

			var period = mainShiftPeriod.Value;
			foreach (var personMeeting in meetings)
			{
				if (!personMeeting.Period.Intersect(period))
					return false;

				foreach (var visibleLayer in mainShiftProjection)
				{
					var activity = visibleLayer.Payload as IActivity;
					if (activity != null && !activity.AllowOverwrite && visibleLayer.Period.Intersect(personMeeting.Period))
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

		public bool CheckTimePersonAssignment(IEditableShift mainShift, IPersonAssignment personAssignment)
		{
			var mainShiftProjection = mainShift.ProjectionService().CreateProjection();
			var worktime = mainShiftProjection.WorkTime();
			var contractTime = mainShiftProjection.ContractTime();
			var clone = mainShift.MakeCopy();

			var mainShiftProjectionPeriod = mainShiftProjection.Period();
			if (!mainShiftProjectionPeriod.HasValue)
				return false;

			var period = mainShiftProjectionPeriod.Value;

			if (!personAssignment.Period.Intersect(period))
				return false;

			foreach (var mainShiftLayer in mainShift.LayerCollection)
			{
				if (mainShiftLayer.Payload.AllowOverwrite)
					continue;
				foreach (var personalLayer in personAssignment.PersonalActivities())
				{
					if (mainShiftLayer.Period.Intersect(personalLayer.Period))
						return false;
				}
			}

			clone.LayerCollection.AddRange(personAssignment.PersonalActivities().Select(personalLayer => new EditableShiftLayer(personalLayer.Payload, personalLayer.Period)));
			
			var mainWithPersonalShiftProjection = clone.ProjectionService().CreateProjection();
			var mainWithPersonalShiftWorkTime = mainWithPersonalShiftProjection.WorkTime();
			var mainWithPersonalShiftContractTime = mainWithPersonalShiftProjection.ContractTime();

			return (worktime == mainWithPersonalShiftWorkTime) && (contractTime == mainWithPersonalShiftContractTime);
		}
	}
}
