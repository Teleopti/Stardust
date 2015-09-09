using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IJobHelper : IDisposable
	{
		IList<IBusinessUnit> BusinessUnitCollection { get; }
		List<ITenantName> TenantCollection { get; }
		IRaptorRepository Repository { get; }
		ISignalRClient MessageClient { get; }
		IMessageSender MessageSender { get; }
		bool SelectDataSourceContainer(string dataSourceName);
		bool SetBusinessUnit(IBusinessUnit businessUnit);
		void LogOffTeleoptiCccDomain();
		IDataSource SelectedDataSource { get; }
		void RefreshTenantList();
	}

	public interface ITenantName
	{
		string DataSourceName { get; set; }
	}
}