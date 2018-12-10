using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class QueueMetricsForDayQuery : IQueueMetricsForDayQuery
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		private const string tsql =
            @"exec mart.report_data_agent_queue_metrics_web_metrics 
									@date_from=:date_from,
									@person_code=:person_code, 
									@business_unit_code=:business_unit_code";

	    public QueueMetricsForDayQuery(ILoggedOnUser loggedOnUser, ICurrentDataSource currentDataSource,
	        ICurrentBusinessUnit currentBusinessUnit)
	    {
	        _loggedOnUser = loggedOnUser;
	        _currentDataSource = currentDataSource;
	        _currentBusinessUnit = currentBusinessUnit;
	    }

	    public ICollection<QueueMetricsForDayResult> Execute(DateOnly date)
		{
            using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
            {
                return uow.Session().CreateSQLQuery(tsql)
                    .AddScalar("Person", NHibernateUtil.String)
                    .AddScalar("Queue", NHibernateUtil.String)
                    .AddScalar("AnsweredCalls", NHibernateUtil.Int32)
                    .AddScalar("AverageAfterCallWork", NHibernateUtil.Double)
                    .AddScalar("AverageHandlingTime", NHibernateUtil.Double)
                    .AddScalar("AverageTalkTime", NHibernateUtil.Double)
                    .SetDateTime("date_from", date.Date)
                    .SetGuid("person_code", _loggedOnUser.CurrentUser().Id.Value)
                    .SetGuid("business_unit_code", _currentBusinessUnit.Current().Id.Value)
                    .SetResultTransformer(new queueMetricsForDayResultTransformer())
                    .List<QueueMetricsForDayResult>();
            }
		}

		private class queueMetricsForDayResultTransformer : IResultTransformer
		{
			public object TransformTuple(object[] tuple, string[] aliases)
			{
				var ret = new QueueMetricsForDayResult();
				for (var i = 0; i < aliases.Length; i++)
				{
					var tupleValue = tuple[i];
					if (tupleValue == null) continue;

					switch (aliases[i])
					{
						case "Person":
							ret.Person = (string)tupleValue;
							break;
						case "Queue":
							ret.Queue = (string)tupleValue;
							break;
						case "AnsweredCalls":
							ret.AnsweredCalls = (int)tupleValue;
							break;
						case "AverageAfterCallWork":
							ret.AverageAfterCallWorkTime = TimeSpan.FromSeconds((double)tupleValue);
							break;
						case "AverageTalkTime":
							ret.AverageTalkTime = TimeSpan.FromSeconds((double)tupleValue);
							break;
						case "AverageHandlingTime":
							ret.AverageHandlingTime = TimeSpan.FromSeconds((double)tupleValue);
							break;
					}
				}
				return ret;
			}

			public IList TransformList(IList collection)
			{
				return collection.Cast<QueueMetricsForDayResult>().ToList();
			}
		}
	}
}