using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
	[TestFixture]
	public class BlockInfoTest
	{
		//private MockRepository _mocks;
		private IBlockInfo _target;

		[SetUp]
		public void Setup()
		{
			//_mocks = new MockRepository();
			_target = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 27)));
		}

		[Test]
		public void ShouldReturnBlockPeriod()
		{
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 27)), _target.BlockPeriod); 
		}

	}
}