using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public interface IIsEditablePredicate
    {
        bool IsPreferenceEditable(DateOnly calendarDate, IPerson person);
        bool IsStudentAvailabilityEditable(DateOnly dateOnly, IPerson person);
    }
}