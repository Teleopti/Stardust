using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class PreferenceAbsenceChecker : IPreferenceAbsenceChecker
    {
        private readonly IScheduleDay _scheduleDay;

        public PreferenceAbsenceChecker(IScheduleDay scheduleDay)
        {
            _scheduleDay = scheduleDay;
        }

        public PermissionState CheckPreferenceAbsence(IPreferenceRestriction preference, PermissionState permissionState)
        {
            if (preference != null && preference.Absence != null)
            {
                var significantPart = _scheduleDay.SignificantPart();

                if (significantPart != SchedulePartView.FullDayAbsence)
                    return PermissionState.Broken;

                if (significantPart == SchedulePartView.FullDayAbsence)
                {
                    var visualLayerCollection = _scheduleDay.ProjectionService().CreateProjection();

                    var visualLayers = visualLayerCollection.FilterLayers(preference.Absence);

                    return !visualLayers.HasLayers ? PermissionState.Broken : PermissionState.Satisfied;
                }
            }

            return permissionState;
        }   
    }
}
