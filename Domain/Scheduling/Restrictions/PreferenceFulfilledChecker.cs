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
		public bool? IsPreferenceFulfilled(IScheduleDay day)
		{

			var restrictionChecker = new RestrictionChecker {ScheduleDay = day};
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
