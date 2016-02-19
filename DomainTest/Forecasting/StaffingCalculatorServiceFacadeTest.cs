using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class StaffingCalculatorServiceFacadeTest
	{
		private StaffingCalculatorServiceFacade _target;

		[SetUp]
		public void Setup()
		{
			_target = new StaffingCalculatorServiceFacade();
		}

		[Test]
		public void UtilizationShouldHandleChatAsWell()
		{
			var agents = _target.AgentsUseOccupancy(.8, 20, 5d, 550, TimeSpan.FromMinutes(15), .9, .9, 2);
			var calculatedOcc = _target.Utilization(agents, 5d, 550, TimeSpan.FromMinutes(15), 2);
			calculatedOcc.Should().Be.IncludedIn(.89, .91);
		}
	}
}