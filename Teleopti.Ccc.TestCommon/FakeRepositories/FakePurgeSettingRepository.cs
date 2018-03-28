using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePurgeSettingRepository : IPurgeSettingRepository
	{
		private readonly ICollection<PurgeSetting> datas = new List<PurgeSetting>()
		{
			new PurgeSetting
			{
				Id =  0,
				Key = "DaysToKeepExternalPerformanceData",
				Value = 60
			}
		};

		public IEnumerable<PurgeSetting> FindAllPurgeSettings()
		{
			return datas.ToList();
		}
	}
}
