using System.Collections.Specialized;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	public class FakeConfigReader : IConfigReader
	{
		public FakeConfigReader()
		{
			AppSettings = new NameValueCollection();
		}

		public NameValueCollection AppSettings { get; set; }
	}
}