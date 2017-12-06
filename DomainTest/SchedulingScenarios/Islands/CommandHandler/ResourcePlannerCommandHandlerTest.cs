using NUnit.Framework;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	[TestFixture(SUT.IntradayOptimization)]
	[TestFixture(SUT.Scheduling)]
	[DomainTest]
	public abstract class ResourcePlannerCommandHandlerTest
	{
	
	}
}