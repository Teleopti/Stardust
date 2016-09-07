using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IPersonAssignmentAddActivity _addActivity;
		private readonly INonoverwritableLayerMovabilityChecker _movabilityChecker;
		private readonly INonoverwritableLayerMovingHelper _movingHelper;


		public AddActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, 
			ICurrentScenario currentScenario, 
			IProxyForId<IActivity> activityForId, 
			IProxyForId<IPerson> personForId, 
			IUserTimeZone timeZone, 
			IShiftCategoryRepository shiftCategoryRepository, 
			IPersonAssignmentAddActivity addActivity,
			INonoverwritableLayerMovabilityChecker movabilityChecker,
			INonoverwritableLayerMovingHelper movingHelper)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_timeZone = timeZone;
			_shiftCategoryRepository = shiftCategoryRepository;
			_addActivity = addActivity;
			_movabilityChecker = movabilityChecker;
			_movingHelper = movingHelper;
		}

		public void Handle(AddActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = _personForId.Load(command.PersonId);
			var timeZone = _timeZone.TimeZone();
			var scenario = _currentScenario.Current();
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			command.ErrorMessages = new List<string>();
			
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, timeZone), TimeZoneHelper.ConvertToUtc(command.EndTime, timeZone));

			var personAssignmentOfPreviousDay = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date.AddDays(-1),
				Scenario = scenario,
				Person = person
			});

			if (personAssignmentOfPreviousDay != null && personAssignmentOfPreviousDay.Period.EndDateTime >= period.StartDateTime)
			{
				command.ErrorMessages.Add(Resources.ActivityConflictsWithOvernightShiftsFromPreviousDay);
				return;
			}

			if (personAssignment == null)
			{
				var newPersonAssignment = new PersonAssignment(person, scenario, command.Date);
				_addActivity.AddActivity(newPersonAssignment, activity, period, command.TrackedCommandInfo);
				var shiftCategories = _shiftCategoryRepository.FindAll().ToList();
				shiftCategories.Sort(new ShiftCategorySorter());
				var shiftCategory = shiftCategories.FirstOrDefault();
				if (shiftCategory != null)
				{
					newPersonAssignment.SetShiftCategory(shiftCategory);
					_personAssignmentRepository.Add(newPersonAssignment);
				}
			}
			else if (!personAssignment.ShiftLayers.Any())
			{
				_addActivity.AddActivity(personAssignment,activity, period, command.TrackedCommandInfo);

				var shiftCategories = _shiftCategoryRepository.FindAll().ToList();
				shiftCategories.Sort(new ShiftCategorySorter());
				var shiftCategory = shiftCategories.FirstOrDefault();
				if (shiftCategory != null)
					personAssignment.SetShiftCategory(shiftCategory);
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
								fixableLayer.Period.StartDateTime.Add(movingDistance), null);
						}
					}
					else
					{
						warnings.Add(Resources.NewActivityOverlapsNonoverwritableActivities);
					}
					command.WarningMessages = warnings;
				}
				_addActivity.AddActivity(personAssignment, activity, period, command.TrackedCommandInfo);
				if (personAssignment.ShiftCategory == null)
				{
					var shiftCategories = _shiftCategoryRepository.FindAll().ToList();
					shiftCategories.Sort(new ShiftCategorySorter());
					var shiftCategory = shiftCategories.FirstOrDefault();
					if (shiftCategory != null)
						personAssignment.SetShiftCategory(shiftCategory);
				}
			}
		}
	}

	[EnabledBy(Toggles.AddActivity_TriggerResourceCalculation_39346)]
	public class AddActivityWithResourceCalculation : IPersonAssignmentAddActivity
	{
		public void AddActivity(IPersonAssignment personAssignment, IActivity activity, DateTimePeriod period,
			TrackedCommandInfo trackedCommandInfo)
		{
			personAssignment.AddActivity(activity, period, trackedCommandInfo,true);
		}
	}

	[DisabledBy(Toggles.AddActivity_TriggerResourceCalculation_39346)]
	public class AddActivityWithoutResourceCalculation : IPersonAssignmentAddActivity
	{
		public void AddActivity(IPersonAssignment personAssignment, IActivity activity, DateTimePeriod period,
			TrackedCommandInfo trackedCommandInfo)
		{
			personAssignment.AddActivity(activity, period, trackedCommandInfo);
		}
	}

	public interface IPersonAssignmentAddActivity
	{
		void AddActivity(IPersonAssignment personAssignment, IActivity activity, DateTimePeriod period, TrackedCommandInfo trackedCommandInfo);
	}
}