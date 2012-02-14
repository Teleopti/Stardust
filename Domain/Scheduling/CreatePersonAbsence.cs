using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class CreatePersonAbsence:ICreatePersonAbsence
    {
       

        public IPersonAbsence Create(IAbsence absence, DateTimePeriod period, IScenario scenario, IPerson person)
        {
            return new PersonAbsence(person, scenario, new AbsenceLayer(absence, period));
        }
    }
}