using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class AlertTimeSettingConfigurable : IDataSetup
	{
		public int AlertTime { get; set; }
		public AlertTimeSettingConfigurable(int alertTime)
		{
			AlertTime = alertTime;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var globalSettingRep = GlobalSettingDataRepository.DONT_USE_CTOR(currentUnitOfWork);

			var alertTimeSetting = globalSettingRep.FindValueByKey("AsmAlertTime", new AsmAlertTime());
			alertTimeSetting.SecondsBeforeChange = AlertTime;

			globalSettingRep.PersistSettingValue(alertTimeSetting);
		}
	}
}
