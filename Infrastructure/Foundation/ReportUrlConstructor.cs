using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

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

		public string Build(IApplicationFunction applicationFunction)
		{
			var matrixWebsiteUrl = _reportServer ?? "/";
			if (!matrixWebsiteUrl.EndsWith("/")) matrixWebsiteUrl += "/";

			string url = null;
			
			if (applicationFunction.ForeignId == DefinedRaptorApplicationFunctionForeignIds.ScheduleAuditTrailWebReport)
			{
				url = applicationFunction.IsWebReport ? "auditTrail" : string.Format(CultureInfo.InvariantCulture, "{0}WFM/#/report/audit-trail",matrixWebsiteUrl);
			}
			else if (applicationFunction.ForeignId == DefinedRaptorApplicationFunctionForeignIds.GeneralAuditTrailWebReport)
			{
				url = applicationFunction.IsWebReport ? "generalAuditTrail" : string.Format(CultureInfo.InvariantCulture, "{0}WFM/#/report/general-audit-trail", matrixWebsiteUrl);
			}
			else
				url = string.Format(CultureInfo.InvariantCulture, "{0}Reporting/Report/{1}#{2}",
					matrixWebsiteUrl, applicationFunction.ForeignId, applicationFunction.ForeignId);

			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			return (_configReader.ReadValue("UseRelativeConfiguration", false) && !applicationFunction.IsWebReport)
				? "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)
				: uri.ToString();

		}
	}
}