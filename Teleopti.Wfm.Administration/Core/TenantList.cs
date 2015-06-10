using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantList
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public TenantList(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public IList<TenantModel> GetTenantList()
		{
			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("loadAllTenants")
				.SetResultTransformer(Transformers.AliasToBean<TenantModel>())
				.List<TenantModel>();

			//return new List<TenantModel>
			//{
			//	new TenantModel {AnalyticsDatabase = "Tenant ett Analytics", AppDatabase = "Tenant ett Database", Name = "My first Tenant"},
			//	new TenantModel {AnalyticsDatabase = "Tenant två Analytics", AppDatabase = "Tenant två Database", Name = "My second Tenant"}
			//};
		}
	}


	public class TenantModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		//public string AppDatabase { get; set; }
		//public string AnalyticsDatabase { get; set; }

	}
}

