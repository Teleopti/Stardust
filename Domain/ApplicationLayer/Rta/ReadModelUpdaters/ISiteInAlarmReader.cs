using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface ISiteInAlarmReader
	{
		IEnumerable<SiteInAlarmModel> Read();
	}
}