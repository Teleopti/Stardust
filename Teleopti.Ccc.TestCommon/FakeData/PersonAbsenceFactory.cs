using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class PersonAbsenceFactory
    {
        public static IPersonAbsence CreatePersonAbsence(IPerson person, IScenario scenario, DateTimePeriod period)
        {
            return new PersonAbsence(person, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("sdf"), period));
        }

		public static IPersonAbsence CreatePersonAbsence(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence)
		{
			return new PersonAbsence(person, scenario, new AbsenceLayer(absence, period));
		}
    }
}
