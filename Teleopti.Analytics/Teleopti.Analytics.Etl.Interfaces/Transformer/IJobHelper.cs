using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
	public interface IJobHelper : IDisposable
	{
		IList<IBusinessUnit> BusinessUnitCollection { get; }
		IList<DataSourceContainer> DataSourceContainers { get; }
		IRaptorRepository Repository { get; }
		ISignalRClient MessageClient { get; }
		IMessageSender MessageSender { get; }
		bool SelectDataSourceContainer(string dataSourceName);
		bool SetBusinessUnit(IBusinessUnit businessUnit);
		void LogOffTeleoptiCccDomain();
	}
}