using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class FixNotOverwriteLayerCommandHandler : IHandleCommand<FixNotOverwriteLayerCommand>
	{
		private IScheduleDayProvider _scheduleDayProvider;
		private readonly IProxyForId<IPerson> _personForId;
		private INonoverwritableLayerMovingHelper _movingHelper;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment,PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;

		public FixNotOverwriteLayerCommandHandler(IScheduleDayProvider scheduleDayProvider, IProxyForId<IPerson> personForId, INonoverwritableLayerMovingHelper nonoverwritableLayerMovingHelper, IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario)
		{
			_scheduleDayProvider = scheduleDayProvider;
			_personForId = personForId;
			_movingHelper = nonoverwritableLayerMovingHelper;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
		}


		public void Handle(FixNotOverwriteLayerCommand command)
		{
			var scenario = _currentScenario.Current();
			var person = _personForId.Load(command.PersonId);
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
			{
				Date = command.Date,
				Scenario = scenario,
				Person = person
			});


			var dict = _scheduleDayProvider.GetScheduleDictionary(command.Date, person);
			var rule = new NotOverwriteLayerRule();
			var overlappingLayers = rule.GetOverlappingLayerses(dict, dict[person].ScheduledDay(command.Date));

			if (overlappingLayers.Count == 0) return;

			var overlappedGroups = overlappingLayers.GroupBy(l => new layerRepresentation
			{
				Id = l.LayerBelowId,
				Period = l.LayerBelowPeriod
			}, l => l.LayerAbovePeriod).OrderBy(g => g.Key.Period.StartDateTime);
			var isFixed = true;
			foreach (var g in overlappedGroups)
			{
				var avoidPeriod = new DateTimePeriod(g.Min(x => x.StartDateTime), g.Max(x => x.EndDateTime));
				var movingDistance = _movingHelper.GetMovingDistance(person, command.Date, avoidPeriod, g.Key.Id);

				if (movingDistance != TimeSpan.Zero)
				{
					var targetLayer = personAssignment.ShiftLayers.First(l => l.Id.HasValue && l.Id.Value.Equals(g.Key.Id));
					personAssignment.MoveActivityAndKeepOriginalPriority(targetLayer,
						targetLayer.Period.StartDateTime.Add(movingDistance), null, true);
				}
				else
				{
					isFixed = false;
				}
			}
			command.ErrorMessages = !isFixed ? new List<string> {Resources.OverlappedNonoverwritableActivitiesExist} : new List<string>();
		}
	}

	struct layerRepresentation
	{
		public Guid Id { get; set; }
		public DateTimePeriod Period { get; set; }
	}
}
