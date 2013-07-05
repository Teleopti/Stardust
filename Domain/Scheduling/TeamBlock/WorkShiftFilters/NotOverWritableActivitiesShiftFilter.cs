using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private readonly ISchedulingResultStateHolder _resultStateHolder;

		public NotOverWritableActivitiesShiftFilter(ISchedulingResultStateHolder resultStateHolder)
		{
			_resultStateHolder = resultStateHolder;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly dateToSchedule, IPerson person,
		                                           IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (person == null) return null;
			if (finderResult == null) return null;
			
			if (shiftList.Count == 0) return shiftList;
			var part = _resultStateHolder.Schedules[person].ScheduledDay(dateToSchedule);

			var filteredList = new List<IShiftProjectionCache>();
			var meetings = part.PersonMeetingCollection();
			var personAssignments = part.PersonAssignmentCollection();
			var cnt = shiftList.Count;

			if (meetings.Count == 0 && personAssignments.Count == 0)
				return shiftList;

			foreach (var shift in shiftList)
			{
				if (shift.MainShiftProjection.Any(x => !((VisualLayer) x).HighestPriorityActivity.AllowOverwrite &&
				                                       isActivityIntersectedWithMeetingOrPersonalShift(
					                                       personAssignments, meetings, x)))
					continue;
				filteredList.Add(shift);
			}
			finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.AfterCheckingAgainstActivities,
			                                                        cnt, filteredList.Count));

			return filteredList;
		}

		private static bool isActivityIntersectedWithMeetingOrPersonalShift(IEnumerable<IPersonAssignment> personAssignments,
		                                                                    IEnumerable<IPersonMeeting> meetings,
		                                                                    IVisualLayer layer)
		{
			if (meetings.Any(x => x.Period.Intersect(layer.Period)))
				return true;

			foreach (var personAssignment in personAssignments)
			{
				if (personAssignment.PersonalLayers().Any(l => l.Period.Intersect(layer.Period)))
					return true;
			}
			return false;
		}
	}
}
