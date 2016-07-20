using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	[EnabledBy(Toggles.RTA_RuleMappingOptimization_39812)]
	public class MappingReadModelUpdater :
		IHandleEvent<TenantMinuteTickEvent>,
		IHandleEvent<ActivityChangedEvent>,
		IHandleEvent<RtaStateGroupChangedEvent>,
		IRunOnHangfire
	{
		private readonly IActivityRepository _activities;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly IRtaMapRepository _mappings;
		private readonly IMappingReadModelPersister _persister;

		public MappingReadModelUpdater(IActivityRepository activities, IRtaStateGroupRepository stateGroups, IRtaMapRepository mappings, IMappingReadModelPersister persister)
		{
			_activities = activities;
			_stateGroups = stateGroups;
			_mappings = mappings;
			_persister = persister;
		}

		public void Handle(ActivityChangedEvent @event)
		{
		}

		public void Handle(RtaStateGroupChangedEvent @event)
		{
		}

		public void Handle(TenantMinuteTickEvent @event)
		{
			var existingMappings = _mappings.LoadAll();
			var activitiesIncludingNull = _activities.LoadAll().Concat(new[] {(IActivity) null});
			var stateGroupsIncludingNull = _stateGroups.LoadAll().Concat(new[] {(IRtaStateGroup) null});

			activitiesIncludingNull
				.ForEach(activity => stateGroupsIncludingNull
					.ForEach(stateGroup =>
					{
						var stateCodes = stateGroup?.StateCollection ?? new List<IRtaState>();
						stateCodes.Add(null);
						stateCodes.ForEach(stateCode =>
						{
							var mapping = existingMappings.SingleOrDefault(m =>
							{
								return activity != null &&
									   stateCode != null &&
									   m.Activity.Id == activity.Id &&
									   m.StateGroup.StateCollection.Any(s => s.StateCode == stateCode.StateCode);
							});
							_persister.Add(new Mapping
							{
								ActivityId = activity?.Id,
								StateCode = stateCode?.StateCode,
								RuleId = mapping?.RtaRule.Id ?? Guid.Empty,
								RuleName = mapping?.RtaRule.Description.Name,
								StaffingEffect = mapping?.RtaRule.StaffingEffect,
								Adherence = mapping?.RtaRule.Adherence
							});
						});
					}));
		}
	}
}