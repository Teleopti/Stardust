using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceNightRestCheckResult
	{
		public bool HasViolationToPreviousDay;
		public bool HasViolationToNextDay;
		public TimeSpan ExpectedNightRest;
	}

	public interface IPreferenceNightRestChecker
	{
		PreferenceNightRestCheckResult CheckNightRestViolation(IPerson person, DateOnly date);
	}
}