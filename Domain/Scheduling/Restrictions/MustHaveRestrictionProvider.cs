using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public interface IMustHaveRestrictionProvider
	{
		int CountMustHave(DateOnly dateOnly, IPerson person);
	}

	public class MustHaveRestrictionProvider : IMustHaveRestrictionProvider
	{
		private readonly IPreferenceDayRepository _preferenceDayRepository;

		public MustHaveRestrictionProvider(IPreferenceDayRepository preferenceDayRepository)
		{
			_preferenceDayRepository = preferenceDayRepository;
		}

		public int CountMustHave(DateOnly dateOnly, IPerson person)
		{
			var schedulePeriod = person.VirtualSchedulePeriodOrNext(dateOnly).DateOnlyPeriod;
			var preferenceDays = _preferenceDayRepository.Find(schedulePeriod,person);
			return preferenceDays.Count(p => p.Restriction.MustHave);
		}
	}
}
