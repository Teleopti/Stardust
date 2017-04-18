using System;
using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers
{
	/// <summary>
	/// Compares the Period Override of the schedule period tab in people admin form.
	/// </summary>
	/// <remarks>
	/// Created By: tamasb
	/// Created Date: 15-06-2012
	/// </remarks>
	public class SchedulePeriodTimeOverrideComparer : IComparer<SchedulePeriodModel>
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

			if (x.PeriodTime == TimeSpan.MinValue && y.PeriodTime == TimeSpan.MinValue)
			{
				// No need to set the value since the deault value equal to 0
			}
			else if (x.PeriodTime == TimeSpan.MinValue)
			{
				result = -1;
			}
			else if (y.PeriodTime == TimeSpan.MinValue)
			{
				result = 1;
			}
			else
			{
				if (x.PeriodTime < y.PeriodTime)
				{
					result = -1;
				}
				if (x.PeriodTime > y.PeriodTime)
				{
					result = 1;
				}
			}

			return result;
		}

		#endregion
	}
}

