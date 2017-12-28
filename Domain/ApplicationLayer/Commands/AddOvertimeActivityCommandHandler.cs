using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	[EnabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step3_44331)]
	public class AddOvertimeActivityCommandHandler : IHandleCommand<AddOvertimeActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetForId;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public AddOvertimeActivityCommandHandler(IProxyForId<IActivity> activityForId, ICurrentScenario currentScenario, IProxyForId<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetForId, IScheduleStorage scheduleStorage,
			IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_activityForId = activityForId;
			_currentScenario = currentScenario;
			_multiplicatorDefinitionSetForId = multiplicatorDefinitionSetForId;
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(AddOvertimeActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = command.Person;
			var scenario = _currentScenario.Current();
			var multiplicatorDefinitionSet = _multiplicatorDefinitionSetForId.Load(command.MultiplicatorDefinitionSetId);

			command.ErrorMessages = new List<string>();
			var dateTime = new DateTime(command.Date.Year, command.Date.Month, command.Date.Day, 0, 0, 0, DateTimeKind.Utc);
			var loadedPeriod = new DateTimePeriod(dateTime, dateTime);

			var dic = _scheduleStorage.FindSchedulesForPersons(scenario, new[] { person }, new ScheduleDictionaryLoadOptions(false, false), loadedPeriod, new[] { person }, false);
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			if (!isActivityPeriodValid(command, scheduleDay))
			{
				command.ErrorMessages.Add(Resources.InvalidInput);
				return;
			}
			scheduleDay.CreateAndAddOvertime(activity, command.Period, multiplicatorDefinitionSet, false, command.TrackedCommandInfo);
			dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}

		private static bool isActivityPeriodValid(AddOvertimeActivityCommand command, IScheduleDay scheduleDay)
		{
			var agentTimezone = command.Person.PermissionInformation.DefaultTimeZone();
			var activityStartInAgentTimezone = command.Period.StartDateTimeLocal(agentTimezone);
			var activityEndInAgentTimezone = command.Period.EndDateTimeLocal(agentTimezone);
			var ass = scheduleDay.PersonAssignment();
			var scheduleDayPeriod = ass?.Period ?? scheduleDay.Period;
			var scheduleStartDateInAgentTimezone = scheduleDayPeriod.StartDateTimeLocal(agentTimezone);
			var scheduleEndDateInAgentTimezone = scheduleDayPeriod.EndDateTimeLocal(agentTimezone);

			if (activityStartInAgentTimezone.Date == scheduleStartDateInAgentTimezone.Date)
				return true;

			if (activityStartInAgentTimezone <= scheduleEndDateInAgentTimezone
				&& activityEndInAgentTimezone >= scheduleStartDateInAgentTimezone
				&& activityStartInAgentTimezone.Date >= scheduleStartDateInAgentTimezone.Date)
				return true;
			
			return false;
		}
	}

	[DisabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step3_44331)]
	public class AddOvertimeActivityCommandHandlerNoDeltas : IHandleCommand<AddOvertimeActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetForId;

		public AddOvertimeActivityCommandHandlerNoDeltas(IProxyForId<IActivity> activityForId, IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetForId)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_multiplicatorDefinitionSetForId = multiplicatorDefinitionSetForId;
		}


		public void Handle(AddOvertimeActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = command.Person;
			var scenario = _currentScenario.Current();
			var multiplicatorDefinitionSet = _multiplicatorDefinitionSetForId.Load(command.MultiplicatorDefinitionSetId);

			command.ErrorMessages = new List<string>();

			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			if (personAssignment == null)
			{
				personAssignment = new PersonAssignment(person, scenario, command.Date);
				_personAssignmentRepository.Add(personAssignment);
			}

			personAssignment.AddOvertimeActivity(activity, command.Period, multiplicatorDefinitionSet, false, command.TrackedCommandInfo);
		}
	}
}
