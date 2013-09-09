using System;

namespace Teleopti.Interfaces.Domain
{

	/// <summary>
	/// Responsible to update all the person absence accounts
	/// </summary>
	public interface IPersonAccountUpdater
	{
		/// <summary>
		/// Updates the person absence accounts
		/// </summary>
		void UpdatePersonAccounts(DateTime? terminalDate);
	}
}