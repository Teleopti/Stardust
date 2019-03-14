using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Events;
using Teleopti.Wfm.Adherence.States.Events;
using Teleopti.Wfm.Adherence.States.Infrastructure;

namespace Teleopti.Wfm.Adherence.States
{
	public class MappingReadModelUpdater :
		IHandleEvent<UnknownStateCodeReceviedEvent>,
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
		private readonly IDistributedLockAcquirer _distributedLock;

		public MappingReadModelUpdater(
			IActivityRepository activities, 
			IRtaStateGroupRepository stateGroups, 
			IRtaMapRepository mappings, 
			IBusinessUnitRepository businessUnits, 
			IMappingReadModelPersister persister,
			IDistributedLockAcquirer distributedLock)
		{
			_activities = activities;
			_stateGroups = stateGroups;
			_mappings = mappings;
			_businessUnits = businessUnits;
			_persister = persister;
			_distributedLock = distributedLock;
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual void Handle(UnknownStateCodeReceviedEvent @event)
		{
			var stateGroups = _stateGroups.LoadAll().ToLookup(g => g.BusinessUnit);

			var existingStateGroup = stateGroups[@event.BusinessUnitId].SingleOrDefault(g => g.StateCollection.Any(s => s.StateCode == @event.StateCode));
			if (existingStateGroup != null)
				return;

			var defaultStateGroup = stateGroups[@event.BusinessUnitId].SingleOrDefault(g => g.DefaultStateGroup);
			if (defaultStateGroup == null)
				return;

			var stateDescription = @event.StateDescription ?? @event.StateCode;

			var hasStateCode = defaultStateGroup
				.StateCollection
				.Any(x => x.StateCode == @event.StateCode);
			if (!hasStateCode)
				defaultStateGroup.AddState(@event.StateCode, stateDescription);
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
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				if (!_persister.Invalid())
					return;

				var mappings = MakeMappings(_businessUnits, _activities, _stateGroups, _mappings);

				_persister.Persist(mappings);
			});
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
					g.Id,
					BusinessUnitId = g.BusinessUnit.Value,
					g.Name,
					IsLoggedOut = g.IsLogOutState,
					States = g.StateCollection.AsEnumerable()
				})
				.Concat(
					from b in businessUnits
					select new
					{
						Id = null as Guid?,
						BusinessUnitId = b.Id.Value,
						Name = null as string,
						IsLoggedOut = true,
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
						c.StateCode
					}
				let codes = legalStateCodes.IsEmpty()
					? legalStateCodes.Append(
						new
						{
							StateCode = null as string
						})
					: legalStateCodes
				from c in codes
				select new
				{
					c.StateCode,
					StateGroupId = g.Id,
					StateGroupName = g.Name,
					IsLoggedOut = g.IsLoggedOut,
					BusinessUnitId = g.BusinessUnitId
				};

			var configuredMappings =
				(from m in maps.Cast<RtaMap>()
				let legalStateCodes =
					from c in (m.StateGroup?.StateCollection).EmptyIfNull()
					let legal = c.StateCode != null
					where legal
					select new
					{
						c.StateCode
					}
				let codes = m.StateGroup == null
					? legalStateCodes.Append(
						new
						{
							StateCode = null as string
						})
					: legalStateCodes
				from c in codes
				select new
				{
					Key = new Tuple<Guid,Guid?,String>(m.BusinessUnit.Value,m.Activity,c.StateCode),
					Rule = m.RtaRule
				}).ToLookup(m => m.Key);

			var mappings =
				from a in activities
				from s in stateCodes
				let mapping = configuredMappings[new Tuple<Guid, Guid?, string>(s.BusinessUnitId,a,s.StateCode)].SingleOrDefault()
				let businessUnitId = mapping?.Key.Item1 ?? s.BusinessUnitId
				let ruleId = mapping?.Rule?.Id
				let ruleName = mapping?.Rule?.Description.Name
				let staffingEffect = mapping?.Rule?.StaffingEffect
				let adherence = mapping?.Rule?.Adherence
				let displayColor = mapping?.Rule?.DisplayColor.ToArgb() ?? 0
				let isAlarm = mapping?.Rule?.IsAlarm ?? false
				let thresholdTime = mapping?.Rule?.ThresholdTime ?? 0
				let alarmColor = mapping?.Rule?.AlarmColor.ToArgb() ?? 0
				select new Mapping
				{
					BusinessUnitId = businessUnitId,
					ActivityId = a,
					StateCode = s.StateCode,
					StateGroupId = s.StateGroupId,
					StateGroupName = s.StateGroupName,
					IsLoggedOut = s.IsLoggedOut,
					RuleId = ruleId,
					RuleName = ruleName,
					StaffingEffect = staffingEffect,
					Adherence = adherence,
					DisplayColor = displayColor,
					IsAlarm = isAlarm,
					ThresholdTime = thresholdTime,
					AlarmColor = alarmColor
				};
			return mappings;
		}

	}
}