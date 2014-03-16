using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public class ReportsProvider: IReportsProvider
	{
		private readonly IPrincipalAuthorization _principalAuthorization;

		public ReportsProvider(IPrincipalAuthorization principalAuthorization)
		{
			_principalAuthorization = principalAuthorization;
		}

		public IEnumerable<IApplicationFunction> GetReports()
		{
			var externalApplicationFunctionSpecification =
				new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);
			var mobileReportFunctionSpecification = DefinedReports.ReportInformations;

			var allPermittedReports = _principalAuthorization.GrantedFunctionsBySpecification(externalApplicationFunctionSpecification).ToList();
			var query =
				from c in allPermittedReports
				where !(from o in mobileReportFunctionSpecification select o.FunctionCode)
					.Contains(c.FunctionCode)
				select c;
			return query;
		}
	}

	public interface IReportsProvider
	{
		IEnumerable<IApplicationFunction> GetReports();
	}
}