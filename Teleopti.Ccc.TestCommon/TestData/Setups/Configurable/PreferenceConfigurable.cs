using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PreferenceConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public bool IsExtended { get; set; }
		public bool MustHave { get; set; }
		public string ShiftCategory { get; set; }
		public string Preference { get; set; } //same as the ShiftCategory
		public string Dayoff { get; set; }
		public string Absence { get; set; }
		public DateTime? EndTimeMaximum { get; set; }
		public string WorkTimeMinimum { get; set; }
		public string WorkTimeMaximum { get; set; }

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			if (Preference != null)
				ShiftCategory = Preference;
			var restriction = new PreferenceRestriction();

			if (IsExtended) // shortcut to create an extended preference
				restriction.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(8));

			if (EndTimeMaximum.HasValue)
			{
				restriction.EndTimeLimitation = new EndTimeLimitation(null, EndTimeMaximum.Value.TimeOfDay);
			}
			if (ShiftCategory != null)
			{
				var shiftCategoryRepository = new ShiftCategoryRepository(unitOfWork);
				var shiftCategory = shiftCategoryRepository.LoadAll().Single(b => b.Description.Name == ShiftCategory);
				restriction.ShiftCategory = shiftCategory;
			}

			if (Dayoff != null)
			{
				var dayOffRepository = new DayOffTemplateRepository(unitOfWork);
				var dayOffTemplate = dayOffRepository.LoadAll().Single(b => b.Description.Name == Dayoff);
				restriction.DayOffTemplate = dayOffTemplate;
			}

			if (Absence != null)
			{
				var absenceRepository = new AbsenceRepository(unitOfWork);
				var absence = absenceRepository.LoadAll().Single(b => b.Description.Name == Absence);
				restriction.Absence = absence;
			}

			restriction.MustHave = MustHave;

			if (WorkTimeMinimum != null || WorkTimeMaximum != null)
				restriction.WorkTimeLimitation = new WorkTimeLimitation(toNullableTimeSpan(WorkTimeMinimum), toNullableTimeSpan(WorkTimeMaximum));

			var preferenceDay = new PreferenceDay(person, new DateOnly(Date), restriction);

			var preferenceDayRepository = new PreferenceDayRepository(unitOfWork);
			preferenceDayRepository.Add(preferenceDay);
		}

		private static TimeSpan? toNullableTimeSpan(string value)
		{
			if (value == null)
				return null;
			return TimeSpan.Parse(value);
		}
	}
}