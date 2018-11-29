using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IUserTimeZone _timeZone;
		private readonly IShiftCategorySelector _shiftCategorySelector;
		private readonly INonoverwritableLayerMovabilityChecker _movabilityChecker;
		private readonly INonoverwritableLayerMovingHelper _movingHelper;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;


		public AddActivityCommandHandler(ICurrentScenario currentScenario,
			IProxyForId<IActivity> activityForId,
			IUserTimeZone timeZone,
			IShiftCategorySelector shiftCategorySelector,
			INonoverwritableLayerMovabilityChecker movabilityChecker,
			INonoverwritableLayerMovingHelper movingHelper, IScheduleStorage scheduleStorage, 
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_activityForId = activityForId;
			_currentScenario = currentScenario;
			_timeZone = timeZone;
			_shiftCategorySelector = shiftCategorySelector;
			_movabilityChecker = movabilityChecker;
			_movingHelper = movingHelper;
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(AddActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = command.Person;
			var timeZone = _timeZone.TimeZone();
			var scenario = _currentScenario.Current();
			var periodForPerson = command.Date.ToDateOnlyPeriod().Inflate(1).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var dic = _scheduleStorage.FindSchedulesForPersons(scenario, new[] { person }, new ScheduleDictionaryLoadOptions(false, false), periodForPerson, new[] { person }, false);
			var scheduleRange = dic[person];
			
			command.ErrorMessages = new List<string>();

			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, timeZone), TimeZoneHelper.ConvertToUtc(command.EndTime, timeZone));
			var previousScheduleDay = scheduleRange.ScheduledDay(command.Date.AddDays(-1));
			var personAssignmentOfPreviousDay = previousScheduleDay.PersonAssignment();
			
			if (personAssignmentOfPreviousDay != null && !personAssignmentOfPreviousDay.ShiftLayers.IsEmpty() && personAssignmentOfPreviousDay.Period.EndDateTime >= period.StartDateTime)
			{
				command.ErrorMessages.Add(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
				return;
			}
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment(true);
			if (!personAssignment.ShiftLayers.Any())
			{
				personAssignment.AddActivity(activity, period, command.TrackedCommandInfo);

				var shiftCategory = _shiftCategorySelector.Get(person, command.Date, period);
				if (shiftCategory != null)
					personAssignment.SetShiftCategory(shiftCategory);
				dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
				_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
			}
			else
			{
				if (command.MoveConflictLayerAllowed && _movabilityChecker.HasNonoverwritableLayer(person, command.Date, period, activity))
				{
					var warnings = new List<string>();
					if (_movabilityChecker.IsFixableByMovingNonoverwritableLayer(period, person, command.Date))
					{
						var fixableLayer = _movabilityChecker.GetNonoverwritableLayersToMove(person, command.Date, period).Single();
						var movingDistance = _movingHelper.GetMovingDistance(person, command.Date, period,
							fixableLayer.Id.GetValueOrDefault());
						if (movingDistance == TimeSpan.Zero)
						{
							warnings.Add(Resources.NewActivityOverlapsNonoverwritableActivities);
						}
						else
						{
							personAssignment.MoveActivityAndKeepOriginalPriority(fixableLayer,
								fixableLayer.Period.StartDateTime.Add(movingDistance), null, true);
						}
					}
					else
					{
						warnings.Add(Resources.NewActivityOverlapsNonoverwritableActivities);
					}
					command.WarningMessages = warnings;
				}
				personAssignment.AddActivity(activity, period, command.TrackedCommandInfo);
				if (personAssignment.ShiftCategory == null)
				{
					var shiftCategory = _shiftCategorySelector.Get(person, command.Date, period);
					if (shiftCategory != null)
						personAssignment.SetShiftCategory(shiftCategory);
				}
				dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
				_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
			}
		}
	}
}