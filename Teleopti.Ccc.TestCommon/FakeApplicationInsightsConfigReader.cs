using System;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeApplicationInsightsConfigReader : IApplicationInsightsConfigurationReader
	{
		private string iKey = Guid.Empty.ToString();

		public string InstrumentationKey()
		{
			return iKey;
		}

		public void SetInstrumentationKey(string key)
		{
			iKey = key;
		}
	}
}
