using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public interface ILogOnHelper : IDisposable
	{
		IList<IBusinessUnit> GetBusinessUnitCollection();
		List<ITenantName> TenantCollection { get; }
		IDataSourceContainer SelectedDataSourceContainer { get; }
		bool SetBusinessUnit(IBusinessUnit businessUnit);
		bool SelectDataSourceContainer(string dataSourceName);
		void RefreshTenantList();
	}
}