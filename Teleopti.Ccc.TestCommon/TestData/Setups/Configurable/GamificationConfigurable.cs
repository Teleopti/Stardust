using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class GamificationConfigurable: IDataSetup
	{
		public string Name { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var repository = GamificationSettingRepository.DONT_USE_CTOR(currentUnitOfWork);
			GamificationSetting setting=new GamificationSetting(Name);
			repository.Add(setting);
		}
	}
}
