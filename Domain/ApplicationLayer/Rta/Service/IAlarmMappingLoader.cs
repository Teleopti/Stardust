using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAlarmMappingLoader
	{
		IEnumerable<RuleMapping> Load();
	}
}