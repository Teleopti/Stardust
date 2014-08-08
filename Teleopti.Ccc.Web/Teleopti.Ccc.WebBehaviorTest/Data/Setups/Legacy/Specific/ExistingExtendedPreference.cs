using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
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

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
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