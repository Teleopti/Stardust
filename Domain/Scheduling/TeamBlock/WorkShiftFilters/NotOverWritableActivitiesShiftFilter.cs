using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface INotOverWritableActivitiesShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly dateToSchedule, IPerson person,
															IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
	}

	public class NotOverWritableActivitiesShiftFilter : INotOverWritableActivitiesShiftFilter
	{
		private readonly Func<IScheduleDayForPerson> _scheduleDayForPerson;

		public NotOverWritableActivitiesShiftFilter(Func<IScheduleDayForPerson> scheduleDayForPerson)
		{
			_scheduleDayForPerson = scheduleDayForPerson;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly dateToSchedule, IPerson person,
		                                           IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (person == null) return null;
			if (finderResult == null) return null;
			
			if (shiftList.Count == 0) return shiftList;
			var part = _scheduleDayForPerson().ForPerson(person,dateToSchedule);

			var filteredList = new List<IShiftProjectionCache>();
			var meetings = part.PersonMeetingCollection();
			var personAssignment = part.PersonAssignment(true);
			var cnt = shiftList.Count;

			if (meetings.Count == 0 && !personAssignment.PersonalActivities().Any())
				return shiftList;

			foreach (var shift in shiftList)
			{
				if (shift.MainShiftProjection.Any(x => !((VisualLayer) x).HighestPriorityActivity.AllowOverwrite &&
				                                       isActivityIntersectedWithMeetingOrPersonalShift(
					                                       personAssignment, meetings, x)))
					continue;
				filteredList.Add(shift);
			}
			finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.AfterCheckingAgainstActivities,
			                                                        cnt, filteredList.Count));

			return filteredList;
		}

		private static bool isActivityIntersectedWithMeetingOrPersonalShift(IPersonAssignment personAssignment,
			IEnumerable<IPersonMeeting> meetings,
			IVisualLayer layer)
		{
			if (meetings.Any(x => x.Period.Intersect(layer.Period)))
				return true;

			if (personAssignment.PersonalActivities().Any(l => l.Period.Intersect(layer.Period)))
				return true;

			return false;
		}
	}
}
