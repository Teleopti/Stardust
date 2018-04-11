using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[DomainTest]
	[TestFixture(true, true)]
	[TestFixture(true, false)]
	[TestFixture(false, false)]
	public class StaffingCalculatorServiceFacadeTest:IConfigureToggleManager
	{
		public IStaffingCalculatorServiceFacade Target;
		private readonly bool _useErlangA;
		private readonly bool _useErlangAWithEsl;

		public StaffingCalculatorServiceFacadeTest(bool useErlangA, bool useErlangAWithEsl)
		{
			_useErlangA = useErlangA;
			_useErlangAWithEsl = useErlangAWithEsl;
		}

		[Test]
		public void UtilizationShouldHandleChatAsWell()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 5d, 550, TimeSpan.FromMinutes(15), .9, .9, 2);
			var calculatedOcc = Target.Utilization(agents, 5d, 550, TimeSpan.FromMinutes(15), 2);
			calculatedOcc.Should().Be.IncludedIn(.89, .91);
		}

		[Test]
		public void BetterEslCalculationOnSmallVolumeChatUseLinearIfTaskDivMaxParallelIsTwoOrLess()
		{
			var tasks = 4d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, agents, 3);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void EslCalculationOnLargerVolymesShouldNotUseLinear()
		{
			var tasks = 40d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, agents, 3);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void ShouldCalculateLinearEslToMatchSlaWhenPerfectlyStaffed()
		{
			var tasks = 6d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = Target.LinearEsl(agents, agents, .8);
			esl.Should().Be.IncludedIn(.79, .81);
		}

		[Test]
		public void LinearEslCanNotBeNegative()
		{
			var tasks = 6d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
			var esl = Target.LinearEsl(agents, 0, .8);
			esl.Should().Be.EqualTo(0);
		}

		[Test]
		public void LinearEslCanNotBeMoreThan100Percent()
		{
			var tasks = 6d;
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, 550, TimeSpan.FromMinutes(15), 0, 1, 3);
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
			var agents = Target.AgentsUseOccupancy(.8, 20, tasks, aht, TimeSpan.FromMinutes(15), 0, 1, 1);
			agents.Should().Not.Be.EqualTo(double.NaN);
		}

		[Test]
		public void ErlangCErlangAComparison()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 37, 320, TimeSpan.FromMinutes(15), 0.3, 0.9, 1);
			Math.Round(agents,2).Should().Be.EqualTo(16.87);
		}
		[Test]
		public void ErlangCErlangAComparison1()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 38, 320, TimeSpan.FromMinutes(15), 0.3, 0.9, 1);
			Math.Round(agents, 1).Should().Be.EqualTo(17.3);
		}

		[Test]
		public void ErlangCErlangAComparison2()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 40, 3.75, TimeSpan.FromMinutes(15), 0.3, 0.9, 1);
			Math.Round(agents, 2).Should().Be.EqualTo(0.56);
		}

		[Test]
		public void ErlangCErlangAComparison3()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 17, 320, TimeSpan.FromMinutes(30), 0.3, 0.9, 1);
			Math.Round(agents, 1).Should().Be.EqualTo(5.1);
		}

		[Test]
		public void ErlangCErlangAComparison4()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 8, 320, TimeSpan.FromMinutes(30), 0.3, 0.9, 1);
			Math.Round(agents, 2).Should().Be.EqualTo(2.97);
		}

		[Test]
		public void ErlangCErlangAComparison5()
		{
			var agents = Target.AgentsUseOccupancy(.8, 20, 32, 10, TimeSpan.FromMinutes(30), 0.3, 0.9, 1);
			Math.Round(agents, 2).Should().Be.EqualTo(0.59);
		}

		[Test]
		[Ignore("#74899 - maybe to be fixed")]
		public void ErlangCErlangAComparison6()
		{
			const double tasks = 40d;
			const double agents = 9d;
			const double forecasted = 10d;
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, forecasted, 3);

			esl.Should().Be.IncludedIn(.63, .65);
		}

		[Test]
		[Ignore("#74899 - maybe to be fixed")]
		public void ErlangCErlangAComparison7()
		{
			const double tasks = 40d;
			const double agents = 6d;
			const double forecasted = 10d;
			var esl = Target.ServiceLevelAchievedOcc(agents, 20, tasks, 550, TimeSpan.FromMinutes(15), .8, forecasted, 3);

			esl.Should().Be.EqualTo(0d);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_useErlangA)
			{
				if (_useErlangAWithEsl)
				{
					toggleManager.Enable(Toggles.ResourcePlanner_UseErlangAWithInfinitePatienceEsl_74899);
				}
				toggleManager.Enable(Toggles.ResourcePlanner_UseErlangAWithInfinitePatience_45845);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_UseErlangAWithInfinitePatience_45845);
			}
		}
	}
}