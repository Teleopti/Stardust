using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.OutboundSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class OutboundThresholdSettingsPersistorAndProvider : ISettingsPersisterAndProvider<OutboundThresholdSettings>
	{
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private const string outboundThresholdSettingKey = "OutboundThresholdSettings";

		public OutboundThresholdSettingsPersistorAndProvider(IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_personalSettingDataRepository = personalSettingDataRepository;
		}
		
		public OutboundThresholdSettings Persist(OutboundThresholdSettings outboundThresholdSettings)
		{
			var setting = _personalSettingDataRepository.FindValueByKey(outboundThresholdSettingKey, new OutboundThresholdSettings());
			setting.RelativeWarningThreshold = outboundThresholdSettings.RelativeWarningThreshold;
			_personalSettingDataRepository.PersistSettingValue(setting);
			return setting;
		}

		public OutboundThresholdSettings Get()
		{
			return _personalSettingDataRepository.FindValueByKey(outboundThresholdSettingKey, new OutboundThresholdSettings());
		}

		public OutboundThresholdSettings GetByOwner(IPerson person)
		{
			return _personalSettingDataRepository.FindValueByKeyAndOwnerPerson(outboundThresholdSettingKey, person, new OutboundThresholdSettings());
		}
	}
}