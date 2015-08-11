using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeServiceBusSender : IServiceBusSender
	{
		public List<object> SentMessages = new List<object>();
		public bool WasEnqueued { get { return SentMessages.Any(); } }

		public void Send(bool throwOnNoBus, params object[] message)
		{
			SentMessages.AddRange(message);
		}

		public void Dispose()
		{
		}

	}
}