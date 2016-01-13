using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	public class JobHelperForTest : JobHelper
	{
		public JobHelperForTest(IRaptorRepository repository, IMessageSender messageSender)
			: base(repository, messageSender)
		{
		}
	}
}
