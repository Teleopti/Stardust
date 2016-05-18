using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReader : IMappingReader
	{
		private readonly IRtaMapRepository _rtaMapRepository;
		private readonly AppliedAdherence _appliedAdherence;
		private readonly IRtaStateGroupRepository _stateGroupRepository;

		public FakeMappingReader(
			IRtaMapRepository rtaMapRepository,
			AppliedAdherence appliedAdherence,
			IRtaStateGroupRepository stateGroupRepository)
		{
			_rtaMapRepository = rtaMapRepository;
			_appliedAdherence = appliedAdherence;
			_stateGroupRepository = stateGroupRepository;
		}

		public IEnumerable<Mapping> Read()
		{
			var stateGroups = _stateGroupRepository.LoadAllCompleteGraph();
			var mappings = _rtaMapRepository.LoadAllCompleteGraph();

			var allMappings = from m in mappings
				let businessUnitId = tryGetBusinessUnitId(m)
				let activityId = m.Activity == null ? (Guid?) null : m.Activity.Id.Value
				let @group = m.StateGroup == null
					? new
					{
						GroupId = Guid.Empty,
						GroupName = null as string,
						StateCodes = new[]
						{
							new
							{
								StateCode = null as string,
								PlatformTypeId = Guid.Empty
							}
						}
					}
					: new
					{
						GroupId = m.StateGroup.Id.Value,
						GroupName = m.StateGroup.Name,
						StateCodes = m.StateGroup.StateCollection.IsEmpty()
							? new[]
							{
								new
								{
									StateCode = null as string,
									PlatformTypeId = Guid.Empty
								}
							}
							: (from s in
								m.StateGroup.StateCollection
								select new
								{
									StateCode = s.StateCode,
									PlatformTypeId = s.PlatformTypeId
								}).ToArray()
					}
				from c in @group.StateCodes
				let rule = m.RtaRule == null
					? new
					{
						RuleId = Guid.Empty,
						RuleName = null as string,
						StaffingEffect = 0,
						DisplayColor = 0,
						IsAlarm = false,
						ThresholdTime = 0L,
						AlarmColor = 0,
					}
					: new
					{
						RuleId = m.RtaRule.Id.Value,
						RuleName = m.RtaRule.Description.Name,
						StaffingEffect = (int) m.RtaRule.StaffingEffect,
						DisplayColor = m.RtaRule.DisplayColor.ToArgb(),
						IsAlarm = m.RtaRule.IsAlarm,
						ThresholdTime = m.RtaRule.ThresholdTime.Ticks,
						AlarmColor = m.RtaRule.AlarmColor.ToArgb(),
					}
				select new Mapping
				{
					BusinessUnitId = businessUnitId,
					PlatformTypeId = c.PlatformTypeId,
					StateCode = c.StateCode,
					ActivityId = activityId,
					RuleId = rule.RuleId,
					RuleName = rule.RuleName,
					Adherence = _appliedAdherence.ForRule(m.RtaRule),
					StaffingEffect = rule.StaffingEffect,
					DisplayColor = rule.DisplayColor,
					IsAlarm = rule.IsAlarm,
					ThresholdTime = rule.ThresholdTime,
					AlarmColor = rule.AlarmColor,
					StateGroupId = @group.GroupId,
					StateGroupName = @group.GroupName
				};

			var groupsWithoutMapping = (
				from g in stateGroups
				let assignedToMapping = (from m in mappings where m.StateGroup == g select m).Any()
				where !assignedToMapping
				from c in g.StateCollection
				select new Mapping
				{
					BusinessUnitId = g.BusinessUnit.Id.Value,
					PlatformTypeId = c.PlatformTypeId,
					StateCode = c.StateCode,
					StateGroupId = g.Id.Value,
					StateGroupName = g.Name
				});

			return allMappings
				.Concat(groupsWithoutMapping)
				.ToArray();
		}

		private static Guid tryGetBusinessUnitId(IRtaMap m)
		{
			if (m.StateGroup != null) return m.StateGroup.BusinessUnit.Id.Value;
			if (m.RtaRule != null) return m.RtaRule.BusinessUnit.Id.Value;
			return Guid.Empty;
		}

		public IEnumerable<Mapping> ReadFor(IEnumerable<string> stateCodes, IEnumerable<Guid?> activities)
		{
			// mimics dbo.LoadRtaMappingsFor.sql .. maybe
			// if this is complex, just return all, but to mimics gives a better test suite
			var mappings = Read();
			var combinations = (
				from m in mappings
				where
					(m.StateCode == null || stateCodes.Contains(m.StateCode)) &&
					(m.ActivityId == null || activities.Contains(m.ActivityId))
				select m
				).ToArray();
			var unmappedStates = (
				from c in stateCodes
				let m = mappings.FirstOrDefault(x => x.StateCode == c)
				where m != null
				select m
				).ToArray();
			return combinations
				.Union(unmappedStates);
		}
	}
}
