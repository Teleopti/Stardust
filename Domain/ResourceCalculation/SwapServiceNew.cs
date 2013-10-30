using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class SwapServiceNew : ISwapServiceNew
	{
		private IList<IScheduleDay> _selectedSchedules;

		public void Init(IList<IScheduleDay> selectedSchedules)
		{
			_selectedSchedules = selectedSchedules;
		}

		public bool CanSwapAssignments()
		{
			if (!CheckBasicRules())
				return false;
			return true;
		}

		public IList<IScheduleDay> Swap(IScheduleDictionary schedules)
		{
			if(schedules == null)
				throw new ArgumentNullException("schedules");

			if (!CanSwapAssignments())
				throw new ConstraintException("Can not swap assignments");

			var retList = new List<IScheduleDay>();

			var schedulePart1 = schedules[_selectedSchedules[1].Person].ReFetch(_selectedSchedules[1]);
			var schedulePart2 = schedules[_selectedSchedules[0].Person].ReFetch(_selectedSchedules[0]);
			if ((schedulePart1.PersonAssignmentCollection().Count == 0 || schedulePart2.PersonAssignmentCollection().Count == 0) &&
				(schedulePart1.PersonDayOffCollection().Count == 0 && schedulePart2.PersonDayOffCollection().Count == 0))
			{
				if (schedulePart1.PersonAssignmentCollection().Count == 0)
				{
					_selectedSchedules[1].Merge(schedulePart2, false);
					_selectedSchedules[1].DeletePersonalStuff();
					_selectedSchedules[0].DeleteMainShift(schedulePart2);
				}
				else if (schedulePart2.PersonAssignmentCollection().Count == 0)
				{
					_selectedSchedules[0].Merge(schedulePart1, false);
					_selectedSchedules[0].DeletePersonalStuff();
					_selectedSchedules[1].DeleteMainShift(schedulePart1);
				}
			}
			else
			{
				if (schedulePart1.PersistableScheduleDataCollection().Count() == 0)
					_selectedSchedules[0].Merge(_selectedSchedules[0], true);
				else
					_selectedSchedules[0].Merge(schedulePart1, false);


				if(schedulePart2.PersistableScheduleDataCollection().Count() == 0)
					_selectedSchedules[1].Merge(_selectedSchedules[1], true);
				else
					_selectedSchedules[1].Merge(schedulePart2, false);
			}

			((ExtractedSchedule)_selectedSchedules[1]).DeleteOvertime();
			((ExtractedSchedule)_selectedSchedules[1]).MergeOvertime(schedulePart2);
			((ExtractedSchedule)_selectedSchedules[0]).DeleteOvertime();
			((ExtractedSchedule)_selectedSchedules[0]).MergeOvertime(schedulePart1);
			

			retList.AddRange(_selectedSchedules);
			return retList;
		}

		public IList<IScheduleDay> Swap(IList<IScheduleDay> selectedSchedules, IScheduleDictionary schedules)
		{
			Init(selectedSchedules);
			return Swap(schedules);
		}

		private bool CheckBasicRules()
		{
			if (_selectedSchedules.Count != 2)
				return false;

			if (_selectedSchedules[0].Person == _selectedSchedules[1].Person)
				return false;

			return true;
		}
	}
}
