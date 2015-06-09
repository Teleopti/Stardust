using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceNightRestCheckResult
	{
		public bool HasViolation;
		public string Message;
		public TimeSpan ExpectedNightRest;
	}

	public interface IPreferenceNightRestChecker
	{
		PreferenceNightRestCheckResult CheckNightRestViolation(DateOnly date);
	}
}