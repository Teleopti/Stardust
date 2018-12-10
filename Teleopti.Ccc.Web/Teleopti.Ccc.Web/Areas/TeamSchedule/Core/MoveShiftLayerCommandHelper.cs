using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class MoveShiftLayerCommandHelper : IMoveShiftLayerCommandHelper
	{
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepositoryTypedId;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPermissionProvider _permissionProvider;

		public MoveShiftLayerCommandHelper(IWriteSideRepositoryTypedId<IPersonAssignment,
			PersonAssignmentKey> personAssignmentRepositoryTypedId,
			ICurrentScenario currentScenario,
			IPermissionProvider permissionProvider)
		{
			_personAssignmentRepositoryTypedId = personAssignmentRepositoryTypedId;
			_currentScenario = currentScenario;
			_permissionProvider = permissionProvider;
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
				var layerToMoveTime = layerToMoveTimeMap[l.Id.Value];
				if (layerToMoveTime < earliestStart)
					earliestStart = layerToMoveTime;
				if (layerToMoveTime.Add(l.Period.ElapsedTime()) > latestEnd)
					latestEnd = layerToMoveTime.Add(l.Period.ElapsedTime());
			});
			return latestEnd.Subtract(earliestStart).Duration() <= new TimeSpan(36, 0, 0);
		}

		public bool CheckPermission(IList<Guid> layerIds, IPerson person, DateOnly date, out List<string> messages)
		{
			messages = new List<string>();
			if (agentScheduleIsWriteProtected(date, person))
			{
				messages.Add(Resources.WriteProtectSchedule);
				return false;
			}
			var currentScenario = _currentScenario.Current();
			var hasMoveActivityPermission =
				_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.MoveActivity, date,
					person);
			var hasMoveOvertimePermission =
				_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.MoveOvertime, date,
					person);

			var personAssignment = _personAssignmentRepositoryTypedId.LoadAggregate(new PersonAssignmentKey
			{
				Date = date,
				Person = person,
				Scenario = currentScenario
			});

			foreach (var layerId in layerIds)
			{
				var layer = personAssignment.ShiftLayers.SingleOrDefault(sl => sl.Id == layerId);
				if (layer == null)
				{
					messages.Add(Resources.NoShiftsFound);
					continue;
				}
				var isOvertime = layer is OvertimeShiftLayer;
				if (isOvertime)
				{
					if (!hasMoveOvertimePermission)
						messages.Add(Resources.NoPermissionMoveAgentOvertime);
				}
				else if (!hasMoveActivityPermission)
				{
					messages.Add(Resources.NoPermissionMoveAgentActivity);
				}
			}

			return !messages.Any();
		}

		private bool agentScheduleIsWriteProtected(DateOnly date, IPerson agent)
		{
			return !_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)
					&& agent.PersonWriteProtection.IsWriteProtected(date);
		}
	}



	public interface IMoveShiftLayerCommandHelper
	{
		IDictionary<Guid, DateTime> GetCorrectNewStartForLayersForPerson(IPerson person, DateOnly scheduleDate,
			IEnumerable<Guid> shiftLayerIds, DateTime newStartTimeUtc);

		bool ValidateLayerMoveToTime(IDictionary<Guid, DateTime> layerToMoveTimeMap, IPerson person, DateOnly date);
		bool CheckPermission(IList<Guid> layerIds, IPerson person, DateOnly date, out List<string> messages);
	}
}