using System;
using System.Collections.Generic;
using System.Globalization;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class EtlJobStatusRepository : IEtlJobStatusRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public EtlJobStatusRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public IEnumerable<EtlJobStatusModel> Load(DateOnly date, bool showOnlyErrors)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				const string tsql = "EXEC [mart].[etl_job_execution_history] :start_date,:end_date,:business_unit_id,:show_only_errors";
				return uow.Session().CreateSQLQuery(tsql)
					.AddScalar("job_name", NHibernateUtil.String)
					.AddScalar("business_unit_name", NHibernateUtil.String)
					.AddScalar("job_start_time", NHibernateUtil.UtcDateTime)
					.AddScalar("job_end_time", NHibernateUtil.UtcDateTime)
					.AddScalar("job_duration_s", NHibernateUtil.Int32)
					.AddScalar("job_affected_rows", NHibernateUtil.Int32)
					.AddScalar("schedule_name", NHibernateUtil.String)
					.AddScalar("jobstep_name", NHibernateUtil.String)
					.AddScalar("jobstep_duration_s", NHibernateUtil.Int32)
					.AddScalar("jobstep_affected_rows", NHibernateUtil.Int32)
					.AddScalar("exception_msg", NHibernateUtil.String)
					.AddScalar("exception_trace", NHibernateUtil.String)
					.AddScalar("inner_exception_msg", NHibernateUtil.String)
					.AddScalar("inner_exception_trace", NHibernateUtil.String)
					.SetDateTime("start_date", DateHelper.GetFirstDateInWeek(date.Date,CultureInfo.CurrentCulture))
					.SetDateTime("end_date", DateHelper.GetLastDateInWeek(date.Date, CultureInfo.CurrentCulture))
					.SetBoolean("show_only_errors", showOnlyErrors)
					.SetGuid("business_unit_id", new Guid("00000000-0000-0000-0000-000000000002"))
					.SetResultTransformer(new AliasToBeanResultTransformer(typeof(EtlJobStatusModel)))
					.List<EtlJobStatusModel>();
			}
		}
	}
}