using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class GroupPersonBuilderWrapperTest
	{
		public IGroupPersonBuilderWrapper Target;

		[Test]
		public void ShouldReturnGroupPersonBuilderForOptimization()
		{
			var builder = Target.ForOptimization();
			builder.Should().Be.OfType<GroupPersonBuilderForOptimization>();
		}

		[Test]
		public void ShouldReturnGroupPersonBuilderForOptimizationAndSingleAgentTeam()
		{
			Target.SetSingleAgentTeam();
			var builder = Target.ForOptimization();
			builder.Should().Be.OfType<GroupPersonBuilderForOptimizationAndSingleAgentTeam>();
		}

		[Test]
		public void ShouldReset()
		{
			Target.SetSingleAgentTeam();
			var builder = Target.ForOptimization();
			builder.Should().Be.OfType<GroupPersonBuilderForOptimizationAndSingleAgentTeam>();
	
			Target.Reset();
			builder = Target.ForOptimization();
			builder.Should().Be.OfType<GroupPersonBuilderForOptimization>();
		}
	}
}
