using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers
{
	public class DefinedReportProvider : IDefinedReportProvider
	{
		private readonly IPrincipalAuthorization _principalAuthorization;

		public DefinedReportProvider(IPrincipalAuthorization principalAuthorization)
		{
			_principalAuthorization = principalAuthorization;
		}

		public IDefinedReport Get(string reportId)
		{
			return GetDefinedReports().FirstOrDefault(r => r.ReportId.Equals(reportId));
		}

		public IEnumerable<DefinedReportInformation> GetDefinedReports()
		{
			var externalApplicationFunctionSpecification =
				new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);
			var definedReportFunctionSpecification = new DefinedReportFunctionSpecification(DefinedReports.ReportInformations);

			var grantedFunctionsBySpecification = _principalAuthorization.GrantedFunctionsBySpecification(
				externalApplicationFunctionSpecification.And(definedReportFunctionSpecification)
				);
			var grantedFunctions = grantedFunctionsBySpecification.Select(f => f.FunctionCode);

			return DefinedReports.ReportInformations.Where(r => grantedFunctions.Contains(r.FunctionCode)).ToList();
		}

		public class DefinedReportFunctionSpecification : Specification<IApplicationFunction>
		{
			private readonly string[] _definedFunctionsList;

			public DefinedReportFunctionSpecification(IEnumerable<IDefinedReport> definedReports)
			{
				_definedFunctionsList = definedReports.Select(f => f.FunctionCode).ToArray();
			}

			public override bool IsSatisfiedBy(IApplicationFunction obj)
			{
				return _definedFunctionsList.Contains(obj.FunctionCode);
			}
		}
	}
}