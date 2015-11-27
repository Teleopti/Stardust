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
		private readonly IStateGroupActivityAlarmRepository _stateGroupActivityAlarmRepository;
		private readonly IAppliedAdherence _appliedAdherence;

		public AlarmMappingLoader(
			IStateGroupActivityAlarmRepository stateGroupActivityAlarmRepository,
			IAppliedAdherence appliedAdherence
			)
		{
			_stateGroupActivityAlarmRepository = stateGroupActivityAlarmRepository;
			_appliedAdherence = appliedAdherence;
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual IEnumerable<AlarmMapping> Load()
		{
			var mappings = _stateGroupActivityAlarmRepository.LoadAll();
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
				let alarmType = m.AlarmType != null && !alarmIsSoftDeleted(m.AlarmType) ? m.AlarmType : new AlarmType()
				let alarmTypeId = alarmType.Id.HasValue ? alarmType.Id.Value : Guid.Empty
				select new AlarmMapping
				{
					BusinessUnitId = businessUnitId,
					PlatformTypeId = platformTypeId,
					StateCode = statecode,
					ActivityId = activityId,
					AlarmTypeId = alarmTypeId,
					AlarmName = alarmType.Description.Name,
					Adherence = _appliedAdherence.ForAlarm(m.AlarmType),
					StaffingEffect = (int) alarmType.StaffingEffect,
					DisplayColor = alarmType.DisplayColor.ToArgb(),
					ThresholdTime = alarmType.ThresholdTime.Ticks
				}
				).ToArray();
		}

		private static bool alarmIsSoftDeleted(IAlarmType alarmType)
		{
			return alarmType != null && ((IDeleteTag)alarmType).IsDeleted;
		}

		private static Guid tryGetBusinessUnitId(IStateGroupActivityAlarm m)
		{
			if (m.StateGroup != null) return m.StateGroup.BusinessUnit.Id.Value;
			if (m.AlarmType != null) return m.AlarmType.BusinessUnit.Id.Value;
			return Guid.Empty;
		}
	}

}