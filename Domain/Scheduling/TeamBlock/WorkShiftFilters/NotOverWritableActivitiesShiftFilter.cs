using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface INotOverWritableActivitiesShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IScheduleDictionary scheduleDictionary, DateOnly dateToSchedule, IPerson person,
															IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult);
	}

	public class NotOverWritableActivitiesShiftFilter : INotOverWritableActivitiesShiftFilter
	{
		public IList<ShiftProjectionCache> Filter(IScheduleDictionary scheduleDictionary, DateOnly dateToSchedule, IPerson person,
		                                           IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (person == null) return null;
			if (finderResult == null) return null;
			
			if (shiftList.Count == 0) return shiftList;
			var part = scheduleDictionary[person].ScheduledDay(dateToSchedule);

			var meetings = part.PersonMeetingCollection();
			var personAssignment = part.PersonAssignment(true);
			var cnt = shiftList.Count;

			if (meetings.Count == 0 && !personAssignment.PersonalActivities().Any())
				return shiftList;

			var filteredList =
				shiftList.Where(
					shift =>
						!shift.MainShiftProjection.Any(
							x =>
								!((VisualLayer) x).HighestPriorityActivity.AllowOverwrite &&
								isActivityIntersectedWithMeetingOrPersonalShift(personAssignment, meetings, x))).ToList();
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
