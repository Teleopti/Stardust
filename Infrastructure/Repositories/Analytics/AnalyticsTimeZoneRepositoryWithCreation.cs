using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsTimeZoneRepositoryWithCreation : IAnalyticsTimeZoneRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;
		private readonly IEventPublisher _eventPublisher;
		private readonly IAnalyticsConfigurationRepository _analyticsConfigurationRepository;

		public AnalyticsTimeZoneRepositoryWithCreation(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork, IEventPublisher eventPublisher, IAnalyticsConfigurationRepository analyticsConfigurationRepository)
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
			_eventPublisher = eventPublisher;
			_analyticsConfigurationRepository = analyticsConfigurationRepository;
		}
		
		public void SetUtcInUse(bool isUtcInUse)
		{
			var query = $@"SELECT CONVERT(BIT, count(1)) FROM mart.dim_time_zone WITH (NOLOCK) WHERE time_zone_code = 'UTC' AND utc_in_use =:{nameof(isUtcInUse)}";
			var dontUpdate = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetBoolean(nameof(isUtcInUse), isUtcInUse)
				.UniqueResult<bool>();

			if (dontUpdate)
				return;

			query = $@"UPDATE mart.dim_time_zone SET utc_in_use=:{nameof(isUtcInUse)} WHERE time_zone_code = 'UTC'";
			_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetBoolean(nameof(isUtcInUse), isUtcInUse)
				.ExecuteUpdate();
		}

		public void SetToBeDeleted(string timeZoneCode, bool tobeDeleted)
		{
			var query = $@"SELECT CONVERT(BIT, count(1)) FROM mart.dim_time_zone WITH (NOLOCK) WHERE time_zone_code=:{nameof(timeZoneCode)} AND to_be_deleted=:{nameof(tobeDeleted)}";
			var dontUpdate = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetString(nameof(timeZoneCode), timeZoneCode)
				.SetBoolean(nameof(tobeDeleted), tobeDeleted)
				.UniqueResult<bool>();

			if (dontUpdate)
				return;

			query = $@"UPDATE mart.dim_time_zone SET to_be_deleted=:{nameof(tobeDeleted)} WHERE time_zone_code=:{nameof(timeZoneCode)}";
			_currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(query)
				.SetString(nameof(timeZoneCode), timeZoneCode)
				.SetBoolean(nameof(tobeDeleted), tobeDeleted)
				.ExecuteUpdate();
		}

		public IList<AnalyticsTimeZone> GetAll()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                time_zone_id {nameof(AnalyticsTimeZone.TimeZoneId)},
					time_zone_code {nameof(AnalyticsTimeZone.TimeZoneCode)},
					utc_in_use {nameof(AnalyticsTimeZone.IsUtcInUse)},
					to_be_deleted {nameof(AnalyticsTimeZone.ToBeDeleted)}
				from mart.dim_time_zone WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.List<AnalyticsTimeZone>();
		}

		public IList<AnalyticsTimeZone> GetAllUsedByLogDataSourcesAndBaseConfig()
		{
			return _currentAnalyticsUnitOfWork.Current()
				.Session()
				.CreateSQLQuery(@"exec mart.sys_get_timezones_used_by_datasource_and_base_config")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.List<AnalyticsTimeZone>();
		}

		public AnalyticsTimeZone Get(string timeZoneCode)
		{
			return timeZoneInDb(timeZoneCode) ?? createTimeZone(timeZoneCode);
		}

		private AnalyticsTimeZone timeZoneInDb(string timeZoneCode)
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
					$@"select 
	                time_zone_id {nameof(AnalyticsTimeZone.TimeZoneId)},
					time_zone_code {nameof(AnalyticsTimeZone.TimeZoneCode)},
					utc_in_use {nameof(AnalyticsTimeZone.IsUtcInUse)},
					to_be_deleted {nameof(AnalyticsTimeZone.ToBeDeleted)}
				from mart.dim_time_zone WITH (NOLOCK) where time_zone_code=:{nameof(timeZoneCode)}")
				.SetString(nameof(timeZoneCode), timeZoneCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsTimeZone)))
				.UniqueResult<AnalyticsTimeZone>();
		}

		private AnalyticsTimeZone createTimeZone(string timeZoneCode)
		{
			create(timeZoneCode, _analyticsConfigurationRepository.GetTimeZone());

			_currentAnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				_eventPublisher.Publish(new AnalyticsTimeZoneChangedEvent());
			});

			return timeZoneInDb(timeZoneCode);
		}

		private void create(string timeZoneCode, TimeZoneInfo defaultTimeZone)
		{
			var timeZoneDim = new TimeZoneDim(TimeZoneInfo.FindSystemTimeZoneById(timeZoneCode), timeZoneCode == defaultTimeZone.Id, false);
			var query = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery($@"exec mart.[etl_dim_time_zone_insert]
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