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
	}

	public interface IMoveShiftLayerCommandHelper
	{
		IDictionary<Guid, DateTime> GetCorrectNewStartForLayersForPerson(IPerson person, DateOnly scheduleDate,
			IEnumerable<Guid> shiftLayerIds, DateTime newStartTimeUtc);
	}
}