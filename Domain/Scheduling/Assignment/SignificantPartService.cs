using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class SignificantPartService : ISignificantPartService
    {
        private readonly ISignificantPartProvider _source;


        public static SignificantPartService CreateService(IScheduleDay part)
        {
            return new SignificantPartService(new SchedulePartSignificantPartDefinitions(part, new HasDayOffDefinition(part)));
        }

        public static SignificantPartService CreateService(ISignificantPartProvider source)
        {
            return new SignificantPartService(source);
        }

        public static SignificantPartService CreateServiceForDisplay(IScheduleDay part)
        {
            return new SignificantPartService(new SchedulePartSignificantPartForDisplayDefinitions(part, new HasDayOffDefinition(part)));
        }

        public static SignificantPartService CreateServiceForDisplay(ISignificantPartProvider source)
        {
            return new SignificantPartService(source);
        }
        private SignificantPartService(ISignificantPartProvider source)
        {
            InParameter.NotNull("source", source);
            _source = source;
        }

        public SchedulePartView SignificantPart()
        {
            using (_source.BeginRead())
            {
                if (_source.HasContractDayOff()) return SchedulePartView.ContractDayOff;
                if (_source.HasDayOff() && _source.HasFullAbsence())
                    return SchedulePartView.FullDayAbsence;
                if (_source.HasFullAbsence()) return SchedulePartView.FullDayAbsence;
                if (_source.HasAssignment())
                {
                    if (_source.HasMainShift()) return SchedulePartView.MainShift;
                    if (_source.HasPersonalShift())
                    {
                        return
                            _source.HasDayOff() ? SchedulePartView.DayOff : SchedulePartView.PersonalShift;
                    }
                }
                if (_source.HasDayOff()) return SchedulePartView.DayOff;
                if (_source.HasAbsence()) return SchedulePartView.Absence;
                if (_source.HasPersonalShift()) return SchedulePartView.Absence;
                if (_source.HasOvertimeShift()) return SchedulePartView.Overtime;
                if (_source.HasPreferenceRestriction()) return SchedulePartView.PreferenceRestriction;
                if (_source.HasStudentAvailabilityRestriction())
                    return SchedulePartView.StudentAvailabilityRestriction;

                return SchedulePartView.None;
            }
        }
    }
}
