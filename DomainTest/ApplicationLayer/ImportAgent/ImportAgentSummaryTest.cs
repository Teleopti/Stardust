using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ImportAgent
{
	[TestFixture]
	public class ImportAgentSummaryTest
	{
		[Test]
		public void ShouldGetTheCorrectSummaryCountFromMessage()
		{
			var result = new JobResultDetail(DetailLevel.Info, "success count:2, failed count:6, warning count:1", DateTime.UtcNow, null);
			var count = result.GetSummaryCount();
			count.SuccessCount.Should().Be(2);
			count.FailedCount.Should().Be(6);
			count.WarningCount.Should().Be(1);
		}
	}
}
