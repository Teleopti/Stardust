using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class DetailedAdherenceForDayQuery : IDetailedAdherenceForDayQuery
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		private const string tsql =
			@"exec mart.report_data_agent_schedule_web_adherence 
									@date_from=:date_from,
									@person_code=:person_code, 
									@adherence_id=:adherence_id, 
									@business_unit_code=:business_unit_code";

		public DetailedAdherenceForDayQuery(ILoggedOnUser loggedOnUser, ICurrentDataSource currentDataSource, ICurrentBusinessUnit currentBusinessUnit, IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_loggedOnUser = loggedOnUser;
			_currentDataSource = currentDataSource;
			_currentBusinessUnit = currentBusinessUnit;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public ICollection<DetailedAdherenceForDayResult> Execute(DateOnly date)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(tsql)
					.AddScalar("ShiftDate", NHibernateUtil.DateTime)
					.AddScalar("IntervalsPerDay", NHibernateUtil.Int32)
					.AddScalar("IntervalId", NHibernateUtil.Int32)
					.AddScalar("Adherence", NHibernateUtil.Double)
					.AddScalar("TotalAdherence", NHibernateUtil.Double)
					.AddScalar("Deviation", NHibernateUtil.Int32)
					.AddScalar("DisplayColor", NHibernateUtil.Int32)
					.AddScalar("DisplayName", NHibernateUtil.String)
					.AddScalar("IntervalCounter", NHibernateUtil.Int32)
					.SetDateTime("date_from", date.Date)
					.SetInt32("adherence_id", (int)_globalSettingDataRepository.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting()).AdherenceIdForReport())
					.SetGuid("person_code", _loggedOnUser.CurrentUser().Id.Value)
					.SetGuid("business_unit_code", _currentBusinessUnit.Current().Id.Value)
					.SetResultTransformer(new detailedAdherenceForDayResultTransformer())
					.List<DetailedAdherenceForDayResult>();
			}
		}

		private class detailedAdherenceForDayResultTransformer : IResultTransformer
		{
			public object TransformTuple(object[] tuple, string[] aliases)
			{
				var ret = new DetailedAdherenceForDayResult();
				for (var i = 0; i < aliases.Length; i++)
				{
					var tupleValue = tuple[i];
					if (tupleValue == null) continue;

					switch (aliases[i])
					{
						case "ShiftDate":
							ret.ShiftDate = new DateOnly((DateTime)tupleValue);
							break;
						case "IntervalsPerDay":
							ret.IntervalsPerDay = (int)tupleValue;
							break;
						case "IntervalId":
							ret.IntervalId = (int)tupleValue;
							break;
						case "IntervalCounter":
							ret.IntervalCounter = (int)tupleValue;
							break;
						case "TotalAdherence":
							ret.TotalAdherence = new Percent((double)tupleValue);
							break;
						case "Deviation":
							ret.Deviation = (int)tupleValue;
							break;
						case "DisplayColor":
							ret.DisplayColor = Color.FromArgb((int)tupleValue);
							break;
						case "DisplayName":
							ret.DisplayName = (string)tupleValue;
							break;
						case "Adherence":
							ret.Adherence = (double)tupleValue;
							break;
					}
				}
				return ret;
			}

			public IList TransformList(IList collection)
			{
				return collection.Cast<DetailedAdherenceForDayResult>().ToList();
			}
		}
	}
}