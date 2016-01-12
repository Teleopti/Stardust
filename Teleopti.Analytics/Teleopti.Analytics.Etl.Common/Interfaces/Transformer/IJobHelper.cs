using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IJobHelper
	{
		IList<IBusinessUnit> BusinessUnitCollection { get; }
		IEnumerable<TenantInfo> TenantCollection { get; }
		IRaptorRepository Repository { get; }
		IMessageSender MessageSender { get; }
		bool SelectDataSourceContainer(string dataSourceName);
		bool SetBusinessUnit(IBusinessUnit businessUnit);
		void LogOffTeleoptiCccDomain();
		IDataSource SelectedDataSource { get; }
		void RefreshTenantList();
	}
}