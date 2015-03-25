using System;
using System.Globalization;
using Teleopti.Ccc.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ReportUrlConstructor : IReportUrl
	{
		private readonly string _reportServerPath;
		private readonly IConfigReader _configReader;

		public ReportUrlConstructor(string reportServerPath, IConfigReader configReader)
		{
			_reportServerPath = reportServerPath;
			_configReader = configReader;
		}

		public string Build(string reportId)
		{
			var matrixWebsiteUrl = _reportServerPath ?? "/";
			if (!matrixWebsiteUrl.EndsWith("/")) matrixWebsiteUrl += "/";

			var url = string.Format(CultureInfo.InvariantCulture, "{0}Reporting/Report/{1}#{2}",
											matrixWebsiteUrl, reportId, reportId);
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			return _configReader.AppSettings.GetBoolSetting("UseRelativeConfiguration")
				? "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)
				: uri.ToString();
		}
	}
}