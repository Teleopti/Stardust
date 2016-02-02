using Teleopti.Interfaces.Domain;

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

		public int CallCount { get; private set; }
	}
}	