using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MonitorSystem
{
	[DomainTest]
	public class MonitorStepTests
	{
		public IEnumerable<IMonitorStep> MonitorSteps;
		
		[Test]
		public void ShouldHaveRealMonitorStepsRegistered()
		{
			MonitorSteps.Select(x => x.GetType()).Should().Contain(typeof(CheckLegacySystemStatus));
		}
		
		[Test]
		public void ShouldHaveNoDuplicateStepNames()
		{
			MonitorSteps.Select(x => x.Name).GroupBy(n => n).Any(c => c.Count() > 1)
				.Should().Be.False();
		}
		
		[Test]
		public void ShouldHaveSetNameOnAllRegisteredMonitorSteps()
		{
			MonitorSteps.Select(x => x.Name).Should().Not.Contain(null);
		}
	}
}