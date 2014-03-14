using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.DataProvider
{
	[TestFixture]
	public class ReportsProviderTest
	{
		[Test]
		public void ShouldReturnPermittedReports()
		{
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var reportList = new List<IApplicationFunction> { new ApplicationFunction("Report CA"), new ApplicationFunction("ResReportAbandonmentAndSpeedOfAnswer") };
			principalAuthorization.Stub(x => x.GrantedFunctionsBySpecification(new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix))).IgnoreArguments().Return(reportList);

			var target = new ReportsProvider(principalAuthorization);

			var result = target.GetReports();

			result.Count().Should().Be(1);
		}

	}
}