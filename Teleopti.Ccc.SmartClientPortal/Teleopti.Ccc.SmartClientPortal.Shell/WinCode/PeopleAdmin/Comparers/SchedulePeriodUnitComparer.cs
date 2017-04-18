using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the Unit of the schedule period data.
	/// </summary>
	/// <remarks>
	/// Created By: madhurangap
	/// Created Date: 25-07-2008
	/// </remarks>
	public class SchedulePeriodUnitComparer : IComparer<SchedulePeriodModel>
	{
		#region IComparer<SchedulePeriodModel> Members

		/// <summary>
		/// Comparese the start date of two objects objects.
		/// </summary>
		/// <param name="x">A Person Period Grid Data object</param>
		/// <param name="y">A Person Period Grid Data object</param>
		/// <returns>Result of the comparisom</returns>
		public int Compare(SchedulePeriodModel x, SchedulePeriodModel y)
		{
			int result = 0;

			if (x.PeriodType == null && y.PeriodType == null)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.PeriodType == null)
			{
				result = -1;
			}
			else if (y.PeriodType == null)
			{
				result = 1;
			}
			else 
			{
				//// Compares the teminal date of the y with the teminal date of y
				result = string.Compare(((SchedulePeriodType)x.PeriodType).ToString(), ((SchedulePeriodType)y.PeriodType).ToString()
				    , StringComparison.CurrentCulture);
			}

			return result;
		}

		#endregion
	}
}
