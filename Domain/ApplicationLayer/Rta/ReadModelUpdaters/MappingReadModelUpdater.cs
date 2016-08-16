using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		IHandleEvent<RtaRuleChangedEvent>,
		IHandleEvent<BusinessUnitChangedEvent>,
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
		public virtual void Handle(BusinessUnitChangedEvent @event)
		{
			_persister.Invalidate();
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

		[ReadModelUnitOfWork]
		public virtual void Handle(RtaRuleChangedEvent @event)
		{
			_persister.Invalidate();
		}

		[AllBusinessUnitsUnitOfWork]
		[ReadModelUnitOfWork]
		public virtual void Handle(TenantMinuteTickEvent @event)
		{
			if (!_persister.Invalid())
				return;

			var mappings = MakeMappings(_businessUnits, _activities, _stateGroups, _mappings);

			_persister.Persist(mappings);
		}

		public static IEnumerable<Mapping> MakeMappings(
			IBusinessUnitRepository businessUnitsRepository,
			IActivityRepository activityRepository,
			IRtaStateGroupRepository stateGroupRepository,
			IRtaMapRepository mapRepository
			)
		{
			var businessUnits = businessUnitsRepository.LoadAllWithDeleted();

			var maps = mapRepository.LoadAll();

			var activities = activityRepository.LoadAll()
				.Select(a => a.Id)
				.Concat(new[] {(Guid?) null});

			var stateGroups = (
				from g in stateGroupRepository.LoadAllCompleteGraph()
				where g.StateCollection.Any()
				select new
				{
					Id = g.Id.Value,
					BusinessUnitId = g.BusinessUnit.Id.Value,
					g.Name,
					States = g.StateCollection.AsEnumerable()
				})
				.Concat(
					from b in businessUnits
					select new
					{
						Id = Guid.Empty,
						BusinessUnitId = b.Id.Value,
						Name = null as string,
						States = Enumerable.Empty<IRtaState>()
					});

			var stateCodes =
				from g in stateGroups
				let legalStateCodes =
					from c in g.States
					let legal = c.StateCode != null
					where legal
					select new
					{
						c.StateCode,
						c.PlatformTypeId
					}
				let codes = legalStateCodes.IsEmpty()
					? legalStateCodes.Append(
						new
						{
							StateCode = null as string,
							PlatformTypeId = Guid.Empty
						})
					: legalStateCodes
				from c in codes
				select new
				{
					c.StateCode,
					StateGroupId = g.Id,
					StateGroupName = g.Name,
					PlatformTypeId = c.PlatformTypeId,
					BusinessUnitId = g.BusinessUnitId
				};

			var configuredMappings =
				from m in maps.Cast<RtaMap>()
				let legalStateCodes =
					from c in (m.StateGroup?.StateCollection).EmptyIfNull()
					let legal = c.StateCode != null
					where legal
					select new
					{
						c.StateCode,
						c.PlatformTypeId
					}
				let codes = m.StateGroup == null
					? legalStateCodes.Append(
						new
						{
							StateCode = null as string,
							PlatformTypeId = Guid.Empty
						})
					: legalStateCodes
				from c in codes
				select new
				{
					BusinessUnitId = m.BusinessUnit.Id.Value,
					ActivityId = m.Activity?.Id,
					StateCode = c.StateCode,
					PlatformTypeId = c.PlatformTypeId,
					Rule = m.RtaRule
				};

			var mappings =
				from a in activities
				from s in stateCodes
				let mapping = (
					from m in configuredMappings
					where
						m.BusinessUnitId == s.BusinessUnitId &&
						m.ActivityId == a &&
						m.StateCode == s.StateCode &&
						m.PlatformTypeId == s.PlatformTypeId
					select m
					).SingleOrDefault()
				let businessUnitId = mapping?.BusinessUnitId ?? s.BusinessUnitId
				let ruleId = mapping?.Rule?.Id ?? Guid.Empty
				let ruleName = mapping?.Rule?.Description.Name
				let staffingEffect = mapping?.Rule?.StaffingEffect
				let adherence = mapping?.Rule?.Adherence
				let displayColor = mapping?.Rule?.DisplayColor.ToArgb() ?? 0
				let isAlarm = mapping?.Rule?.IsAlarm ?? false
				let thresholdTime = mapping?.Rule?.ThresholdTime.Ticks ?? 0
				let alarmColor = mapping?.Rule?.AlarmColor.ToArgb() ?? 0
				select new Mapping
				{
					BusinessUnitId = businessUnitId,
					ActivityId = a,
					StateCode = s.StateCode,
					StateGroupId = s.StateGroupId,
					StateGroupName = s.StateGroupName,
					RuleId = ruleId,
					RuleName = ruleName,
					StaffingEffect = staffingEffect,
					Adherence = adherence,
					DisplayColor = displayColor,
					IsAlarm = isAlarm,
					ThresholdTime = thresholdTime,
					AlarmColor = alarmColor,
					PlatformTypeId = s.PlatformTypeId
				};
			return mappings;
		}
	}
}