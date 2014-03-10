using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class Swapper
	{
		private readonly ScheduleViewBase _scheduleView;
		private readonly IUndoRedoContainer _undoRedo;
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IGridlockManager _gridlockManager;
		private readonly SchedulingScreen _schedulingScreen;
		private readonly IScheduleTag _defaultScheduleTag;

		public Swapper(ScheduleViewBase scheduleView, IUndoRedoContainer undoRedo, ISchedulerStateHolder schedulerState, IGridlockManager gridlockManager, SchedulingScreen schedulingScreen, IScheduleTag defaultScheduleTag)
		{
			_scheduleView = scheduleView;
			_undoRedo = undoRedo;
			_schedulerState = schedulerState;
			_gridlockManager = gridlockManager;
			_schedulingScreen = schedulingScreen;
			_defaultScheduleTag = defaultScheduleTag;
		}

		public void SwapRaw()
		{
			var selectedSchedules = _scheduleView.SelectedSchedulesPerEqualTwoRanges();

			if (selectedSchedules.IsEmpty())
				return;

			ISwapRawService swapRawService = new SwapRawService(PrincipalAuthorization.Instance());
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService = new SchedulePartModifyAndRollbackService(_schedulerState.SchedulingResultState,
														 new SchedulerStateScheduleDayChangedCallback(new ResourceCalculateDaysDecider(), _schedulerState), new ScheduleTagSetter(_defaultScheduleTag));

			_undoRedo.CreateBatch(Resources.UndoRedoPaste);

			try
			{
				swapRawService.Swap(schedulePartModifyAndRollbackService, selectedSchedules[0], selectedSchedules[1], _scheduleView.LockedDatesOnPerson(_gridlockManager));
			}

			catch (ValidationException ex)
			{
				schedulePartModifyAndRollbackService.Rollback();
				_undoRedo.RollbackBatch();
				_schedulingScreen.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PersonAssignmentIsNotValidDot, ex.Message), Resources.ValidationError);
				return;
			}
			catch (PermissionException ex)
			{
				schedulePartModifyAndRollbackService.Rollback();
				_undoRedo.RollbackBatch();
				_schedulingScreen.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, ex.Message), "");
				return;
			}

			_undoRedo.CommitBatch();
			_schedulingScreen.RecalculateResources();
		}

        private List<DateOnly> getLockedDates(IEnumerable<DateOnly> dates, List<IPerson> personList)
        {
            var lockedDates = new List<DateOnly>();
            foreach (var day in dates)
            {
                var dateOnly = extractDateIfLocked(day, personList[0]);
                if (dateOnly != DateOnly.MinValue && !lockedDates.Contains(dateOnly))
                    lockedDates.Add(dateOnly);
                dateOnly = extractDateIfLocked(day, personList[1]);
                if (dateOnly != DateOnly.MinValue && !lockedDates.Contains(dateOnly))
                    lockedDates.Add(dateOnly);
            }
            return lockedDates;
        }

        private DateOnly extractDateIfLocked(DateOnly dateOnly, IPerson person)
        {
            var scheduleDay = _schedulerState.Schedules[person].ScheduledDay(dateOnly);

            var lockDictionary = _gridlockManager.Gridlocks(scheduleDay);
            if (lockDictionary != null && lockDictionary.Count != 0)
                return scheduleDay.DateOnlyAsPeriod.DateOnly;
            return DateOnly.MinValue;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.IFormatProvider,System.String,System.Object[])"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void SwapSelectedSchedules(IHandleBusinessRuleResponse handleBusinessRuleResponse, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder)
		{
			ISwapServiceNew swapService = new SwapServiceNew();
			//var swapAndModifyService = new SwapAndModifyService(swapService);
			var swapAndModifyServiceNew = new SwapAndModifyServiceNew(swapService, new SchedulerStateScheduleDayChangedCallback(new ResourceCalculateDaysDecider(), _schedulerState));

			IList<IScheduleDay> selectedSchedules = _scheduleView.SelectedSchedules();
			if (selectedSchedules.Count > 1)
			{
				var personList = new List<IPerson>(_scheduleView.AllSelectedPersons(selectedSchedules));
				if (personList.Count != 2)
					return;

				_undoRedo.CreateBatch(Resources.UndoRedoPaste);
				IList<DateOnly> dates = allSelectedDates(selectedSchedules);
				IEnumerable<IBusinessRuleResponse> lstBusinessRuleResponse;

				INewBusinessRuleCollection newRules = _schedulerState.SchedulingResultState.GetRulesToRun();
				try
				{
                    lstBusinessRuleResponse = swapAndModifyServiceNew.Swap(personList[0], personList[1], dates, getLockedDates(dates, personList), _schedulerState.Schedules, newRules, new ScheduleTagSetter(_defaultScheduleTag));
				}
				catch (ValidationException ex)
				{
					_undoRedo.RollbackBatch();
					_schedulingScreen.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, Resources.PersonAssignmentIsNotValidDot, ex.Message), Resources.ValidationError);
					return;
				}
				catch (PermissionException ex)
				{
					_undoRedo.RollbackBatch();
					_schedulingScreen.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, ex.Message), "");
					return;
				}

				var lstBusinessRuleResponseToOverride = new List<IBusinessRuleResponse>();
				var handleBusinessRules = new HandleBusinessRules(handleBusinessRuleResponse, _schedulingScreen, overriddenBusinessRulesHolder);
				lstBusinessRuleResponseToOverride.AddRange(handleBusinessRules.Handle(lstBusinessRuleResponse, lstBusinessRuleResponseToOverride));
				if (lstBusinessRuleResponseToOverride.Any())
				{
					lstBusinessRuleResponseToOverride.ForEach(newRules.Remove);
                    lstBusinessRuleResponse = swapAndModifyServiceNew.Swap(personList[0], personList[1], dates, getLockedDates(dates,personList ), _schedulerState.Schedules, newRules, new ScheduleTagSetter(_defaultScheduleTag));
					lstBusinessRuleResponseToOverride = new List<IBusinessRuleResponse>();
					foreach (var response in lstBusinessRuleResponse)
					{
						if (!response.Overridden)
							lstBusinessRuleResponseToOverride.Add(response);
					}
				}

				//if it's more than zero now. Cancel!!!
				if (lstBusinessRuleResponseToOverride.Any())
				{
					// show a MessageBox, another not overridable rule (Mandatory) might have been found later in the SheduleRange
					// will probably not happen
					_schedulingScreen.ShowErrorMessage(lstBusinessRuleResponse.First().Message, Resources.ViolationOfABusinessRule);
					_undoRedo.RollbackBatch();
					return;
				}
				_undoRedo.CommitBatch();
				_schedulingScreen.RecalculateResources();
			}
		}

		private static IList<DateOnly> allSelectedDates(IEnumerable<IScheduleDay> selectedSchedules)
		{
			ICollection<DateOnly> ret = new HashSet<DateOnly>();
			foreach (IScheduleDay part in selectedSchedules)
			{
				DateOnly dateOnly = part.DateOnlyAsPeriod.DateOnly;
				ret.Add(dateOnly);
			}
			return ret.ToList();
		}
	}
}
