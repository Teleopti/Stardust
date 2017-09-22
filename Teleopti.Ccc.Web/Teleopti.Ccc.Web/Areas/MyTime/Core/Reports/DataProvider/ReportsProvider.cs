using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public class ReportsProvider: IReportsProvider
	{
		private readonly IAuthorization _authorization;

		public ReportsProvider(IAuthorization authorization)
		{
			_authorization = authorization;
		}

		public IEnumerable<IApplicationFunction> GetReports()
		{
			return
				_authorization.GrantedFunctions().FilterBySpecification(
					new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix)).ToList();
		}
	}

}