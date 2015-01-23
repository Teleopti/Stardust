using System;
using System.Globalization;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportUrl : IReportUrl
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IConfigReader _configReader;

		public ReportUrl(ICurrentBusinessUnit currentBusinessUnit, IConfigReader configReader)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_configReader = configReader;
		}

		public string Build(string foreignId)
		{
			var businessId = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var matrixWebsiteUrl = _configReader.AppSettings["MatrixWebSiteUrl"] ?? "/";
			if (!matrixWebsiteUrl.EndsWith("/")) matrixWebsiteUrl += "/";
			
			var url = string.Format(CultureInfo.InvariantCulture, "{0}Selection.aspx?ReportId={1}&BuId={2}",
											matrixWebsiteUrl, foreignId, businessId);
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			return _configReader.AppSettings.GetBoolSetting("UseRelativeConfiguration")
				? "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)
				: uri.ToString();
		}
	}
}