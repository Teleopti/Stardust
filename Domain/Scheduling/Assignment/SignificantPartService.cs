using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class SignificantPartService : ISignificantPartService
    {
        private readonly ISignificantPartProvider _source;


        public static SignificantPartService CreateService(IScheduleDay part)
        {
            return new SignificantPartService(new SchedulePartSignificantPartDefinitions(part, new HasContractDayOffDefinition()));
        }

        public static SignificantPartService CreateService(ISignificantPartProvider source)
        {
            return new SignificantPartService(source);
        }

        public static SignificantPartService CreateServiceForDisplay(IScheduleDay part)
        {
            return new SignificantPartService(new SchedulePartSignificantPartForDisplayDefinitions(part, new HasContractDayOffDefinition()));
        }

        private SignificantPartService(ISignificantPartProvider source)
        {
            InParameter.NotNull(nameof(source), source);
            _source = source;
        }

        public SchedulePartView SignificantPart()
        {
            using (_source.BeginRead())
            {
                if (_source.HasContractDayOff()) return SchedulePartView.ContractDayOff;
                if (_source.HasDayOff() && _source.HasFullAbsence())
                    return SchedulePartView.DayOff;
                if (_source.HasFullAbsence()) return SchedulePartView.FullDayAbsence;
                if (_source.HasAssignment())
                {
                    if (_source.HasMainShift()) return SchedulePartView.MainShift;
                    if (_source.HasPersonalShift())
                    {
                        return _source.HasDayOff() ? SchedulePartView.DayOff : SchedulePartView.PersonalShift;
                    }
                }
                if (_source.HasDayOff()) return SchedulePartView.DayOff;
				if (_source.HasOvertimeShift()) return SchedulePartView.Overtime;
				if (_source.HasAbsence()) return SchedulePartView.Absence;
                if (_source.HasPreferenceRestriction()) return SchedulePartView.PreferenceRestriction;
                if (_source.HasStudentAvailabilityRestriction())
                    return SchedulePartView.StudentAvailabilityRestriction;

                return SchedulePartView.None;
            }
        }
    }
}
