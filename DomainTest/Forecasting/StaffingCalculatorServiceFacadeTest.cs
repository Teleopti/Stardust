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
	}
}