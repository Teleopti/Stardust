using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    public interface IPreferenceContainerDto
    {
        Guid? Id { get; set; }

        ShiftCategoryDto ShiftCategory { get; set; }

        DayOffInfoDto DayOff { get; set; }

        AbsenceDto Absence { get; set; }

        ICollection<ActivityRestrictionDto> ActivityRestrictionCollection { get; }

        TimeLimitationDto StartTimeLimitation { get; set; }

        TimeLimitationDto EndTimeLimitation { get; set; }

        TimeLimitationDto WorkTimeLimitation { get; set; }

        string LimitationStartTimeString { get; set; }

        string LimitationEndTimeString { get; set; }
    }
}