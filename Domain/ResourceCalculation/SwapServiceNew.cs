using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
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


		public IList<IScheduleDay> Swap(IScheduleDictionary schedules)
		{
			if (schedules == null)
				throw new ArgumentNullException("schedules");

			if (!CanSwapAssignments())
				throw new ConstraintException("Can not swap assignments");

			var retList = new List<IScheduleDay>();

			var schedulePart0 = schedules[_selectedSchedules[0].Person].ReFetch(_selectedSchedules[0]);
			var schedulePart1 = schedules[_selectedSchedules[1].Person].ReFetch(_selectedSchedules[1]);

			swapScheduleDays(schedulePart0, schedulePart1);

			retList.AddRange(new List<IScheduleDay> { schedulePart0, schedulePart1 });
			return retList;
		}

		private void swapScheduleDays(IScheduleDay source, IScheduleDay destination)
		{

			IScheduleDay tempSlot = (IScheduleDay)source.Clone();
			tempSlot.Clear<IPersistableScheduleData>();

			copyAndClean(source, tempSlot);
			copyAndClean(destination, source);
			copyAndClean(tempSlot, destination);

		}

		private void copyAndClean(IScheduleDay source, IScheduleDay destination)
		{
			IScheduleDay tempPart = (IScheduleDay)source.Clone();
			tempPart.Clear<IPersonDayOff>();
			tempPart.Clear<IPersonAbsence>();
			tempPart.Clear<IPreferenceDay>();
			tempPart.Clear<IStudentAvailabilityDay>();
			destination.MergeSwap(tempPart, false);

			tempPart = (IScheduleDay)source.Clone();
			tempPart.Clear<IPersonAbsence>();
			tempPart.Clear<IPersonAssignment>();
			tempPart.Clear<IPreferenceDay>();
			tempPart.Clear<IStudentAvailabilityDay>();
			destination.Merge(tempPart, false);

			((ExtractedSchedule)destination).MergeOvertime(source);

			source.DeleteDayOff();
			source.DeleteOvertime();
			source.DeleteMainShift(source);
		}
	}
}
