using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IJobHelper
	{
		IList<IBusinessUnit> BusinessUnitCollection { get; }
		IDataSource SelectedDataSource { get; }
		bool SelectDataSourceContainer(string dataSourceName);
		bool SetBusinessUnit(IBusinessUnit businessUnit);

		IRaptorRepository Repository { get; }
		IMessageSender MessageSender { get; }
		void LogOffTeleoptiCccDomain();
	}
}