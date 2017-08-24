using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{

	[EnabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class RemoveActivityCommandHandler : IHandleCommand<RemoveActivityCommand>
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public RemoveActivityCommandHandler( ICurrentScenario currentScenario, IProxyForId<IPerson> personForId, IScheduleStorage scheduleStorage, IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_currentScenario = currentScenario;
			_personForId = personForId;
			_scheduleStorage = scheduleStorage;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(RemoveActivityCommand command)
		{

			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var period = new DateTimePeriod(command.Date.Date.Utc(), command.Date.Date.Utc());
			var dic = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(period), scenario, new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false), new[] { person });
			var scheduleRange = dic[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);
			var personAssignment = scheduleDay.PersonAssignment();
			
			command.ErrorMessages = new List<string>();

			if (personAssignment == null)
			{
				command.ErrorMessages.Add(Resources.PersonAssignmentIsNotValid);
				return;
			}

			var shiftLayer = personAssignment.ShiftLayers.FirstOrDefault(layer => layer.Id == command.ShiftLayerId);

			if (shiftLayer == null)
			{
				command.ErrorMessages.Add(Resources.NoShiftsFound);
				return;
			}

			var mainShiftLayer = shiftLayer as MainShiftLayer;

			var minOrderIndex = personAssignment.ShiftLayers.Min(layer =>
			{
				var layerAsMain = layer as MainShiftLayer;
				return layerAsMain?.OrderIndex ?? int.MaxValue;
			});

			if (mainShiftLayer != null && mainShiftLayer.OrderIndex == minOrderIndex)
			{
				command.ErrorMessages.Add(Resources.CannotDeleteBaseActivity);
				return;
			}
			scheduleDay.RemoveActivity(shiftLayer, false, command.TrackedCommandInfo);
			dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
			_scheduleDifferenceSaver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
		}
	}

	[DisabledBy(Toggles.Staffing_ReadModel_BetterAccuracy_Step4_43389)]
	public class RemoveActivityCommandHandlerWithoutDeltas:IHandleCommand<RemoveActivityCommand>
	{	
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;

		public RemoveActivityCommandHandlerWithoutDeltas(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IPerson> personForId)
		{
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
		}

		public void Handle(RemoveActivityCommand command)
		{
			
			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});

			command.ErrorMessages = new List<string>();

			if (personAssignment == null)
			{
				command.ErrorMessages.Add(Resources.PersonAssignmentIsNotValid);
				return;
			}
			
			var shiftLayer = personAssignment.ShiftLayers.FirstOrDefault(layer => layer.Id == command.ShiftLayerId);

			if (shiftLayer == null)
			{
				command.ErrorMessages.Add(Resources.NoShiftsFound);
				return;
			}

			var mainShiftLayer = shiftLayer as MainShiftLayer;

			var minOrderIndex = personAssignment.ShiftLayers.Min(layer =>
			{
				var layerAsMain = layer as MainShiftLayer;
				return layerAsMain?.OrderIndex ?? int.MaxValue;
			});

			if (mainShiftLayer != null && mainShiftLayer.OrderIndex == minOrderIndex)
			{
				command.ErrorMessages.Add(Resources.CannotDeleteBaseActivity);
				return;
			}

			personAssignment.RemoveActivity(shiftLayer, false, command.TrackedCommandInfo);		
		}
	}
}
