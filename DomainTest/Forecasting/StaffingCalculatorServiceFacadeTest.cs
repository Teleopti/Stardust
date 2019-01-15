using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[DomainTest]
	
	public class StaffingCalculatorServiceFacadeTest
	{
		public IStaffingCalculatorServiceFacade Target;

		[Test]
		public void UtilizationShouldHandleChatAsWell()
		{
			var agentsAndOccupancy = Target.AgentsUseOccupancy(.8, 20, 5d, 550, TimeSpan.FromMinutes(15), .9, .9, 2,0);
			var calculatedOcc = Target.Utilization(agentsAndOccupancy.Agents, 5d, 550, TimeSpan.FromMinutes(15), 2, agentsAndOccupancy.Occupancy);
			calculatedOcc.Should().Be.IncludedIn(.89, .91);
		}

		[Test]
		public void BetterEslCalculationOnSmallVolumeChatUseLinearIfTaskDivMaxParallelIsTwoOrLess()
		{
			var tasks = 4d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3,0).Agents;
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, agents, 3, 0);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void EslCalculationOnLargerVolymesShouldNotUseLinear()
		{
			var tasks = 40d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3,0).Agents;
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, agents, 3, 0);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void ShouldCalculateLinearEslToMatchSlaWhenPerfectlyStaffed()
		{
			var tasks = 6d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3,0).Agents;
			var esl = Target.LinearEsl(agents, agents, .8);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void LinearEslCanNotBeNegative()
		{
			var tasks = 6d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3,0).Agents;
			var esl = Target.LinearEsl(agents, 0, .8);
			esl.Should().Be.EqualTo(0);
		}

		[Test]
		public void LinearEslCanNotBeMoreThan100Percent()
		{
			var tasks = 6d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3,0).Agents;
			var esl = Target.LinearEsl(agents, agents * 10, .8d);
			esl.Should().Be.EqualTo(1);
		}

		[Test]
		public void LinearEslShouldHandleZeroForecast()
		{
			var esl = Target.LinearEsl(0d, 0d, .8);
			esl.Should().Be.EqualTo(0);
		}

		[Test]
		public void TestMultipleCombinationsForCallsAndAht([Values(0.1,1,10,100,1000,10000)] double tasks, [Values(0.1,1,10,100,1000,10000)] double aht)

		{
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, aht, TimeSpan.FromMinutes(15), 0, 1, 1,0);
			agents.Should().Not.Be.EqualTo(double.NaN);
		}

		[Test]
		public void ErlangCErlangAComparison()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 37, 320, TimeSpan.FromMinutes(15), 0.3, 0.9, 1,0).Agents;
			Math.Round(agents,2).Should().Be.EqualTo(16.87);
		}
		[Test]
		public void ErlangCErlangAComparison1()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 38, 320, TimeSpan.FromMinutes(15), 0.3, 0.9, 1,0).Agents;
			Math.Round(agents, 1).Should().Be.EqualTo(17.3);
		}

		[Test]
		public void ErlangCErlangAComparison2()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 40, 3.75, TimeSpan.FromMinutes(15), 0.3, 0.9, 1,0).Agents;
			Math.Round(agents, 2).Should().Be.EqualTo(0.56);
		}

		[Test]
		public void ErlangCErlangAComparison3()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 17, 320, TimeSpan.FromMinutes(30), 0.3, 0.9, 1,0).Agents;
			Math.Round(agents, 1).Should().Be.EqualTo(5.1);
		}

		[Test]
		public void ErlangCErlangAComparison4()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 8, 320, TimeSpan.FromMinutes(30), 0.3, 0.9, 1,0).Agents;
			Math.Round(agents, 2).Should().Be.EqualTo(2.97);
		}

		[Test]
		public void ErlangCErlangAComparison5()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 32, 10, TimeSpan.FromMinutes(30), 0.3, 0.9, 1,0).Agents;
			Math.Round(agents, 2).Should().Be.EqualTo(0.59);
		}

		[Test]
		public void ErlangCErlangAComparison6()
		{
			const double tasks = 40d;
			const double agents = 9d;
			const double forecasted = 10d;
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, forecasted, 3, 0);

			esl.Should().Be.IncludedIn(.63, .65);
		}

		[Test]
		public void ErlangCErlangAComparison7()
		{
			const double tasks = 40d;
			const double agents = 6d;
			const double forecasted = 10d;
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, forecasted, 3, 0);

			esl.Should().Be.LessThan(0.000001);
		}

		[Test]
		public void ErlangCErlangAComparison8()
		{
			const double tasks = 40d;
			const double agents = 7.5d;
			const double forecasted = 10d;
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, forecasted, 3, 0);

			esl.Should().Be.IncludedIn(.18, .23);
		}

		[Test]
		public void ErlangCErlangAComparison9()
		{
			const double tasks = 6d;
			const double agents = 2d;
			var forecasted = Target.AgentsUseOccupancy(.8, 20, tasks, 850, TimeSpan.FromMinutes(15), 0, 1, 3,0).Agents;	
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 850, TimeSpan.FromMinutes(15), .8, forecasted, 3, 0);

			esl.Should().Be.IncludedIn(.80, .82);
		}

		[Test]
		public void ErlangCErlangAComparison10()
		{
			double callsPerInterval = 50;
			double averageHandelingTimeSeconds = 300;
			int serviceLevelSeconds = 20;
			const int intervalLengthInSeconds = 3600;

			const double targetServiceLevelPercentage = 0.868;
			const double maximumOccupancy = 1;
			const double minimumOccupancy = 0;

			var agents = Target.AgentsUseOccupancy(targetServiceLevelPercentage, serviceLevelSeconds, callsPerInterval,
				averageHandelingTimeSeconds, TimeSpan.FromSeconds(intervalLengthInSeconds), minimumOccupancy, maximumOccupancy, 1,0).Agents;

			Assert.AreEqual(Math.Round(agents, 2), 7.00);
		}

		[Test]
		public void ShouldNotLoopForever()
		{
			double callsPerInterval = 341;
			double averageHandelingTimeSeconds = 7920;
			int serviceLevelSeconds = 30;
			const int intervalLengthInSeconds = 1800;
			var agents = 15;
			var forecastedAgents = 1667.1111001273;

			const double targetServiceLevelPercentage = 0.6;

			Target.ServiceLevelAchievedOcc(agents, serviceLevelSeconds, callsPerInterval, averageHandelingTimeSeconds, TimeSpan.FromSeconds(intervalLengthInSeconds), targetServiceLevelPercentage,
				forecastedAgents, 1, 0);
		}
	}
}