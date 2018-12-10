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
	public class FixNotOverwriteLayerCommandHandler : IHandleCommand<FixNotOverwriteLayerCommand>
	{
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly INonoverwritableLayerMovingHelper _movingHelper;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;

		public FixNotOverwriteLayerCommandHandler(INonoverwritableLayerMovingHelper movingHelper, IProxyForId<IPerson> personForId, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IScheduleDifferenceSaver scheduleDifferenceSaver)
		{
			_movingHelper = movingHelper;
			_personForId = personForId;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
		}

		public void Handle(FixNotOverwriteLayerCommand command)
		{
			var person = _personForId.Load(command.PersonId);
			var dict = _scheduleStorage.FindSchedulesForPersons(_currentScenario.Current(), new[] { person },
				new ScheduleDictionaryLoadOptions(false, false),
				command.Date.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()), new[] { person }, false);
			var rule = new NotOverwriteLayerRule();
			var scheduleRange = dict[person];
			var scheduledDay = scheduleRange.ScheduledDay(command.Date);
			var personAss = scheduledDay.PersonAssignment();
			var overlappingLayers = rule.GetOverlappingLayerses(scheduledDay);

			if (overlappingLayers.Count == 0)
			{
				command.ErrorMessages = new List<string> { Resources.NoNonOverwritableActivities };
				return;
			}

			var overlappedGroups = overlappingLayers.GroupBy(l => new layerRepresentation
			{
				Id = l.LayerBelowId,
				Period = l.LayerBelowPeriod
			}, l => l.LayerAbovePeriod).OrderBy(g => g.Key.Period.StartDateTime);
			var isFixed = true;
			foreach (var g in overlappedGroups)
			{
				var avoidPeriod = new DateTimePeriod(g.Min(x => x.StartDateTime), g.Max(x => x.EndDateTime));
				var movingDistance = _movingHelper.GetMovingDistance(scheduledDay, avoidPeriod, g.Key.Id);

				if (movingDistance != TimeSpan.Zero)
				{
					var targetLayer = personAss.ShiftLayers.First(l => l.Id.HasValue && l.Id.Value.Equals(g.Key.Id));
					personAss.MoveActivityAndKeepOriginalPriority(targetLayer,
						targetLayer.Period.StartDateTime.Add(movingDistance), command.TrackedCommandInfo, false);
					dict.Modify(scheduledDay, NewBusinessRuleCollection.Minimum());

				}
				else
				{
					isFixed = false;
				}
			}
			var diff = scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>());
			_scheduleDifferenceSaver.SaveChanges(diff, (ScheduleRange)scheduleRange);
			command.ErrorMessages = !isFixed ? new List<string> { Resources.OverlappedNonoverwritableActivitiesExist } : new List<string>();
		}
	}

	struct layerRepresentation
	{
		public Guid Id { get; set; }
		public DateTimePeriod Period { get; set; }
	}
}
