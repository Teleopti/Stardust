namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Responsible to update all the person absence accounts
	/// </summary>
	public interface IPersonAccountUpdater
	{
		/// <summary>
		/// Updates the person absence accounts on activation / on termination
		/// </summary>
		void Update(IPerson person);

		bool UpdateForAbsence (IPerson person, IAbsence absence, DateOnly personAbsenceStartDate);

		IPersonAbsenceAccount FetchPersonAbsenceAccount(IPerson person, IAbsence absence);
	}
}