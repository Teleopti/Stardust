using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class RequiredScheduleHelper
	{
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
		private readonly IStudentSchedulingService _studentSchedulingService;
		private readonly Func<IOptimizationPreferences> _optimizationPreferences;
		private readonly IGridlockManager _gridlockManager;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public RequiredScheduleHelper(RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak, 
				Func<ISchedulingResultStateHolder> resultStateHolder, 
				IStudentSchedulingService studentSchedulingService, 
				Func<IOptimizationPreferences> optimizationPreferences, 
				IGridlockManager gridlockManager, 
				IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
			_resultStateHolder = resultStateHolder;
			_studentSchedulingService = studentSchedulingService;
			_optimizationPreferences = optimizationPreferences;
			_gridlockManager = gridlockManager;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public void ScheduleSelectedStudents(IEnumerable<IScheduleDay> allSelectedSchedules, ISchedulingProgress backgroundWorker,
			SchedulingOptions schedulingOptions)
		{
			if (allSelectedSchedules == null) throw new ArgumentNullException(nameof(allSelectedSchedules));
			if (schedulingOptions == null) throw new ArgumentNullException(nameof(schedulingOptions));

			IList<IScheduleDay> unlockedSchedules = new List<IScheduleDay>();
			foreach (var scheduleDay in allSelectedSchedules)
			{
				GridlockDictionary locks = _gridlockManager.Gridlocks(scheduleDay);
				if (locks == null || locks.Count == 0)
					unlockedSchedules.Add(scheduleDay);
			}

			if (unlockedSchedules.Count == 0)
			{
				return;
			}

			var selectedPersons = new PersonListExtractorFromScheduleParts().ExtractPersons(unlockedSchedules);
			var selectedPeriod = unlockedSchedules.Select(s => s.DateOnlyAsPeriod.DateOnly).OrderBy(d => d.Date);
			var period = new DateOnlyPeriod(selectedPeriod.FirstOrDefault(), selectedPeriod.LastOrDefault());
			var scheduledCount = 0;
			var sendEventEvery = schedulingOptions.RefreshRate;

			_optimizationPreferences().Rescheduling.ConsiderShortBreaks = _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedPersons, period);
			
			EventHandler<SchedulingServiceBaseEventArgs> onDayScheduled = (sender, e) =>
			{
				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
				}
				if (e.IsSuccessful)
					scheduledCount++;
				if (scheduledCount >= sendEventEvery)
				{
					backgroundWorker.ReportProgress(1, e);
					scheduledCount = 0;
				}
			};
			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			tagSetter.ChangeTagToSet(schedulingOptions.TagToUseOnScheduling);
			_studentSchedulingService.DayScheduled += onDayScheduled;
			var rollbackService = new SchedulePartModifyAndRollbackService(_resultStateHolder(),
				_scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			using (PerformanceOutput.ForOperation("Scheduling " + unlockedSchedules.Count))
			{
				_studentSchedulingService.DoTheScheduling(unlockedSchedules, schedulingOptions, false, rollbackService);
			}

			_studentSchedulingService.DayScheduled -= onDayScheduled;
		}
	}
}