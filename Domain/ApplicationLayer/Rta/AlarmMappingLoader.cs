using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
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
				from c in m.StateGroup.StateCollection
				let activityId = m.Activity != null ? m.Activity.Id.Value : (Guid?) null
				select new AlarmMapping
				{
					BusinessUnitId = m.StateGroup.BusinessUnit.Id.Value,
					StateCode = c.StateCode,
					ActivityId = activityId,
					AlarmTypeId = m.AlarmType.Id.Value,
					AlarmName = m.AlarmType.Description.Name,
					Adherence = AdherenceInfo.ConvertAdherence(_appliedAdherence.ForAlarm(m.AlarmType)),
					StaffingEffect = (int) m.AlarmType.StaffingEffect,
					DisplayColor = m.AlarmType.DisplayColor.ToArgb(),
					ThresholdTime = m.AlarmType.ThresholdTime.Ticks
				}
				).ToArray();
		}
	}

}