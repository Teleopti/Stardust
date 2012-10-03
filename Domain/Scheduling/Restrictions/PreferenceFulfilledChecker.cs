using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{

	/// <summary>
	/// Checks if the preference is fullfilled for a given day
	/// </summary>
	public interface IPreferenceFulfilledChecker
	{
		/// <summary>
		/// Determines whether the preference is fulfilled for the specified day.
		/// </summary>
		/// <param name="day">The day.</param>
		/// <returns>
		/// <c>true</c> if the pereference is fulfilled
		/// <c>false</c> if the reference is not fulfilled
		/// <c>null</c> if there is no preference to the given day
		/// </returns>
		bool? IsPreferenceFulfilled(IScheduleDay day);
	}

	public class PreferenceFulfilledChecker : IPreferenceFulfilledChecker
	{
		private ICheckerRestriction _restrictionChecker;

		/// <summary>
		/// Sets the restriction checker.
		/// </summary>
		/// <param name="restrictionChecker">The restriction checker.</param>
		/// <remarks>Only for test purposes</remarks>
		public void SetRestrictionChecker(ICheckerRestriction restrictionChecker)
		{
			_restrictionChecker = restrictionChecker;
		}

		/// <summary>
		/// Gets the or create restriction checker.
		/// </summary>
		/// <returns></returns>
		/// <remarks>Only for test purposes</remarks>
		private ICheckerRestriction getOrCreateRestrictionChecker()
		{
			if (_restrictionChecker == null)
				_restrictionChecker = new RestrictionChecker();
			return _restrictionChecker;
		}

		public bool? IsPreferenceFulfilled(IScheduleDay day)
		{

			var restrictionChecker = getOrCreateRestrictionChecker();
			restrictionChecker.ScheduleDay = day;
			var result = restrictionChecker.CheckPreference();
			switch (result)
			{
				case PermissionState.Satisfied:
					return true;
				case PermissionState.Broken:
					return false;
				default:
					return null;
			}

		}
	}
}
