﻿using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class SwapService : ISwapService
    {
        private IList<IScheduleDay> _selectedSchedules;

        public void Init(IList<IScheduleDay> selectedSchedules)
        {
            _selectedSchedules = selectedSchedules;
        }

        public bool CanSwapAssignments()
        {
            if(!checkBasicRules())
                return false;
            return true;
        }

        public IList<IScheduleDay> SwapAssignments(IScheduleDictionary schedules, bool ignoreAssignmentPermission)
        {
            if(!CanSwapAssignments())
                throw new ConstraintException("Can not swap assignments");

            List<IScheduleDay> retList = new List<IScheduleDay>();

            IScheduleDay schedulePart1 = schedules[_selectedSchedules[1].Person].ReFetch(_selectedSchedules[1]);
            IScheduleDay schedulePart2 = schedules[_selectedSchedules[0].Person].ReFetch(_selectedSchedules[0]);
	        var ass1 = schedulePart1.PersonAssignment();
	        var ass2 = schedulePart2.PersonAssignment();

			removeAbsences(schedulePart1);
			removeAbsences(schedulePart2);

			if ((ass1==null || ass2==null) && (!schedulePart1.HasDayOff() && !schedulePart2.HasDayOff()))
            {
                if (ass1 == null)
                {
                    _selectedSchedules[1].Merge(schedulePart2, false,true);
                    _selectedSchedules[1].DeletePersonalStuff();
                    _selectedSchedules[0].DeleteMainShift();
                }
                else
                {
                    _selectedSchedules[0].Merge(schedulePart1, false,true);
                    _selectedSchedules[0].DeletePersonalStuff();
                    _selectedSchedules[1].DeleteMainShift();
                }
            }
            else
            {
				if (!schedulePart2.IsScheduled() && schedulePart1.IsScheduled())
	            {
					_selectedSchedules[0].Merge(schedulePart1, false, true);
					_selectedSchedules[1].DeleteMainShift();
					_selectedSchedules[1].DeleteDayOff();
	            }
				else if (!schedulePart1.IsScheduled() && schedulePart2.IsScheduled())
				{
					_selectedSchedules[1].Merge(schedulePart2, false, true);
					_selectedSchedules[0].DeleteMainShift();
					_selectedSchedules[0].DeleteDayOff();
				}
				else
				{
					_selectedSchedules[0].Merge(schedulePart1, false, true, ignoreAssignmentPermission);
					_selectedSchedules[1].Merge(schedulePart2, false, true, ignoreAssignmentPermission);
				}
            }
            retList.AddRange(_selectedSchedules);
            return retList;
        }

		private static void removeAbsences(IScheduleDay schedulePart)
		{
			if (schedulePart.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence)
			{
				var personAbsences = schedulePart.PersonAbsenceCollection();
				foreach (var personAbsence in personAbsences)
				{
					schedulePart.Remove(personAbsence);
				}
			}
		}

		private bool checkBasicRules()
        {
            if (_selectedSchedules.Count != 2)
                return false;

            if (_selectedSchedules[0].Person == _selectedSchedules[1].Person)
                return false;

            return true;
        }
    }
}