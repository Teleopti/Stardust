using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
	public interface IJobHelper : IDisposable
	{
		IList<IBusinessUnit> BusinessUnitCollection { get; }
		IRaptorRepository Repository { get; }
		ISignalRClient MessageClient { get; }
		IMessageSender MessageSender { get; }
		bool LogOnTeleoptiCccDomain(IBusinessUnit businessUnit);
		void LogOffTeleoptiCccDomain();
	}
}