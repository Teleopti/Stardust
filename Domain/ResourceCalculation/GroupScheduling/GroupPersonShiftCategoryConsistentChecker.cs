using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public class GroupPersonShiftCategoryConsistentChecker : IGroupPersonShiftCategoryConsistentChecker
	{
		private IShiftCategory _category;
		public bool AllPersonsHasSameOrNoneShiftCategoryScheduled(IScheduleDictionary scheduleDictionary, IList<IPerson> persons, DateOnly dateOnly)
		{
			InParameter.NotNull("persons",persons);
			InParameter.NotNull("scheduleDictionary", scheduleDictionary);
			_category = null;
			foreach (var person in persons)
			{
                if(!person.VirtualSchedulePeriod(dateOnly).IsValid)
                    continue;
				var range = scheduleDictionary[person];
				var day = range.ScheduledDay(dateOnly);
				if (day.SignificantPart().Equals(SchedulePartView.MainShift))
				{
					if (_category == null)
						_category = day.PersonAssignment().ShiftCategory;

					if (!_category.Equals(day.PersonAssignment().ShiftCategory))
						return false;
				}
			}
			return true;
		}

		public IShiftCategory CommonShiftCategory
		{
			get { return _category; }
		}
	}
}