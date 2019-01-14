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
		IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, DeleteOption options, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingProgress backgroundWorker);
		IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService);
		IEnumerable<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, INewBusinessRuleCollection businessRuleCollection);
	}

	public class DeleteSchedulePartService : IDeleteSchedulePartService
	{
		public IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, DeleteOption options, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingProgress backgroundWorker)
		{
			IList<IScheduleDay> returnList = new List<IScheduleDay>();
			if (backgroundWorker == null)
				throw new ArgumentNullException(nameof(backgroundWorker));
 
			foreach (var part in list)
			{
				var cloneParts = preparePart(options, part);

				if (backgroundWorker.CancellationPending)
					return returnList;

				foreach (var scheduleDay in cloneParts)
				{
					rollbackService.Modify(scheduleDay);
				}

				returnList.Add(part.ReFetch());
			}

			return returnList;
		}

		public IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, DeleteOption options, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingProgress backgroundWorker, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			if (backgroundWorker == null)
				throw new ArgumentNullException(nameof(backgroundWorker));

			return list.Select(part =>
			{
				if (backgroundWorker.CancellationPending)
					return null;

				var cloneParts = preparePart(options, part);

				foreach (var scheduleDay in cloneParts)
				{
					rollbackService.Modify(scheduleDay, newBusinessRuleCollection);
				}

				return part.ReFetch();
			}).Where(r => r != null).ToList();
		}

		public IList<IScheduleDay> Delete(IList<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, DeleteOption deleteOption)
		{
			var bgWorker = new NoSchedulingProgress();
			IList<IScheduleDay> retList = Delete(list, deleteOption, rollbackService, bgWorker);

			return retList;
		}

		public IList<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService)
		{
			var deleteOption = new DeleteOption {Default = true};
			var bgWorker = new NoSchedulingProgress();
			IList<IScheduleDay> retList = Delete(list, deleteOption, rollbackService, bgWorker);

			return retList;
		}

		public IEnumerable<IScheduleDay> Delete(IEnumerable<IScheduleDay> list, ISchedulePartModifyAndRollbackService rollbackService, INewBusinessRuleCollection businessRuleCollection)
		{
			var deleteOption = new DeleteOption { Default = true };
			var bgWorker = new NoSchedulingProgress();
			IList<IScheduleDay> retList = Delete(list, deleteOption, rollbackService, bgWorker, businessRuleCollection);

			return retList;
		}

		private IList<IScheduleDay> preparePart(DeleteOption options, IScheduleDay part)
		{
			var clonePart = part.ReFetch();
			IScheduleDay partDayBefore = null;

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
				partDayBefore = intersectingFullDayAbsenceDayBefore(part);
				clonePart.DeleteFullDayAbsence(clonePart);
			}

			if (options.Overtime)
				clonePart.DeleteOvertime();

			if (options.Preference)
				clonePart.DeletePreferenceRestriction();

			if (options.StudentAvailability)
				clonePart.DeleteStudentAvailabilityRestriction();
			if(options.OvertimeAvailability)
				clonePart.DeleteOvertimeAvailability();

			if (options.Default)
			{
				var view = clonePart.SignificantPartForDisplay();

				if (view == SchedulePartView.MainShift)
				{
					clonePart.DeleteMainShift();
				}
					
				else if (view == SchedulePartView.PersonalShift)
				{
					clonePart.DeletePersonalStuff();
				}

				else if (view == SchedulePartView.FullDayAbsence || view == SchedulePartView.ContractDayOff)
				{
					partDayBefore = intersectingFullDayAbsenceDayBefore(part);
					clonePart.DeleteFullDayAbsence(clonePart);
				}

				else if (view == SchedulePartView.DayOff || view == SchedulePartView.ContractDayOff  && view != SchedulePartView.FullDayAbsence)
				{
					clonePart.DeleteDayOff();
				}
					
				else if (view == SchedulePartView.Absence)
				{
					clonePart.DeleteAbsence(false);
				}
					
				else if (view == SchedulePartView.Overtime)
					clonePart.DeleteOvertime();
			}

			return partDayBefore != null ? new List<IScheduleDay> { partDayBefore, clonePart } : new List<IScheduleDay> { clonePart };
		}

		private IScheduleDay intersectingFullDayAbsenceDayBefore(IScheduleDay part)
		{
			if (!part.IsFullDayAbsence()) return null;
			var visualPeriod = part.ProjectionService().CreateProjection().Period();
			if (!visualPeriod.HasValue) return null;
			return part.PersonAbsenceCollection().Any(x => x.Period.StartDateTime < part.Period.StartDateTime && x.Period.Contains(visualPeriod.Value)) 
				? part.Owner[part.Person].ScheduledDay(part.DateOnlyAsPeriod.DateOnly.AddDays(-1)) : null;
		}
	}
}
