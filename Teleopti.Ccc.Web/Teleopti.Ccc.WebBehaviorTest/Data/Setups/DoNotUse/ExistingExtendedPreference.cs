using System;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ExistingExtendedPreference : BasePreference
	{
		private readonly StartTimeLimitation _startTimeLimitation;
		private readonly EndTimeLimitation _endTimeLimitation;
		private readonly WorkTimeLimitation _workTimeLimitation;

		public ExistingExtendedPreference(StartTimeLimitation startTimeLimitation)
		{
			_startTimeLimitation = startTimeLimitation;
			_endTimeLimitation = new EndTimeLimitation();
			_workTimeLimitation = new WorkTimeLimitation();
		}

		public ExistingExtendedPreference(EndTimeLimitation endTimeLimitation)
		{
			_startTimeLimitation = new StartTimeLimitation();
			_endTimeLimitation = endTimeLimitation;
			_workTimeLimitation = new WorkTimeLimitation();
		}

		public ExistingExtendedPreference(WorkTimeLimitation workTimeLimitation)
		{
			_startTimeLimitation = new StartTimeLimitation();
			_endTimeLimitation = new EndTimeLimitation();
			_workTimeLimitation = workTimeLimitation;
		}

		protected override PreferenceRestriction ApplyRestriction(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PreferenceRestriction
			{
				StartTimeLimitation = _startTimeLimitation,
				EndTimeLimitation = _endTimeLimitation,
				WorkTimeLimitation = _workTimeLimitation
			};
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Parse(Date,SwedishCultureInfo);
		}
	}
}