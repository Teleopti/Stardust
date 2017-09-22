using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;

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