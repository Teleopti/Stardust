using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.DomainTest.Reports
{
	[TestFixture]
	public class ScheduleAnalysisProviderTest
	{
		private ScheduleAnalysisAuditTrailProvider _target;

		[SetUp]
		public void Setup()
		{
			_target = new ScheduleAnalysisAuditTrailProvider();
		}

		[Test]
		public void ShouldProvideAnalysisApplicationFunctions()
		{
			
			var applicationFunctions = new List<IApplicationFunction>
			{
				new ApplicationFunction {ForeignId = "132E3AF2-3557-4EA7-813E-05CD4869D5DB", ForeignSource = DefinedForeignSourceNames.SourceMatrix},
				new ApplicationFunction {ForeignId = "AnIdNotInAnalysisReports", ForeignSource = DefinedForeignSourceNames.SourceMatrix}
			};
			var result = _target.GetScheduleAnalysisApplicationFunctions(applicationFunctions);

			result.Count.Should().Be.EqualTo(1);
			result.First().ForeignId.Should().Be.EqualTo(applicationFunctions.First().ForeignId);
		}
	}
}
