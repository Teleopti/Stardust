using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class TenantPersonLogonQuerier : ITenantPersonLogonQuerier
	{
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

			return _currentTenantSession.CurrentSession()
				.GetNamedQuery("findIPersonInfoModelByLogonNames")
				.SetResultTransformer(Transformers.AliasToBean<PersonInfoModel>())
				.SetParameterList("logonNames", logonNames)
				.SetEntity("tenant", currentTenant)
				.List<PersonInfoModel>();
		}
	}
}
