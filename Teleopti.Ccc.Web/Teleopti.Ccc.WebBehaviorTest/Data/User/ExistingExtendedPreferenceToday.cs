using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ExistingExtendedPreferenceToday : BasePreference
	{
		private readonly TimeSpan _earliest;
		private readonly TimeSpan _latest;

		public ExistingExtendedPreferenceToday(string earliest, string latest)
		{
			TimeHelper.TryParse(earliest, out _earliest);
			TimeHelper.TryParse(latest, out _latest);
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction()
			                            	{
			                            		EndTimeLimitation = new EndTimeLimitation(_earliest, _latest)
			                            	};
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateTime.Now.Date; }
	}
}