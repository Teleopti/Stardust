using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AlarmMappingLoader : IAlarmMappingLoader
	{
		private readonly IRtaMapRepository _rtaMapRepository;
		private readonly IAppliedAdherence _appliedAdherence;

		public AlarmMappingLoader(
			IRtaMapRepository rtaMapRepository,
			IAppliedAdherence appliedAdherence
			)
		{
			_rtaMapRepository = rtaMapRepository;
			_appliedAdherence = appliedAdherence;
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual IEnumerable<AlarmMapping> Load()
		{
			var mappings = _rtaMapRepository.LoadAll();
			return (
				from m in mappings
				let businessUnitId = tryGetBusinessUnitId(m)
				let stateCodes = m.StateGroup == null
					? new IRtaState[] {null}
					: from s in m.StateGroup.StateCollection select s
				from c in stateCodes
				let statecode = c != null ? c.StateCode : null
				let platformTypeId = c != null ? c.PlatformTypeId : Guid.Empty
				let activityId = m.Activity != null ? m.Activity.Id.Value : (Guid?) null
				let rule = m.RtaRule ?? new RtaRule()
				let ruleId = rule.Id.HasValue ? rule.Id.Value : Guid.Empty
				select new AlarmMapping
				{
					BusinessUnitId = businessUnitId,
					PlatformTypeId = platformTypeId,
					StateCode = statecode,
					ActivityId = activityId,
					RuleId = ruleId,
					IsAlarm = rule.IsAlarm,
					AlarmName = rule.Description.Name,
					Adherence = _appliedAdherence.ForAlarm(m.RtaRule),
					StaffingEffect = (int) rule.StaffingEffect,
					DisplayColor = rule.DisplayColor.ToArgb(),
					ThresholdTime = rule.ThresholdTime.Ticks
				}
				).ToArray();
		}
		
		private static Guid tryGetBusinessUnitId(IRtaMap m)
		{
			if (m.StateGroup != null) return m.StateGroup.BusinessUnit.Id.Value;
			if (m.RtaRule != null) return m.RtaRule.BusinessUnit.Id.Value;
			return Guid.Empty;
		}
	}

}