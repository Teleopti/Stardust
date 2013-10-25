using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific
{
	public class ExistingExtendedPreferenceToday : BasePreference
	{

		private readonly StartTimeLimitation _startTimeLimitation;
		private readonly EndTimeLimitation _endTimeLimitation;
		private readonly WorkTimeLimitation _workTimeLimitation;

		public ExistingExtendedPreferenceToday(StartTimeLimitation startTimeLimitation)
		{
			_startTimeLimitation = startTimeLimitation;
			_endTimeLimitation = new EndTimeLimitation();
			_workTimeLimitation = new WorkTimeLimitation();
		}

		public ExistingExtendedPreferenceToday(EndTimeLimitation endTimeLimitation)
		{
			_startTimeLimitation = new StartTimeLimitation();
			_endTimeLimitation = endTimeLimitation;
			_workTimeLimitation = new WorkTimeLimitation();
		}

		public ExistingExtendedPreferenceToday(WorkTimeLimitation workTimeLimitation)
		{
			_startTimeLimitation = new StartTimeLimitation();
			_endTimeLimitation = new EndTimeLimitation();
			_workTimeLimitation = workTimeLimitation;
		}

		protected override PreferenceRestriction ApplyRestriction()
		{
			return new PreferenceRestriction
			                            	{
			                            		StartTimeLimitation = _startTimeLimitation,
												EndTimeLimitation = _endTimeLimitation,
												WorkTimeLimitation = _workTimeLimitation
			                            	};
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo) { return DateOnlyForBehaviorTests.TestToday.Date; }
	}
}