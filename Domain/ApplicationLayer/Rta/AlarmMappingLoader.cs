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

		public AlarmMappingLoader(IStateGroupActivityAlarmRepository stateGroupActivityAlarmRepository)
		{
			_stateGroupActivityAlarmRepository = stateGroupActivityAlarmRepository;
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
					StaffingEffect = (int) m.AlarmType.StaffingEffect,
					DisplayColor = m.AlarmType.DisplayColor.ToArgb(),
					ThresholdTime = m.AlarmType.ThresholdTime.Ticks
				}
				).ToArray();
		}
	}
}