using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.ScheduleThreading
{
	public class JobHelperForScenarioTests : IJobHelper
	{
		public JobHelperForScenarioTests(IRaptorRepository raptorRepository)
		{
			Repository = raptorRepository;
		}

		public IList<IBusinessUnit> BusinessUnitCollection { get; }
		public IDataSource SelectedDataSource { get; }
		public bool SelectDataSourceContainer(string dataSourceName)
		{
			throw new NotImplementedException();
		}

		public bool SetBusinessUnit(IBusinessUnit businessUnit)
		{
			throw new NotImplementedException();
		}

		public IRaptorRepository Repository { get; }
		public IMessageSender MessageSender { get; }
		public void LogOffTeleoptiCccDomain()
		{
			throw new NotImplementedException();
		}
	}
}
