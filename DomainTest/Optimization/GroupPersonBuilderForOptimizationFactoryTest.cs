using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class GroupPersonBuilderForOptimizationFactoryTest
	{
		public IGroupPersonBuilderForOptimizationFactory Target;

		[Test]
		public void ShouldReturnTypeSingleAgentIfOptionsSaysSingleAgent()
		{
			var options = new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("xx", GroupPageType.SingleAgent)
			};

			var result = Target.Create(options);

			result.Should().Be.OfType<GroupPersonBuilderForOptimizationAndSingleAgentTeam>();
		}

		[Test]
		public void ShouldReturnOtherTypeIfOptionsSaysNotSingleAgent()
		{
			var options = new SchedulingOptions
			{
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("xx", GroupPageType.Contract)
			};

			var result = Target.Create(options);

			result.Should().Be.OfType<GroupPersonBuilderForOptimization>();
		}
	}
}