using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IDeleteSchedulePartService
	{
		IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, DeleteOption options,
			ISchedulePartModifyAndRollbackService rollbackService, ISchedulingProgress backgroundWorker);

		IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService);

		IEnumerable<IScheduleDay> Delete(IEnumerable<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService, INewBusinessRuleCollection businessRuleCollection);
	}

	public class DeleteSchedulePartService : IDeleteSchedulePartService
	{
		public IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, DeleteOption options,
			ISchedulePartModifyAndRollbackService rollbackService, ISchedulingProgress backgroundWorker)
		{
			IList<IScheduleDay> returnList = new List<IScheduleDay>();
			if (backgroundWorker == null)
				throw new ArgumentNullException(nameof(backgroundWorker));

			foreach (IScheduleDay part in list)
			{
				var clonePart = preparePart(options, part);
				IList<IScheduleDay> cloneList = new List<IScheduleDay> {clonePart};

				if (backgroundWorker.CancellationPending)
					return returnList;

				//modify the original schedules
				foreach (IScheduleDay scheduleDay in cloneList)
				{
					rollbackService.Modify(scheduleDay);
				}

				returnList.Add(part.ReFetch());
			}

			return returnList;
		}

		public IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, DeleteOption options,
			ISchedulePartModifyAndRollbackService rollbackService, ISchedulingProgress backgroundWorker,
			INewBusinessRuleCollection newBusinessRuleCollection)
		{
			if (backgroundWorker == null)
				throw new ArgumentNullException(nameof(backgroundWorker));

			return list.Select(part =>
			{
				if (backgroundWorker.CancellationPending)
					return null;

				var clonePart = preparePart(options, part);

				rollbackService.Modify(clonePart, newBusinessRuleCollection);




				return part.ReFetch();
			}).Where(r => r != null).ToList();
		}

		public IList<IScheduleDay> Delete(IList<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService, DeleteOption deleteOption)
		{
			var bgWorker = new NoSchedulingProgress();
			IList<IScheduleDay> retList = Delete(list, deleteOption, rollbackService, bgWorker);

			return retList;
		}

		public IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			var deleteOption = new DeleteOption {Default = true};
			var bgWorker = new NoSchedulingProgress();
			IList<IScheduleDay> retList = Delete(list, deleteOption, rollbackService, bgWorker);

			return retList;
		}

		public IEnumerable<IScheduleDay> Delete(IEnumerable<IScheduleDay> list,
			ISchedulePartModifyAndRollbackService rollbackService, INewBusinessRuleCollection businessRuleCollection)
		{
			var deleteOption = new DeleteOption {Default = true};
			var bgWorker = new NoSchedulingProgress();
			IList<IScheduleDay> retList = Delete(list, deleteOption, rollbackService, bgWorker, businessRuleCollection);

			return retList;
		}

		private IScheduleDay preparePart(DeleteOption options, IScheduleDay part)
		{
			IScheduleDay clonePart = part.ReFetch();


			if (options.MainShift)
				clonePart.DeleteMainShift();
			if (options.MainShiftSpecial)
				clonePart.DeleteMainShiftSpecial();
			if (options.PersonalShift)
				clonePart.DeletePersonalStuff();
			if (options.DayOff)
				clonePart.DeleteDayOff();
			if (options.Absence)
			{

				clonePart.DeleteFullDayAbsence(clonePart);
			}

			if (options.Overtime)
				clonePart.DeleteOvertime();

			if (options.Preference)
				clonePart.DeletePreferenceRestriction();

			if (options.StudentAvailability)
				clonePart.DeleteStudentAvailabilityRestriction();
			if (options.OvertimeAvailability)
				clonePart.DeleteOvertimeAvailability();

			if (options.Default)
			{
				SchedulePartView view = clonePart.SignificantPartForDisplay();

				if (view == SchedulePartView.MainShift)
				{
					clonePart.DeleteMainShift();
					return clonePart;
				}

				if (view == SchedulePartView.PersonalShift)
				{
					clonePart.DeletePersonalStuff();
					return clonePart;
				}

				if (view == SchedulePartView.FullDayAbsence || view == SchedulePartView.ContractDayOff)
				{

					clonePart.DeleteFullDayAbsence(clonePart);
					return clonePart;
				}

				if (view == SchedulePartView.DayOff ||
					view == SchedulePartView.ContractDayOff && view != SchedulePartView.FullDayAbsence)
				{
					clonePart.DeleteDayOff();
					return clonePart;
				}

				if (view == SchedulePartView.Absence)
				{
					clonePart.DeleteAbsence(false);
					return clonePart;
				}

				if (view == SchedulePartView.Overtime)
					clonePart.DeleteOvertime();
			}

			return clonePart;
		}
	}
}