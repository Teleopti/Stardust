using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the culture data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 21-07-2008
	/// </remarks>
	public class PeopleAdminTerminalDateComparer : IComparer<PersonGeneralModel>
	{
		#region IComparer<PersonGeneralModel> Members

		/// <summary>
		/// Comparese the terminal date of two objects objects.
		/// </summary>
		/// <param name="x">A People Admin Grid Data object</param>
		/// <param name="y">A People Admin Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonGeneralModel x, PersonGeneralModel y)
		{
			int result = 0;

			if (x.TerminalDate == null && y.TerminalDate == null)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.TerminalDate == null)
			{
				result = -1;
			}
			else if (y.TerminalDate == null)
			{
				result = 1;
			}
			else
			{
				// compares the teminal date of the y with the teminal date of y
				result = DateTime.Compare((DateTime)x.TerminalDate, (DateTime)y.TerminalDate);
			}

			return result;
		}

		#endregion
	}
}
