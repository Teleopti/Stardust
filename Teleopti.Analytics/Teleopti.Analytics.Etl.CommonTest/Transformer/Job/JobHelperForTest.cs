using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job
{
	public class JobHelperForTest : JobHelper
	{
		public JobHelperForTest(IRaptorRepository repository, IMessageSender messageSender) 
			: base(repository, messageSender)
		{
		}
	}

}
