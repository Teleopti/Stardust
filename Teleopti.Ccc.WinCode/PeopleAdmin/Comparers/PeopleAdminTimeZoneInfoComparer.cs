using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the time zone data data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 21-07-2008
	/// </remarks>
	public class PeopleAdminTimeZoneInfoComparer : IComparer<PersonGeneralModel>
	{
		#region IComparer<PersonGeneralModel> Members

		/// <summary>
		/// Comparese the time zone information of two objects objects.
		/// </summary>
		/// <param name="x">A People Admin Grid Data object</param>
		/// <param name="y">A People Admin Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonGeneralModel x, PersonGeneralModel y)
		{
			// compares the time zone information of the y with the time zone information of y
			return string.Compare(x.TimeZoneInformation.DisplayName, y.TimeZoneInformation.DisplayName, 
				StringComparison.CurrentCulture);
		}

		#endregion
	}
}
