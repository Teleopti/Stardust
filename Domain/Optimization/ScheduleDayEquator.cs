using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IScheduleDayEquator
    {
        bool MainShiftEquals(IScheduleDay original, IScheduleDay current);
		bool MainShiftEquals(IEditableShift original, IEditableShift current);
        bool DayOffEquals(IScheduleDay original, IScheduleDay current);
		bool MainShiftBasicEquals(IEditableShift original, IEditableShift current);
		bool MainShiftBasicEquals(IEditableShift sampleShift, IEditableShift destShift, TimeZoneInfo timeZone);
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
					//mycket märkligt här... gör bara som det var förrut. fattar inte.
	        var originalHasDayOff = original.HasDayOff();
	        var currentHasDayOff = current.HasDayOff();
            if (originalHasDayOff && !currentHasDayOff)
                return true;

            if (originalHasDayOff && currentHasDayOff)
                return true;

            if (original.SignificantPart() == SchedulePartView.MainShift && current.SignificantPart() == SchedulePartView.DayOff)
                return true;

			if (original.SignificantPart() == SchedulePartView.None && current.SignificantPart() == SchedulePartView.MainShift)
				return false;

			if (current.SignificantPart() == SchedulePartView.None && original.SignificantPart() == SchedulePartView.MainShift)
				return false;

	        var originalAss = original.PersonAssignment();
	        if (originalAss == null)
		        return false;
	        var currentAss = current.PersonAssignment();
					if (currentAss != null)
					{
						IEditableShift originalMainShift = _editableShiftMapper.CreateEditorShift(originalAss);
						IEditableShift currentMainShift = _editableShiftMapper.CreateEditorShift(currentAss);
						if (originalMainShift == null || currentMainShift == null)
							return false;

						if (!MainShiftEquals(originalMainShift, currentMainShift))
							return false;
					}
            return true;
        }

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
			return MainShiftBasicEquals(original, current, TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
		}

		public bool MainShiftBasicEquals(IEditableShift original, IEditableShift current, TimeZoneInfo timeZone)
		{
			if (original.ShiftCategory.Id != current.ShiftCategory.Id)
				return false;
			if (original.LayerCollection.Count != current.LayerCollection.Count)
				return false;

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

		private static bool activityEquals(ILayer<IActivity> original, ILayer<IActivity> current)
        {
            return original.Period.Equals(current.Period) && original.Payload.Equals(current.Payload);
        }

        private static bool checkDayOffEqual(IScheduleDay original, IScheduleDay current)
        {
            if (original.SignificantPart() == SchedulePartView.DayOff && current.SignificantPart() == SchedulePartView.DayOff)
                return true;

						//mycket märkligt här... gör bara som det var förrut. fattar inte.
						var originalHasDayOff = original.HasDayOff();
						var currentHasDayOff = current.HasDayOff();

            if (originalHasDayOff && currentHasDayOff)
                return true;

            if (!originalHasDayOff && !currentHasDayOff)
                return true;

            if (originalHasDayOff && !currentHasDayOff)
                return true;

            return false;
        }

    }
}
