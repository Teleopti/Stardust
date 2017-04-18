using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
	public class PeopleAdminTerminalDateComparer : IComparer<PersonGeneralModel>
	{
		/// <summary>
		/// Compares the terminal date of two objects objects.
		/// </summary>
		/// <param name="x">A People Admin Grid Data object</param>
		/// <param name="y">A People Admin Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonGeneralModel x, PersonGeneralModel y)
		{
			int result = 0;

			if (x.TerminalDate.HasValue && y.TerminalDate.HasValue)
			{
				result = DateTime.Compare(x.TerminalDate.Value.Date, y.TerminalDate.Value.Date);
			}
			else if (!x.TerminalDate.HasValue && y.TerminalDate.HasValue)
			{
				result = -1;
			}
			else if (!y.TerminalDate.HasValue && x.TerminalDate.HasValue)
			{
				result = 1;
			}

			return result;
		}
	}
}
