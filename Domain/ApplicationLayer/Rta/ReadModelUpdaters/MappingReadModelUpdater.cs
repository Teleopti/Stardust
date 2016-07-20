using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[EnabledBy(Toggles.RTA_RuleMappingOptimization_39812)]
	public class MappingReadModelUpdater :
		IHandleEvent<TenantMinuteTickEvent>,
		IHandleEvent<ActivityChangedEvent>,
		IHandleEvent<RtaStateGroupChangedEvent>,
		IHandleEvent<RtaMapChangedEvent>,
		IRunOnHangfire
	{
		private readonly IActivityRepository _activities;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly IRtaMapRepository _mappings;
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IMappingReadModelPersister _persister;

		public MappingReadModelUpdater(
			IActivityRepository activities, 
			IRtaStateGroupRepository stateGroups, 
			IRtaMapRepository mappings, 
			IBusinessUnitRepository businessUnits, 
			IMappingReadModelPersister persister)
		{
			_activities = activities;
			_stateGroups = stateGroups;
			_mappings = mappings;
			_businessUnits = businessUnits;
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(ActivityChangedEvent @event)
		{
			_persister.Invalidate();
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(RtaStateGroupChangedEvent @event)
		{
			_persister.Invalidate();
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(RtaMapChangedEvent @event)
		{
			_persister.Invalidate();
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			if (!_persister.Invalid())
				return;

			var maps = _mappings.LoadAll();

			var activities = _activities.LoadAll()
				.Select(a => a.Id)
				.Concat(new[] {(Guid?) null});

			var stateGroups = (
				from g in _stateGroups.LoadAllCompleteGraph()
				select new
				{
					Id = g.Id.Value,
					BusinessUnitId = g.BusinessUnit.Id.Value,
					g.Name,
					States = g.StateCollection.AsEnumerable()
				})
				.Concat(
					from b in _businessUnits.LoadAll()
					select new
					{
						Id = Guid.Empty,
						BusinessUnitId = b.Id.Value,
						Name = null as string,
						States = Enumerable.Empty<IRtaState>()
					});

			var stateCodes =
				from g in stateGroups
				let codes = g == null || g.States.IsEmpty()
					? new IRtaState[] {null}
					: g.States
				from s in codes
				select new
				{
					s?.StateCode,
					StateGroupId = g?.Id ?? Guid.Empty,
					StateGroupName = g?.Name,
					PlatformTypeId = s?.PlatformTypeId ?? Guid.Empty,
					BusinessUnitId = g?.BusinessUnitId ?? Guid.Empty
				};

			var mappings =
				from a in activities
				from s in stateCodes
				let mapping = maps.SingleOrDefault(m =>
				{
					var activitiesMatch = a == m.Activity?.Id;
					var statesMatch = m.StateGroup?.StateCollection.Any(x => x.StateCode == s.StateCode) ??
									  s.StateCode == null;

					return activitiesMatch && statesMatch;
				})
				select new Mapping
				{
					BusinessUnitId = (mapping as RtaMap)?.BusinessUnit.Id ?? s.BusinessUnitId,
					ActivityId = a,
					StateCode = s.StateCode,
					StateGroupId = s.StateGroupId,
					StateGroupName = s.StateGroupName,
					RuleId = mapping?.RtaRule.Id ?? Guid.Empty,
					RuleName = mapping?.RtaRule.Description.Name,
					StaffingEffect = mapping?.RtaRule.StaffingEffect,
					Adherence = mapping?.RtaRule.Adherence,
					DisplayColor = mapping?.RtaRule.DisplayColor.ToArgb() ?? 0,
					IsAlarm = mapping?.RtaRule.IsAlarm ?? false,
					ThresholdTime = (long) (mapping?.RtaRule.ThresholdTime.TotalSeconds ?? 0),
					AlarmColor = mapping?.RtaRule.AlarmColor.ToArgb() ?? 0,
					PlatformTypeId = s.PlatformTypeId
				};

			_persister.Persist(mappings);
		}

	}
}