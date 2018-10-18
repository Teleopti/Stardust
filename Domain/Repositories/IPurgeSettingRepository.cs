using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPurgeSettingRepository
	{
		IEnumerable<PurgeSetting> FindAllPurgeSettings();
	}
}
