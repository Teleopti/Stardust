using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IScheduleDayEquator
    {
        bool MainShiftEquals(IScheduleDay original, IScheduleDay current);
		bool MainShiftEquals(IEditableShift original, IEditableShift current);
		bool MainShiftEqualsWithoutPeriod(IEditableShift original, IEditableShift current);
        bool DayOffEquals(IScheduleDay original, IScheduleDay current);
		bool MainShiftBasicEquals(IEditableShift original, IEditableShift current);
    }

    public class ScheduleDayEquator : IScheduleDayEquator
    {
	    private readonly IEditableShiftMapper _editableShiftMapper;

	    public ScheduleDayEquator(IEditableShiftMapper editableShiftMapper)
		{
			_editableShiftMapper = editableShiftMapper;
		}

	    public bool MainShiftEquals(IScheduleDay original, IScheduleDay current)
        {
            if (!checkMainShiftEqual(original, current))
                return false;
            return true;
        }

        public bool DayOffEquals(IScheduleDay original, IScheduleDay current)
        {
            return checkDayOffEqual(original, current);
        }

        private bool checkMainShiftEqual(IScheduleDay original, IScheduleDay current)
        {
            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count == 0)
                return true;

            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count > 0)
                return true;

            if (original.SignificantPart() == SchedulePartView.MainShift && current.SignificantPart() == SchedulePartView.DayOff)
                return true;

			if (original.SignificantPart() == SchedulePartView.None && current.SignificantPart() == SchedulePartView.MainShift)
				return false;

			if (current.SignificantPart() == SchedulePartView.None && original.SignificantPart() == SchedulePartView.MainShift)
				return false;

            for (int assignmentIndex = 0; assignmentIndex < original.PersonAssignmentCollectionDoNotUse().Count; assignmentIndex++)
            {
                if(current.PersonAssignmentCollectionDoNotUse().Count  > 0)
                {
					IEditableShift originalMainShift =_editableShiftMapper.CreateEditorShift(original.PersonAssignmentCollectionDoNotUse()[assignmentIndex]);
					IEditableShift currentMainShift =_editableShiftMapper.CreateEditorShift( current.PersonAssignmentCollectionDoNotUse()[assignmentIndex]);

                    if (originalMainShift == null || currentMainShift == null)
                        return false;

                    if (!MainShiftEquals(originalMainShift, currentMainShift))
                        return false;
                }
                
            }
            return true;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool MainShiftEquals(IEditableShift original, IEditableShift current)
        {
            if(original.ShiftCategory.Id != current.ShiftCategory.Id)
                return false;
            if (original.LayerCollection.Count != current.LayerCollection.Count)
                return false;
            for (int layerIndex = 0; layerIndex < original.LayerCollection.Count; layerIndex++)
            {
                ILayer<IActivity> originalLayer = original.LayerCollection[layerIndex];
                ILayer<IActivity> currentLayer = current.LayerCollection[layerIndex];
                if (!activityEquals(originalLayer, currentLayer))
                    return false;
            }
            return true;
        }

		public bool MainShiftBasicEquals(IEditableShift original, IEditableShift current)
		{
			if (original.ShiftCategory.Id != current.ShiftCategory.Id)
				return false;
			if (original.LayerCollection.Count != current.LayerCollection.Count)
				return false;
			var timeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
			for (int layerIndex = 0; layerIndex < original.LayerCollection.Count; layerIndex++)
			{
				ILayer<IActivity> originalLayer = original.LayerCollection[layerIndex];
				ILayer<IActivity> currentLayer = current.LayerCollection[layerIndex];
				if (!originalLayer.Period.StartDateTimeLocal(timeZone).TimeOfDay.Equals(currentLayer.Period.StartDateTimeLocal(timeZone).TimeOfDay))
					return false;
				if (!originalLayer.Period.EndDateTimeLocal(timeZone).TimeOfDay.Equals(currentLayer.Period.EndDateTimeLocal(timeZone).TimeOfDay))
					return false;
				if (!originalLayer.Payload.Equals(currentLayer.Payload))
					return false;
			}
			return true;
		}

		public bool MainShiftEqualsWithoutPeriod(IEditableShift original, IEditableShift current)
        {
            if (original.ShiftCategory.Id != current.ShiftCategory.Id)
                return false;
            if (original.LayerCollection.Count != current.LayerCollection.Count)
                return false;
            for (int layerIndex = 0; layerIndex < original.LayerCollection.Count; layerIndex++)
            {
                ILayer<IActivity> originalLayer = original.LayerCollection[layerIndex];
                ILayer<IActivity> currentLayer = current.LayerCollection[layerIndex];
                //if (!originalLayer.Period.StartDateTime.TimeOfDay.Equals(currentLayer.Period.StartDateTime.TimeOfDay))
                //    return false;
                //if (!originalLayer.Period.EndDateTime.TimeOfDay.Equals(currentLayer.Period.EndDateTime.TimeOfDay))
                //    return false;
                if (!originalLayer.Payload.Equals(currentLayer.Payload))
                    return false;
            }
            return true;
        }

        private static bool activityEquals(ILayer<IActivity> original, ILayer<IActivity> current)
        {
            return original.Period.Equals(current.Period) && original.Payload.Equals(current.Payload);
        }

        private static bool checkDayOffEqual(IScheduleDay original, IScheduleDay current)
        {
            if (original.SignificantPart() == SchedulePartView.DayOff && current.SignificantPart() == SchedulePartView.DayOff)
                return true;

            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count > 0)
                return true;

            if (original.PersonDayOffCollection().Count == 0 && current.PersonDayOffCollection().Count == 0)
                return true;

            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count == 0)
                return true;

            return false;
        }

    }
}
