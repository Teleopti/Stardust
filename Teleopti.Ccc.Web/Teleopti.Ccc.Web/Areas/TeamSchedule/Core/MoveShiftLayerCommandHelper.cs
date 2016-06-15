using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class MoveShiftLayerCommandHelper : IMoveShiftLayerCommandHelper
	{
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepositoryTypedId;
		private readonly ICurrentScenario _currentScenario;

		public MoveShiftLayerCommandHelper(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepositoryTypedId, ICurrentScenario currentScenario)
		{
			_personAssignmentRepositoryTypedId = personAssignmentRepositoryTypedId;
			_currentScenario = currentScenario;
		}

		public IDictionary<Guid, DateTime> GetCorrectNewStartForLayersForPerson(IPerson person, DateOnly scheduleDate, IEnumerable<Guid> shiftLayerIds, DateTime newStartTimeUtc)
		{
			var currentScenario = _currentScenario.Current();
			var layerToTimeMap = new Dictionary<Guid, DateTime>();
			var personAssignment = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = scheduleDate,
				Person = person,
				Scenario = currentScenario
			});
			if (personAssignment == null)
			{
				return layerToTimeMap;
			}
			var layersToMove = shiftLayerIds.Select(x =>
			{
				return personAssignment.ShiftLayers.SingleOrDefault(l => l.Id.Equals(x));
			});
			var earlistStart = DateTime.MaxValue;
			layersToMove.ForEach(l =>
			{
				if (l.Period.StartDateTime < earlistStart)
					earlistStart = l.Period.StartDateTime;
			});
			layersToMove.ForEach(l =>
			{
				layerToTimeMap.Add(l.Id.Value, l.Period.StartDateTime.Add(newStartTimeUtc.Subtract(earlistStart)));
			});


			return layerToTimeMap;
		}

		public bool ValidateLayerMoveToTime(IDictionary<Guid, DateTime> layerToMoveTimeMap, IPerson person, DateOnly scheduleDate)
		{
			var currentScenario = _currentScenario.Current();
			var personAssignment = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = scheduleDate,
				Person = person,
				Scenario = currentScenario
			});

			if (personAssignment == null)
			{
				return false;
			}

			var layersExcludeSelected = personAssignment.ShiftLayers.Where(l => !layerToMoveTimeMap.ContainsKey(l.Id.Value));
			var selectedLayers = personAssignment.ShiftLayers.Where(l => layerToMoveTimeMap.ContainsKey(l.Id.Value));

			var earliestStart = DateTime.MaxValue;
			var latestEnd = DateTime.MinValue;

			layersExcludeSelected.ForEach(l =>
			{
				if (l.Period.StartDateTime < earliestStart)
					earliestStart = l.Period.StartDateTime;
				if (l.Period.EndDateTime > latestEnd)
					latestEnd = l.Period.EndDateTime;
			});

			selectedLayers.ForEach(l =>
			{
				if (layerToMoveTimeMap[l.Id.Value] < earliestStart)
					earliestStart = layerToMoveTimeMap[l.Id.Value];
				if (layerToMoveTimeMap[l.Id.Value].Add(l.Period.ElapsedTime()) > latestEnd)
					latestEnd = layerToMoveTimeMap[l.Id.Value].Add(l.Period.ElapsedTime());
			});
			return latestEnd.Subtract(earliestStart).Duration() <= new TimeSpan(36, 0, 0);
		}
	}

	public interface IMoveShiftLayerCommandHelper
	{
		IDictionary<Guid, DateTime> GetCorrectNewStartForLayersForPerson(IPerson person, DateOnly scheduleDate,
			IEnumerable<Guid> shiftLayerIds, DateTime newStartTimeUtc);

		bool ValidateLayerMoveToTime(IDictionary<Guid, DateTime> layerToMoveTimeMap, IPerson person, DateOnly date);
	}
}