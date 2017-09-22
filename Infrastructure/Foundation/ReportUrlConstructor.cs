using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ReportUrlConstructor : IReportUrl
	{
		private readonly string _reportServer;
		private readonly IConfigReader _configReader;

		public ReportUrlConstructor(string reportServer, IConfigReader configReader)
		{
			_reportServer = reportServer;
			_configReader = configReader;
		}

		public string Build(string reportId)
		{
			var matrixWebsiteUrl = _reportServer ?? "/";
			if (!matrixWebsiteUrl.EndsWith("/")) matrixWebsiteUrl += "/";

			var url = string.Format(CultureInfo.InvariantCulture, "{0}Reporting/Report/{1}#{2}",
											matrixWebsiteUrl, reportId, reportId);
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			return _configReader.ReadValue("UseRelativeConfiguration", false)
				? "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)
				: uri.ToString();
		}
	}
}