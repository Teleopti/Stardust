using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	[EnabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step3_44331)]
	public class AddOvertimeActivityCommandHandler : IHandleCommand<AddOvertimeActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IProxyForId<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetForId;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public AddOvertimeActivityCommandHandler(IProxyForId<IActivity> activityForId, ICurrentScenario currentScenario, 
			IProxyForId<IPerson> personForId, IProxyForId<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetForId, IScheduleStorage scheduleStorage, 
			IScheduleDifferenceSaver scheduleDifferenceSaver )
		{
			_activityForId = activityForId;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_multiplicatorDefinitionSetForId = multiplicatorDefinitionSetForId;
			_scheduleStorage = scheduleStorage;
			this._scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(AddOvertimeActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var multiplicatorDefinitionSet = _multiplicatorDefinitionSetForId.Load(command.MultiplicatorDefinitionSetId);

			command.ErrorMessages = new List<string>();

			var dic = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(command.Period), scenario, new PersonProvider(new []{person}), new ScheduleDictionaryLoadOptions(false, false), new[] {person});
				//(new[]{person}, new ScheduleDictionaryLoadOptions(false, false), command.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), scenario);
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			scheduleDay.CreateAndAddOvertime(activity,command.Period,multiplicatorDefinitionSet,false);
			dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange) scheduleRange);
		}
	}

	[DisabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step3_44331)]
	public class AddOvertimeActivityCommandHandlerNoDeltas:IHandleCommand<AddOvertimeActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IProxyForId<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetForId;

		public AddOvertimeActivityCommandHandlerNoDeltas(IProxyForId<IActivity> activityForId, IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IPerson> personForId, IProxyForId<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetForId)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_multiplicatorDefinitionSetForId = multiplicatorDefinitionSetForId;
		}


		public void Handle(AddOvertimeActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = _personForId.Load(command.PersonId);			
			var scenario = _currentScenario.Current();
			var multiplicatorDefinitionSet = _multiplicatorDefinitionSetForId.Load(command.MultiplicatorDefinitionSetId);

			command.ErrorMessages = new List<string>();

			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			if(personAssignment == null)
			{
				personAssignment = new PersonAssignment(person,scenario,command.Date);
				_personAssignmentRepository.Add(personAssignment);
			}

			personAssignment.AddOvertimeActivity(activity,command.Period,multiplicatorDefinitionSet, false, command.TrackedCommandInfo);
		}
	}
}
