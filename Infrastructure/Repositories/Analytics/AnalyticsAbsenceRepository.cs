﻿using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsAbsenceRepository : IAnalyticsAbsenceRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsAbsenceRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<AnalyticsAbsence> Absences()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"select [absence_id] AbsenceId
					,[absence_code] AbsenceCode
					,[absence_name] AbsenceName
					,[display_color] DisplayColor
					,[in_contract_time] InContractTime
					,[in_contract_time_name] InContractTimeName
					,[in_paid_time] InPaidTime
					,[in_paid_time_name] InPaidTimeName
					,[in_work_time] InWorkTime
					,[in_work_time_name] InWorkTimeName
					,[business_unit_id] BusinessUnitId
					,[datasource_id] DatasourceId
					,[insert_date] InsertDate
					,[update_date] UpdateDate
					,[datasource_update_date] DatasourceUpdateDate
					,[is_deleted] IsDeleted
					,[display_color_html] DisplayColorHtml
					,[absence_shortname] AbsenceShortName
					from [mart].[dim_absence] WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsAbsence)))
				.SetReadOnly(true)
				.List<AnalyticsAbsence>();
		}

		public void AddAbsence(AnalyticsAbsence analyticsAbsence)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateAbsence(AnalyticsAbsence analyticsAbsence)
		{
			throw new System.NotImplementedException();
		}
	}
}