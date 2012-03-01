namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers
{
	using System.Collections.Generic;
	using System.Linq;

	using Teleopti.Ccc.Domain.Security.AuthorizationData;
	using Teleopti.Ccc.Domain.Security.Matrix;
	using Teleopti.Ccc.Domain.Specification;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;
	using Teleopti.Ccc.Web.Core.RequestContext;
	using Teleopti.Interfaces.Domain;

	public class DefinedReportProvider : IDefinedReportProvider
	{
		private readonly ICurrentPrincipalProvider _grantPrincipalProvider;

		public DefinedReportProvider(ICurrentPrincipalProvider grantPrincipalProvider)
		{
			_grantPrincipalProvider = grantPrincipalProvider;
		}

		public IDefinedReport Get(string reportId)
		{
			return this.GetDefinedReports().FirstOrDefault(r => r.ReportId.Equals(reportId));
		}

		public IEnumerable<DefinedReportInformation> GetDefinedReports()
		{
			var externalApplicationFunctionSpecification =
				new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);
			var definedReportFunctionSpecification = new DefinedReportFunctionSpecification(DefinedReports.ReportInformations);

			var grantedFunctionsBySpecification =
				_grantPrincipalProvider.Current().PrincipalAuthorization.GrantedFunctionsBySpecification(
					externalApplicationFunctionSpecification.And(definedReportFunctionSpecification));
			var grantedFunctions = grantedFunctionsBySpecification.Select(f => f.FunctionCode);

			return DefinedReports.ReportInformations.Where(r => grantedFunctions.Contains(r.FunctionCode)).ToList();
		}

		private class DefinedReportFunctionSpecification : Specification<IApplicationFunction>
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