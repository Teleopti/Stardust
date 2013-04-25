using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public interface IScheduleHelper
    {
        ICollection<ValidatedSchedulePartDto> Validate(PersonDto loggedOnPerson, DateOnly dateInPeriod, bool useStudentAvailability);
    }
}