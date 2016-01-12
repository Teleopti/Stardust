using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public interface ILogOnHelper
	{
		IList<IBusinessUnit> GetBusinessUnitCollection();
		IEnumerable<TenantInfo> TenantCollection { get; }
		IDataSourceContainer SelectedDataSourceContainer { get; }
		bool SetBusinessUnit(IBusinessUnit businessUnit);
		bool SelectDataSourceContainer(string dataSourceName);
		void RefreshTenantList();
	}
}