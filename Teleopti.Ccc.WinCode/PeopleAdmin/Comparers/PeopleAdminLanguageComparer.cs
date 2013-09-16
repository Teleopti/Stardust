using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the time zone data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 21-07-2008
	/// </remarks>
	public class PeopleAdminLanguageComparer : IComparer<PersonGeneralModel>
	{
		#region IComparer<PersonGeneralModel> Members

		/// <summary>
		/// Comparese the language of two objects objects.
		/// </summary>
		/// <param name="x">A People Admin Grid Data object</param>
		/// <param name="y">A People Admin Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(PersonGeneralModel x, PersonGeneralModel y)
		{
			int result = 0;

			if (x.LanguageInfo.Id == 0 && y.LanguageInfo.Id == 0)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.LanguageInfo.Id == 0)
			{
				result = -1;
			}
			else if (y.LanguageInfo.Id == 0)
			{
				result = 1;
			}
			else
			{
				// compares the language of the y with the language of y
				result = string.Compare(x.LanguageInfo.DisplayName, 
                    y.LanguageInfo.DisplayName, StringComparison.CurrentCulture);
			}

			return result;
		}

		#endregion
	}
}
