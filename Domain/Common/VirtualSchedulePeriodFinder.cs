using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class VirtualSchedulePeriodFinder
	{
		private IPerson _person;
		
		public VirtualSchedulePeriodFinder(IPerson person)
		{
			_person = person;
		}

		public IList<IVirtualSchedulePeriod> FindVirtualPeriods(DateOnlyPeriod period)
		{
			IEnumerable<DateOnly> dates = period.DayCollection();
			IList<IVirtualSchedulePeriod> virtualSchedulePeriods = new List<IVirtualSchedulePeriod>();
			IVirtualSchedulePeriod virtualSchedulePeriod;
			
			foreach (DateOnly date in dates)
			{
				virtualSchedulePeriod = _person.VirtualSchedulePeriod(date);
                if (!virtualSchedulePeriods.Contains(virtualSchedulePeriod) && virtualSchedulePeriod.Number != 0 && virtualSchedulePeriod.IsValid)
					virtualSchedulePeriods.Add(virtualSchedulePeriod);
			}

			return virtualSchedulePeriods;
		}
	}
}
