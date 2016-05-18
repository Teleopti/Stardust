using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDayOffRepository : IAnalyticsDayOffRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsDayOffRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void AddOrUpdate(AnalyticsDayOff analyticsDayOff)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_day_off_insert_or_update]
						@day_off_code=:DayOffCode, 
						@day_off_name=:DayOffName, 
						@display_color=:DisplayColor,
						@business_unit_id=:BusinessUnitId,
						@datasource_id=:DatasourceId,
						@datasource_update_date=:DatasourceUpdateDate,
						@display_color_html=:DisplayColorHtml,
						@day_off_shortname=:DayOffShortName
					  ")
				.SetGuid("DayOffCode", analyticsDayOff.DayOffCode.GetValueOrDefault())
				.SetString("DayOffName", analyticsDayOff.DayOffName)
				.SetInt32("DisplayColor", analyticsDayOff.DisplayColor)
				.SetInt32("BusinessUnitId", analyticsDayOff.BusinessUnitId)
				.SetInt32("DatasourceId", analyticsDayOff.DatasourceId)
				.SetDateTime("DatasourceUpdateDate", analyticsDayOff.DatasourceUpdateDate)
				.SetString("DisplayColorHtml", analyticsDayOff.DisplayColorHtml)
				.SetString("DayOffShortName", analyticsDayOff.DayOffShortname);
			query.ExecuteUpdate();
		}

		public void AddNotDefined()
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(@"exec mart.[etl_dim_day_off_insert_not_defined]")
				.ExecuteUpdate();
		}

		public IList<AnalyticsDayOff> DayOffs()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"select 
						  day_off_id DayOffId
						, day_off_code DayOffCode
						, day_off_name DayOffName
						, business_unit_id BusinessUnitId
						, datasource_id DatasourceId
						, datasource_update_date DatasourceUpdateDate
						, day_off_shortname DayOffShortname
						, display_color DisplayColor
						, display_color_html DisplayColorHtml
						from mart.dim_day_off WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDayOff)))
				.SetReadOnly(true)
				.List<AnalyticsDayOff>();
		}
	}
}
