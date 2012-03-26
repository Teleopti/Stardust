using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ExistingExtendedPreferenceToday : BasePreference
	{

		private readonly StartTimeLimitation _startTimeLimitation;
		private readonly EndTimeLimitation _endTimeLimitation;

		public ExistingExtendedPreferenceToday(string earliestStart, string latestStart, string earliestEnd, string latestEnd)
		{
			if (earliestStart != null && latestStart != null)
			{
				TimeSpan earliestStartTime;
				TimeSpan latestStartTime;
				TimeHelper.TryParse(earliestStart, out earliestStartTime);
				TimeHelper.TryParse(latestStart, out latestStartTime);
				_startTimeLimitation = new StartTimeLimitation(earliestStartTime, latestStartTime);
			}

			if (earliestEnd != null && latestEnd != null)
			{
				TimeSpan earliestEndTime;
				TimeSpan latestEndTime;
				TimeHelper.TryParse(earliestEnd, out earliestEndTime);
				TimeHelper.TryParse(latestEnd, out latestEndTime);
				_endTimeLimitation = new EndTimeLimitation(earliestEndTime, latestEndTime);
			}
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction()
			                            	{
			                            		StartTimeLimitation = _startTimeLimitation,
												EndTimeLimitation = _endTimeLimitation
			                            	};
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateTime.Now.Date; }
	}
}