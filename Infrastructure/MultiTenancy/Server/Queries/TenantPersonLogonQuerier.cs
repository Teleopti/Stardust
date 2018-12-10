using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class TenantPersonLogonQuerier : ITenantPersonLogonQuerier
	{
		private const int BatchSize = 500;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IFindTenantByName _findTenant;
		private readonly ICurrentTenantSession _currentTenantSession;


		public TenantPersonLogonQuerier(ICurrentDataSource currentDataSource, IFindTenantByName findTenant, ICurrentTenantSession currentTenantSession)
		{
			_currentDataSource = currentDataSource;
			_findTenant = findTenant;
			_currentTenantSession = currentTenantSession;
		}

		public IEnumerable<IPersonInfoModel> FindApplicationLogonUsers(IEnumerable<string> logonNames)
		{
			var currentTenant = _findTenant.Find(_currentDataSource.CurrentName());
			if (currentTenant == null) return new List<PersonInfoModel>();

			var results = new List<PersonInfoModel>();

			foreach (var names in logonNames.Batch(BatchSize))
			{
				var p = _currentTenantSession.CurrentSession()
					.GetNamedQuery("findIPersonInfoModelByLogonNames")
					.SetResultTransformer(Transformers.AliasToBean<PersonInfoModel>())
					.SetParameterList("logonNames", names)
					.SetEntity("tenant", currentTenant)
					.List<PersonInfoModel>();
				results.AddRange(p);
			}
			return results;
		}
	}
}
