using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public interface IMustHaveRestrictionSetter
	{
		/// <summary>
		/// update must have for the specific preference day and specific person
		/// </summary>
		/// <param name="dateOnly">the date</param>
		/// <param name="person">the person</param>
		/// <param name="mustHave">must have</param>
		/// <returns>true if must have is true, false otherwise or no preference on that day</returns>
		bool SetMustHave(DateOnly dateOnly, IPerson person, bool mustHave);
	}

	public class MustHaveRestrictionSetter : IMustHaveRestrictionSetter
	{
		private readonly IPreferenceDayRepository _preferenceDayRepository;

		public MustHaveRestrictionSetter(IPreferenceDayRepository preferenceDayRepository)
		{
			_preferenceDayRepository = preferenceDayRepository;
		}

		public bool SetMustHave(DateOnly dateOnly, IPerson person, bool mustHave)
		{
			if (person == null) throw new ArgumentNullException(nameof(person));
			IPreferenceDay preferenceDay;
			if (mustHave)
			{
				var schedulePeriod = person.VirtualSchedulePeriodOrNext(dateOnly).DateOnlyPeriod;
				var preferenceDays = _preferenceDayRepository.Find(schedulePeriod, person);
				var nbrOfDaysWithMustHave = preferenceDays.Count(p => p.Restriction.MustHave);
				var currentSchedulePeriod = person.SchedulePeriod(dateOnly);
				preferenceDay = preferenceDays.SingleOrDefault(d => d.RestrictionDate == dateOnly);
				if (nbrOfDaysWithMustHave >= currentSchedulePeriod.MustHavePreference)
					return preferenceDay != null && preferenceDay.Restriction.MustHave;

			}
			else
			{
				var preferenceDays = _preferenceDayRepository.Find(dateOnly, person);
				preferenceDay = preferenceDays.SingleOrDefault();
			}

			if (preferenceDay != null)
			{
				preferenceDay.Restriction.MustHave = mustHave;
				return mustHave;
			}
			return false;
		}
	}
}