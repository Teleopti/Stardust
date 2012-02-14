using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public interface IEditStudentAvailabilityModel
    {
        TimeSpan? StartTimeLimitation { get; set; }
        TimeSpan? EndTimeLimitation { get; set; }
        bool EndTimeLimitationNextDay { get; set; }
        TimeSpan? SecondStartTimeLimitation { get; set; }
        TimeSpan? SecondEndTimeLimitation { get; set; }
        bool SecondEndTimeLimitationNextDay { get; set; }
        IList<StudentAvailabilityRestriction> StudentAvailabilityRestrictions { get; }
        bool CreateStudentAvailabilityIsPermitted { get; }
        void SetStudentAvailabilityRestrictions(IList<StudentAvailabilityRestriction> studentAvailabilityRestrictions);
        void SetValuesToStudentAvailabilityRestrictions();
    }
}