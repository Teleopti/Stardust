using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Reports
{
	[DomainTest]
	public class ScheduleAnalysisAuditTrailProviderTest
	{
		private ScheduleAnalysisAuditTrailProvider _target;
		

		[SetUp]
		public void Setup()
		{
			_target = new ScheduleAnalysisAuditTrailProvider();
		}

		[Test]
		public void ShouldProvideAuditTrailApplicationFunctions()
		{
			var applicationFunctions = new List<IApplicationFunction>
			{
				new ApplicationFunction {ForeignId = "0148", ForeignSource = DefinedForeignSourceNames.SourceRaptor}
			};
			var result = _target.GetScheduleAnalysisApplicationFunctions(applicationFunctions);

			result.Count.Should().Be.EqualTo(1);
			result.First().ForeignId.Should().Be.EqualTo(applicationFunctions.First().ForeignId);
		}
	}
}
