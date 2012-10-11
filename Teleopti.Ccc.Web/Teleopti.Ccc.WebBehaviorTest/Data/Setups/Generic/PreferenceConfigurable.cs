using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PreferenceConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public bool IsExtended { get; set; }
		public bool MustHave { get; set; }
		public string ShiftCategory { get; set; }
		public string Dayoff { get; set; }
		public string Absence { get; set; }
		public string WorkTimeMinimum { get; set; }
		public string WorkTimeMaximum { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var restriction = new PreferenceRestriction();

			if (IsExtended) // shortcut to create an extended preference
				restriction.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(8));

			if (ShiftCategory != null)
			{
				var shiftCategoryRepository = new ShiftCategoryRepository(uow);
				var shiftCategory = shiftCategoryRepository.LoadAll().Single(b => b.Description.Name == ShiftCategory);
				restriction.ShiftCategory = shiftCategory;
			}

			if (Dayoff != null)
			{
				var dayOffRepository = new DayOffRepository(uow);
				var dayOffTemplate = dayOffRepository.LoadAll().Single(b => b.Description.Name == Dayoff);
				restriction.DayOffTemplate = dayOffTemplate;
			}

			if (Absence != null)
			{
				var absenceRepository = new AbsenceRepository(uow);
				var absence = absenceRepository.LoadAll().Single(b => b.Description.Name == Absence);
				restriction.Absence = absence;
			}

			restriction.MustHave = MustHave;

			if (WorkTimeMinimum != null || WorkTimeMaximum != null)
				restriction.WorkTimeLimitation = new WorkTimeLimitation(Transform.ToNullableTimeSpan(WorkTimeMinimum), Transform.ToNullableTimeSpan(WorkTimeMaximum));

			var preferenceDay = new PreferenceDay(user, new DateOnly(Date), restriction);

			var preferenceDayRepository = new PreferenceDayRepository(uow);
			preferenceDayRepository.Add(preferenceDay);
		}
	}
}