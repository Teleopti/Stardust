using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
				$@"exec mart.[etl_dim_day_off_insert_or_update]
						@day_off_code=:{nameof(AnalyticsDayOff.DayOffCode)}, 
						@day_off_name=:{nameof(AnalyticsDayOff.DayOffName)}, 
						@display_color=:{nameof(AnalyticsDayOff.DisplayColor)},
						@business_unit_id=:{nameof(AnalyticsDayOff.BusinessUnitId)},
						@datasource_id=:{nameof(AnalyticsDayOff.DatasourceId)},
						@datasource_update_date=:{nameof(AnalyticsDayOff.DatasourceUpdateDate)},
						@display_color_html=:{nameof(AnalyticsDayOff.DisplayColorHtml)},
						@day_off_shortname=:{nameof(AnalyticsDayOff.DayOffShortname)}
					  ")
				.SetGuid(nameof(AnalyticsDayOff.DayOffCode), analyticsDayOff.DayOffCode)
				.SetString(nameof(AnalyticsDayOff.DayOffName), analyticsDayOff.DayOffName)
				.SetInt32(nameof(AnalyticsDayOff.DisplayColor), analyticsDayOff.DisplayColor)
				.SetInt32(nameof(AnalyticsDayOff.BusinessUnitId), analyticsDayOff.BusinessUnitId)
				.SetInt32(nameof(AnalyticsDayOff.DatasourceId), analyticsDayOff.DatasourceId)
				.SetDateTime(nameof(AnalyticsDayOff.DatasourceUpdateDate), analyticsDayOff.DatasourceUpdateDate)
				.SetString(nameof(AnalyticsDayOff.DisplayColorHtml), analyticsDayOff.DisplayColorHtml)
				.SetString(nameof(AnalyticsDayOff.DayOffShortname), analyticsDayOff.DayOffShortname);
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
				$@"select 
						 day_off_id {nameof(AnalyticsDayOff.DayOffId)}
						,day_off_code {nameof(AnalyticsDayOff.DayOffCode)}
						,day_off_name {nameof(AnalyticsDayOff.DayOffName)}
						,business_unit_id {nameof(AnalyticsDayOff.BusinessUnitId)}
						,datasource_id {nameof(AnalyticsDayOff.DatasourceId)}
						,datasource_update_date {nameof(AnalyticsDayOff.DatasourceUpdateDate)}
						,day_off_shortname {nameof(AnalyticsDayOff.DayOffShortname)}
						,display_color {nameof(AnalyticsDayOff.DisplayColor)}
						,display_color_html {nameof(AnalyticsDayOff.DisplayColorHtml)}
						from mart.dim_day_off WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsDayOff)))
				.SetReadOnly(true)
				.List<AnalyticsDayOff>();
		}
	}
}
