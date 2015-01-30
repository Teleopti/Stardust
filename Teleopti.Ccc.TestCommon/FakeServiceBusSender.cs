using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeServiceBusSender : IServiceBusSender
	{
		public IList<object> SentMessages = new List<object>();
 
		public void Send(object message, bool throwOnNoBus)
		{
			SentMessages.Add(message);
		}

		public void Dispose()
		{
		}

	}
}