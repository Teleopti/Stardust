using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsTimeZoneRepositoryWithCreation : AnalyticsTimeZoneRepositoryBase, IAnalyticsTimeZoneRepository
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IAnalyticsConfigurationRepository _analyticsConfigurationRepository;

		public AnalyticsTimeZoneRepositoryWithCreation(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork, IEventPublisher eventPublisher, IAnalyticsConfigurationRepository analyticsConfigurationRepository) : base(analyticsUnitOfWork)
		{
			_eventPublisher = eventPublisher;
			_analyticsConfigurationRepository = analyticsConfigurationRepository;
		}

		public new AnalyticsTimeZone Get(string timeZoneCode)
		{
			return base.Get(timeZoneCode) ?? createTimeZone(timeZoneCode);
		}

		private AnalyticsTimeZone createTimeZone(string timeZoneCode)
		{
			create(timeZoneCode, _analyticsConfigurationRepository.GetTimeZone());

			AnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				_eventPublisher.Publish(new AnalyticsTimeZoneChangedEvent());
			});

			return base.Get(timeZoneCode);
		}

		private void create(string timeZoneCode, TimeZoneInfo defaultTimeZone)
		{
			var timeZoneDim = new TimeZoneDim(TimeZoneInfo.FindSystemTimeZoneById(timeZoneCode), timeZoneCode == defaultTimeZone.Id, false);
			var query = AnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"exec mart.[etl_dim_time_zone_insert]
						@time_zone_code=:{nameof(TimeZoneDim.TimeZoneCode)}, 
						@time_zone_name=:{nameof(TimeZoneDim.TimeZoneName)}, 
						@default_zone=:{nameof(TimeZoneDim.IsDefaultTimeZone)}, 
						@utc_conversion=:{nameof(TimeZoneDim.UtcConversion)}, 
						@utc_conversion_dst=:{nameof(TimeZoneDim.UtcConversionDst)}
					  ")
				.SetString(nameof(TimeZoneDim.TimeZoneCode), timeZoneDim.TimeZoneCode)
				.SetString(nameof(TimeZoneDim.TimeZoneName), timeZoneDim.TimeZoneName)
				.SetBoolean(nameof(TimeZoneDim.IsDefaultTimeZone), timeZoneDim.IsDefaultTimeZone)
				.SetInt32(nameof(TimeZoneDim.UtcConversion), timeZoneDim.UtcConversion)
				.SetInt32(nameof(TimeZoneDim.UtcConversionDst), timeZoneDim.UtcConversionDst);
			query.ExecuteUpdate();
		}
	}
}