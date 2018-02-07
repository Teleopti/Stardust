using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Teleopti.Ccc.Domain.Config
{
	public class ApplicationInsightsConfigurationReader : IApplicationInsightsConfigurationReader
	{
		public string InstrumentationKey()
		{
			try
			{
				var path = AppDomain.CurrentDomain.BaseDirectory;
				var config = Path.Combine(path, "ApplicationInsights.config");
				var doc = XDocument.Load(config);
				var iKey = doc.Descendants(doc.Root.GetDefaultNamespace() + "InstrumentationKey").Select(t => t.Value).FirstOrDefault();

				return Guid.TryParse(iKey, out _) ? iKey : Guid.Empty.ToString();
			}
			catch (Exception)
			{
				//log blah blah
				return Guid.Empty.ToString();
			}
		}
	}

	public interface IApplicationInsightsConfigurationReader
	{
		string InstrumentationKey();
	}
}
