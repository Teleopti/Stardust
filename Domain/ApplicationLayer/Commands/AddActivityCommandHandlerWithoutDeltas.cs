using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	//[DisabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class AddActivityCommandHandlerWithoutDeltas : IHandleCommand<AddActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly INonoverwritableLayerMovabilityChecker _movabilityChecker;
		private readonly INonoverwritableLayerMovingHelper _movingHelper;


		public AddActivityCommandHandlerWithoutDeltas(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, 
			ICurrentScenario currentScenario, 
			IProxyForId<IActivity> activityForId, 
			IProxyForId<IPerson> personForId, 
			IUserTimeZone timeZone, 
			IShiftCategoryRepository shiftCategoryRepository,
			INonoverwritableLayerMovabilityChecker movabilityChecker,
			INonoverwritableLayerMovingHelper movingHelper)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_timeZone = timeZone;
			_shiftCategoryRepository = shiftCategoryRepository;
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
				newPersonAssignment.AddActivity(activity, period, command.TrackedCommandInfo);
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
				personAssignment.AddActivity(activity, period, command.TrackedCommandInfo);

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
					var shiftCategories = _shiftCategoryRepository.FindAll().ToList();
					shiftCategories.Sort(new ShiftCategorySorter());
					var shiftCategory = shiftCategories.FirstOrDefault();
					if (shiftCategory != null)
						personAssignment.SetShiftCategory(shiftCategory);
				}
			}
		}
	}
}