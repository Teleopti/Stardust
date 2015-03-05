using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAlarmMappingLoader
	{
		IEnumerable<AlarmMapping> Load();
	}
}