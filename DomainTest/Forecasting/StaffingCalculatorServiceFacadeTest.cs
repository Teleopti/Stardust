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

		[Test]
		public void BetterEslCalculationOnSmallVolumeChatUseLinearIfTaskDivMaxParallelIsTwoOrLess()
		{
			var tasks = 4d;
			var agents = _target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = _target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, agents, 3);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void EslCalculationOnLargerVolymesShouldNotUseLinear()
		{
			var tasks = 40d;
			var agents = _target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = _target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, agents, 3);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void ShouldCalculateLinearEslToMatchSlaWhenPerfectlyStaffed()
		{
			var tasks = 6d;
			var agents = _target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = _target.LinearEsl(agents, agents, .8);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void LinearEslCanNotBeNegative()
		{
			var tasks = 6d;
			var agents = _target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = _target.LinearEsl(agents, 0, .8);
			esl.Should().Be.EqualTo(0);
		}

		[Test]
		public void LinearEslCanNotBeMoreThan100Percent()
		{
			var tasks = 6d;
			var agents = _target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = _target.LinearEsl(agents, agents*10, .8);
			esl.Should().Be.EqualTo(1);
		}

		[Test]
		public void LinearEslShouldHandleZeroForecast()
		{
			var esl = _target.LinearEsl(0, 0, .8);
			esl.Should().Be.EqualTo(0);
		}

		[Test]
		public void TestMultipleCombinationsForCallsAndAht([Values(0.1,1,10,100,1000,10000)] double tasks, [Values(0.1,1,10,100,1000,10000)] double aht)

		{
			var agents = _target.AgentsUseOccupancy(.8, 20, tasks, aht, TimeSpan.FromMinutes(15), 0, 1, 1);
			agents.Should().Not.Be.EqualTo(double.NaN);
		}

		[Test]
		public void ErlangCErlangAComparison()
		{
			var agents = _target.AgentsUseOccupancy(.8, 20, 37, 320, TimeSpan.FromMinutes(15), 0.3, 0.9, 1);
			Math.Round(agents,2).Should().Be.EqualTo(16.87);
		}
		[Test]
		public void ErlangCErlangAComparison1()
		{
			var agents = _target.AgentsUseOccupancy(.8, 20, 38, 320, TimeSpan.FromMinutes(15), 0.3, 0.9, 1);
			Math.Round(agents, 1).Should().Be.EqualTo(17.3);
		}

		[Test]
		public void ErlangCErlangAComparison2()
		{
			var agents = _target.AgentsUseOccupancy(.8, 20, 40, 3.75, TimeSpan.FromMinutes(15), 0.3, 0.9, 1);
			Math.Round(agents, 2).Should().Be.EqualTo(0.56);
		}

		[Test]
		public void ErlangCErlangAComparison3()
		{
			var agents = _target.AgentsUseOccupancy(.8, 20, 17, 320, TimeSpan.FromMinutes(30), 0.3, 0.9, 1);
			Math.Round(agents, 1).Should().Be.EqualTo(5.1);
		}

		[Test]
		public void ErlangCErlangAComparison4()
		{
			var agents = _target.AgentsUseOccupancy(.8, 20, 8, 320, TimeSpan.FromMinutes(30), 0.3, 0.9, 1);
			Math.Round(agents, 2).Should().Be.EqualTo(2.97);
		}

		[Test]
		public void ErlangCErlangAComparison5()
		{
			var agents = _target.AgentsUseOccupancy(.8, 20, 32, 10, TimeSpan.FromMinutes(30), 0.3, 0.9, 1);
			Math.Round(agents, 2).Should().Be.EqualTo(0.59);
		}
	}
}