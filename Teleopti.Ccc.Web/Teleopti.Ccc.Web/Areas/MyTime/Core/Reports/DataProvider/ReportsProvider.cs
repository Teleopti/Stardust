using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
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
			return
				_principalAuthorization.GrantedFunctionsBySpecification(
					new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix)).ToList();
		}
	}

}