using System.Drawing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class AbsenceCreator
    {
        public IAbsence Create(Description description, Color color, byte priority, bool requestable)
        {
            IAbsence absence = new Absence
                                   {
                                       Description = description,
                                       DisplayColor = color,
                                       Priority = priority,
                                       Requestable = requestable
                                   };
            return absence;
        }
    }
}
