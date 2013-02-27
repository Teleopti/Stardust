using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class BlockProviderTest
	{
		private BlockProvider _target;
		private MockRepository _mocks;
		private IDynamicBlockFinder _dynamicBlockFinder;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			var schedulingOptions = new SchedulingOptions();
			_dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
			_target = new BlockProvider(schedulingOptions, _dynamicBlockFinder, _groupPersonBuilderForOptimization);
		}

		[Test]
		public void ShouldProvideBlocks()
		{
			
		}
	}
}
