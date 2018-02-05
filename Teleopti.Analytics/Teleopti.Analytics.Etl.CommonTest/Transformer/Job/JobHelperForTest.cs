using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job
{
	public class JobHelperForTest : JobHelper
	{
		public JobHelperForTest(IRaptorRepository repository, IMessageSender messageSender, ITenants tenants = null) 
			: base(repository, messageSender, tenants, null, null)
		{
		}
	}

}
