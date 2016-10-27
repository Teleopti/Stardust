using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.ApplicationLayer.GlobalSettingData
{
	[EnabledBy(Toggles.ETL_EventBasedAgentNameDescription_41432)]
	public class AnalyticsPersonNameUpdater : 
		IHandleEvent<CommonNameDescriptionChangedEvent>,
		IRunOnHangfire
	{
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;

		public AnalyticsPersonNameUpdater(IGlobalSettingDataRepository globalSettingDataRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository)
		{
			_globalSettingDataRepository = globalSettingDataRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(CommonNameDescriptionChangedEvent @event)
		{
			var commonNameDescription = _globalSettingDataRepository.FindValueByKey<CommonNameDescriptionSetting>(CommonNameDescriptionSetting.Key, null);
			_analyticsPersonPeriodRepository.UpdatePersonNames(commonNameDescription, @event.LogOnBusinessUnitId);
		}
	}
}
