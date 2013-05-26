using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPersonalShiftMeetingTimeChecker
	{
		bool CheckTimeMeeting(IEditorShift mainShift, IVisualLayerCollection mainShiftProjection, ReadOnlyCollection<IPersonMeeting> meetings);
		bool CheckTimePersonAssignment(IEditorShift mainShift, IVisualLayerCollection mainShiftProjection, ReadOnlyCollection<IPersonAssignment> personAssignments);
	}

	public class PersonalShiftMeetingTimeChecker : IPersonalShiftMeetingTimeChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool CheckTimeMeeting(IEditorShift mainShift, IVisualLayerCollection mainShiftProjection, ReadOnlyCollection<IPersonMeeting> meetings)
		{
			var worktime = mainShiftProjection.WorkTime();
			var contractTime = mainShiftProjection.ContractTime();
			var clone = mainShift.NoneEntityClone();
			
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

				var layer = new EditorActivityLayer(personMeeting.BelongsToMeeting.Activity, personMeeting.Period);
				clone.LayerCollection.Add(layer);	
			}

			var mainWithMeetingProjection = clone.ProjectionService().CreateProjection();
			var mainWithMeetingWorkTime = mainWithMeetingProjection.WorkTime();
			var mainWithMeetingContractTime = mainWithMeetingProjection.ContractTime();

			return (worktime == mainWithMeetingWorkTime) && (contractTime == mainWithMeetingContractTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool CheckTimePersonAssignment(IEditorShift mainShift, IVisualLayerCollection mainShiftProjection, ReadOnlyCollection<IPersonAssignment> personAssignments)
		{
			var worktime = mainShiftProjection.WorkTime();
			var contractTime = mainShiftProjection.ContractTime();
			var clone = mainShift.NoneEntityClone();

			if (!mainShiftProjection.Period().HasValue)
				return false;

			var period = mainShiftProjection.Period().Value;

			foreach (var personAssignment in personAssignments)
			{
				if (!personAssignment.Period.Intersect(period))
					return false;

				foreach (var mainShiftLayer in mainShift.LayerCollection)
				{
					if (mainShiftLayer.Payload.AllowOverwrite) continue;
					foreach (var personalShift in personAssignment.PersonalShiftCollection)
					{
						if (!personalShift.LayerCollection.Period().HasValue) continue;
						if (mainShiftLayer.Period.Intersect(personalShift.LayerCollection.Period().Value)) return false;
					}
				}

				foreach (var personalShift in personAssignment.PersonalShiftCollection)
				{
					foreach (var personalLayer in personalShift.LayerCollection)
					{
						var layer = new EditorActivityLayer(personalLayer.Payload, personalLayer.Period);
						clone.LayerCollection.Add(layer);
					}
				}
			}

			var mainWithPersonalShiftProjection = clone.ProjectionService().CreateProjection();
			var mainWithPersonalShiftWorkTime = mainWithPersonalShiftProjection.WorkTime();
			var mainWithPersonalShiftContractTime = mainWithPersonalShiftProjection.ContractTime();

			return (worktime == mainWithPersonalShiftWorkTime) && (contractTime == mainWithPersonalShiftContractTime);
		}
	}
}
