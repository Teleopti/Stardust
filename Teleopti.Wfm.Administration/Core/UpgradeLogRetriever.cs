using System;
using System.Collections.Generic;
using NHibernate.Exceptions;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Wfm.Administration.Core
{
	public interface IUpgradeLogRetriever
	{
		IList<UpgradeLog> GetUpgradeLog(int tenantId);
	}
	public class UpgradeLogRetriever: IUpgradeLogRetriever
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public UpgradeLogRetriever(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public IList<UpgradeLog> GetUpgradeLog(int tenantId)
		{
			return
				_currentTenantSession.CurrentSession()
					.CreateSQLQuery("SELECT Tenant, Time, Level, Message FROM Tenant.UpgradeLog WHERE TenantId = :tenantId")
					.SetInt32("tenantId", tenantId)
					.SetResultTransformer(Transformers.AliasToBean(typeof (UpgradeLog)))
					.SetReadOnly(true)
					.List<UpgradeLog>();
		}
	}

	public class UpgradeLog
	{
		public string Tenant { get; set; }
		public DateTime Time { get; set; }
		public string Level { get; set; }
		public string Message { get; set; }
	}
}