using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Reports.Models;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public class ReportNavigationProvider : IReportNavigationProvider
	{
		private readonly IAuthorization _authorization;
		private readonly IReportUrl _reportUrl;

		public ReportNavigationProvider(IAuthorization authorization, IReportUrl reportUrl)
		{
			_authorization = authorization;
			_reportUrl = reportUrl;
		}

		public IList<ReportNavigationItem> GetNavigationItems()
		{
			var grantedFuncs = _authorization.GrantedFunctions().FilterBySpecification(
					new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix)).ToList().OrderBy(x => x.LocalizedFunctionDescription);

			return grantedFuncs.Select(applicationFunction => new ReportNavigationItem
			{
				Url = _reportUrl.Build(applicationFunction.ForeignId),
				Title = applicationFunction.LocalizedFunctionDescription,
				Id = new Guid(applicationFunction.ForeignId)
			}).ToList();
		}
	}
}