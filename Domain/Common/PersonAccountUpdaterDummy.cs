using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class PersonAccountUpdaterDummy : IPersonAccountUpdater
	{
        public void Update(IPerson person)
        {
	        CallCount++;
        }

		public bool UpdateForAbsence (IPerson person, IAbsence absence, DateOnly personAbsenceStartDate)
		{
			CallCount++;
			return true;
		}

		public IPersonAbsenceAccount FetchPersonAbsenceAccount(IPerson person, IAbsence absence)
		{
			return null;
		}

		public int CallCount { get; private set; }
	}
}	