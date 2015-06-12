using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceNightRestCheckResult
	{
		public bool HasViolationToPreviousDay { get; set; }
		public bool HasViolationToNextDay { get; set; }
		public TimeSpan RestTimeToPreviousDay { get; set; }
		public TimeSpan RestTimeToNextDay { get; set; }
		public TimeSpan ExpectedNightRest { get; set; }
	}

	public interface IPreferenceNightRestChecker
	{
		PreferenceNightRestCheckResult CheckNightRestViolation(IPerson person, DateOnly date);
	}
}