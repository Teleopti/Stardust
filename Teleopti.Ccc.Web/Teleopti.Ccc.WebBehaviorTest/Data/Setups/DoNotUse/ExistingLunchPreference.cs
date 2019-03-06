using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ExistingLunchPreference : BasePreference
	{
		public string LunchActivity;

		private readonly StartTimeLimitation _start;
		private readonly EndTimeLimitation _end;
		private readonly WorkTimeLimitation _length;

		public ExistingLunchPreference(WorkTimeLimitation workTimeLimitation)
		{
			_start = new StartTimeLimitation();
			_end = new EndTimeLimitation();
			_length = workTimeLimitation;
		}

		public ExistingLunchPreference(EndTimeLimitation endTimeLimitation)
		{
			_start = new StartTimeLimitation();
			_end = endTimeLimitation;
			_length = new WorkTimeLimitation();
		}

		public ExistingLunchPreference(StartTimeLimitation startTimeLimitation)
		{
			_start = startTimeLimitation;
			_end = new EndTimeLimitation();
			_length = new WorkTimeLimitation();
		}

		protected override PreferenceRestriction ApplyRestriction(ICurrentUnitOfWork currentUnitOfWork)
		{
			IActivity lunchActivity;
			if (LunchActivity == null)
			{
				lunchActivity = new Activity(RandomName.Make()) { DisplayColor = Color.FromKnownColor(KnownColor.Yellow) };
				var activityRepository = ActivityRepository.DONT_USE_CTOR(currentUnitOfWork, null, null);
				activityRepository.Add(lunchActivity);
			}
			else
			{
				var activityRepository = ActivityRepository.DONT_USE_CTOR(currentUnitOfWork, null, null);
				lunchActivity = activityRepository.LoadAll().FirstOrDefault(a => a.Description.Name == LunchActivity);
			}
			var activityRestriction = new ActivityRestriction(lunchActivity)
			                          	{StartTimeLimitation = _start, EndTimeLimitation = _end, WorkTimeLimitation = _length};
			var preferenceRestriction = new PreferenceRestriction();
			preferenceRestriction.AddActivityRestriction(activityRestriction);
			return preferenceRestriction;
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
            return DateTime.Parse(Date,SwedishCultureInfo);
		}
	}
}