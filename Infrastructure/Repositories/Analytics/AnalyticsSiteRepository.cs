using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsSiteRepository : IAnalyticsSiteRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsSiteRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}


		public void UpdateName(Guid siteCode, string name)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"UPDATE mart.dim_site SET site_name=:{nameof(name)} WHERE site_code=:{nameof(siteCode)}")
				.SetString(nameof(name), name)
				.SetGuid(nameof(siteCode),siteCode)
				.ExecuteUpdate();
		}

		public IList<AnalyticsSite> GetSites()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery($@"select 
					site_code {nameof(AnalyticsSite.SiteCode)},
					site_name {nameof(AnalyticsSite.Name)}
					from mart.dim_site WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean<AnalyticsSite>())
				.List<AnalyticsSite>();
		}
	}
}