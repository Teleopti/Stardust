using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsAbsenceRepository : IAnalyticsAbsenceRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;

		public AnalyticsAbsenceRepository(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork)
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
		}

		public IList<AnalyticsAbsence> Absences()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select [absence_id] {nameof(AnalyticsAbsence.AbsenceId)}
					,[absence_code] {nameof(AnalyticsAbsence.AbsenceCode)}
					,[absence_name] {nameof(AnalyticsAbsence.AbsenceName)}
					,[display_color] {nameof(AnalyticsAbsence.DisplayColor)}
					,[in_contract_time] {nameof(AnalyticsAbsence.InContractTime)}
					,[in_contract_time_name] {nameof(AnalyticsAbsence.InContractTimeName)}
					,[in_paid_time] {nameof(AnalyticsAbsence.InPaidTime)}
					,[in_paid_time_name] {nameof(AnalyticsAbsence.InPaidTimeName)}
					,[in_work_time] {nameof(AnalyticsAbsence.InWorkTime)}
					,[in_work_time_name] {nameof(AnalyticsAbsence.InWorkTimeName)}
					,[business_unit_id] {nameof(AnalyticsAbsence.BusinessUnitId)}
					,[datasource_id] {nameof(AnalyticsAbsence.DatasourceId)}
					,[insert_date] {nameof(AnalyticsAbsence.InsertDate)}
					,[update_date] {nameof(AnalyticsAbsence.UpdateDate)}
					,[datasource_update_date] {nameof(AnalyticsAbsence.DatasourceUpdateDate)}
					,[is_deleted] {nameof(AnalyticsAbsence.IsDeleted)}
					,[display_color_html] {nameof(AnalyticsAbsence.DisplayColorHtml)}
					,[absence_shortname] {nameof(AnalyticsAbsence.AbsenceShortName)}
					from [mart].[dim_absence] WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsAbsence)))
				.SetReadOnly(true)
				.List<AnalyticsAbsence>();
		}

		public void AddAbsence(AnalyticsAbsence analyticsAbsence)
		{
			var query = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_absence_insert]
						@absence_code=:{nameof(AnalyticsAbsence.AbsenceCode)}, 
						@absence_name=:{nameof(AnalyticsAbsence.AbsenceName)}, 
						@display_color=:{nameof(AnalyticsAbsence.DisplayColor)}, 
						@in_contract_time=:{nameof(AnalyticsAbsence.InContractTime)}, 
						@in_contract_time_name=:{nameof(AnalyticsAbsence.InContractTimeName)}, 
						@in_paid_time=:{nameof(AnalyticsAbsence.InPaidTime)}, 
						@in_paid_time_name=:{nameof(AnalyticsAbsence.InPaidTimeName)}, 
						@in_work_time=:{nameof(AnalyticsAbsence.InWorkTime)}, 
						@in_work_time_name=:{nameof(AnalyticsAbsence.InWorkTimeName)}, 
						@business_unit_id=:{nameof(AnalyticsAbsence.BusinessUnitId)},
						@datasource_id=:{nameof(AnalyticsAbsence.DatasourceId)},
						@datasource_update_date=:{nameof(AnalyticsAbsence.DatasourceUpdateDate)},
						@is_deleted=:{nameof(AnalyticsAbsence.IsDeleted)},
						@display_color_html=:{nameof(AnalyticsAbsence.DisplayColorHtml)},
						@absence_shortname=:{nameof(AnalyticsAbsence.AbsenceShortName)}
					  ")
				.SetGuid(nameof(AnalyticsAbsence.AbsenceCode), analyticsAbsence.AbsenceCode)
				.SetString(nameof(AnalyticsAbsence.AbsenceName), analyticsAbsence.AbsenceName)
				.SetInt32(nameof(AnalyticsAbsence.DisplayColor), analyticsAbsence.DisplayColor)
				.SetBoolean(nameof(AnalyticsAbsence.InContractTime), analyticsAbsence.InContractTime)
				.SetString(nameof(AnalyticsAbsence.InContractTimeName), analyticsAbsence.InContractTimeName)
				.SetBoolean(nameof(AnalyticsAbsence.InPaidTime), analyticsAbsence.InPaidTime)
				.SetString(nameof(AnalyticsAbsence.InPaidTimeName), analyticsAbsence.InPaidTimeName)
				.SetBoolean(nameof(AnalyticsAbsence.InWorkTime), analyticsAbsence.InWorkTime)
				.SetString(nameof(AnalyticsAbsence.InWorkTimeName), analyticsAbsence.InWorkTimeName)
				.SetInt32(nameof(AnalyticsAbsence.BusinessUnitId), analyticsAbsence.BusinessUnitId)
				.SetInt32(nameof(AnalyticsAbsence.DatasourceId), analyticsAbsence.DatasourceId)
				.SetDateTime(nameof(AnalyticsAbsence.DatasourceUpdateDate), analyticsAbsence.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsAbsence.IsDeleted), analyticsAbsence.IsDeleted)
				.SetString(nameof(AnalyticsAbsence.DisplayColorHtml), analyticsAbsence.DisplayColorHtml)
				.SetString(nameof(AnalyticsAbsence.AbsenceShortName), analyticsAbsence.AbsenceShortName);
			query.ExecuteUpdate();
		}

		public void UpdateAbsence(AnalyticsAbsence analyticsAbsence)
		{
			var query = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_absence_update]
						@absence_code=:{nameof(AnalyticsAbsence.AbsenceCode)}, 
						@absence_name=:{nameof(AnalyticsAbsence.AbsenceName)}, 
						@display_color=:{nameof(AnalyticsAbsence.DisplayColor)}, 
						@in_contract_time=:{nameof(AnalyticsAbsence.InContractTime)}, 
						@in_contract_time_name=:{nameof(AnalyticsAbsence.InContractTimeName)}, 
						@in_paid_time=:{nameof(AnalyticsAbsence.InPaidTime)}, 
						@in_paid_time_name=:{nameof(AnalyticsAbsence.InPaidTimeName)}, 
						@in_work_time=:{nameof(AnalyticsAbsence.InWorkTime)}, 
						@in_work_time_name=:{nameof(AnalyticsAbsence.InWorkTimeName)}, 
						@business_unit_id=:{nameof(AnalyticsAbsence.BusinessUnitId)},
						@datasource_id=:{nameof(AnalyticsAbsence.DatasourceId)},
						@datasource_update_date=:{nameof(AnalyticsAbsence.DatasourceUpdateDate)},
						@is_deleted=:{nameof(AnalyticsAbsence.IsDeleted)},
						@display_color_html=:{nameof(AnalyticsAbsence.DisplayColorHtml)},
						@absence_shortname=:{nameof(AnalyticsAbsence.AbsenceShortName)}
					  ")
				.SetGuid(nameof(AnalyticsAbsence.AbsenceCode), analyticsAbsence.AbsenceCode)
				.SetString(nameof(AnalyticsAbsence.AbsenceName), analyticsAbsence.AbsenceName)
				.SetInt32(nameof(AnalyticsAbsence.DisplayColor), analyticsAbsence.DisplayColor)
				.SetBoolean(nameof(AnalyticsAbsence.InContractTime), analyticsAbsence.InContractTime)
				.SetString(nameof(AnalyticsAbsence.InContractTimeName), analyticsAbsence.InContractTimeName)
				.SetBoolean(nameof(AnalyticsAbsence.InPaidTime), analyticsAbsence.InPaidTime)
				.SetString(nameof(AnalyticsAbsence.InPaidTimeName), analyticsAbsence.InPaidTimeName)
				.SetBoolean(nameof(AnalyticsAbsence.InWorkTime), analyticsAbsence.InWorkTime)
				.SetString(nameof(AnalyticsAbsence.InWorkTimeName), analyticsAbsence.InWorkTimeName)
				.SetInt32(nameof(AnalyticsAbsence.BusinessUnitId), analyticsAbsence.BusinessUnitId)
				.SetInt32(nameof(AnalyticsAbsence.DatasourceId), analyticsAbsence.DatasourceId)
				.SetDateTime(nameof(AnalyticsAbsence.DatasourceUpdateDate), analyticsAbsence.DatasourceUpdateDate==DateTime.MinValue?DateTime.UtcNow: analyticsAbsence.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsAbsence.IsDeleted), analyticsAbsence.IsDeleted)
				.SetString(nameof(AnalyticsAbsence.DisplayColorHtml), analyticsAbsence.DisplayColorHtml)
				.SetString(nameof(AnalyticsAbsence.AbsenceShortName), analyticsAbsence.AbsenceShortName);
			query.ExecuteUpdate();
		}
	}
}