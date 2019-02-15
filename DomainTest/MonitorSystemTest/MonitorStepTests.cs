using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
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
	}
}