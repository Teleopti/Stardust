﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
			var ass1 = schedulePart1.PersonAssignment();
			var ass2 = schedulePart2.PersonAssignment();
			if ((ass1==null || ass2==null) &&
				(!schedulePart1.HasDayOff() && !schedulePart2.HasDayOff()))
			{
				if (ass1==null)
				{
					_selectedSchedules[1].Merge(schedulePart2, false);
					_selectedSchedules[1].DeletePersonalStuff();
					_selectedSchedules[0].DeleteMainShift(schedulePart2);
				}
				else
				{
					_selectedSchedules[0].Merge(schedulePart1, false);
					_selectedSchedules[0].DeletePersonalStuff();
					_selectedSchedules[1].DeleteMainShift(schedulePart1);
				}
			}
			else
			{
				if (!schedulePart1.PersistableScheduleDataCollection().Any())
					_selectedSchedules[0].Merge(_selectedSchedules[0], true);
				else
					_selectedSchedules[0].Merge(schedulePart1, false);

				if(!schedulePart2.PersistableScheduleDataCollection().Any())
					_selectedSchedules[1].Merge(_selectedSchedules[1], true);
				else
					_selectedSchedules[1].Merge(schedulePart2, false);
			}

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
