using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class EqualWorkShiftValueDeciderTest
	{

		private MockRepository _mocks;
		private IEqualWorkShiftValueDecider _target;
		private IShiftProjectionCache _cache1;
		private IShiftProjectionCache _cache2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new EqualWorkShiftValueDecider();
			_cache1 = _mocks.StrictMock<IShiftProjectionCache>();
			_cache2 = _mocks.StrictMock<IShiftProjectionCache>();

		}

		[Test]
		public void ShortBreaksShouldBeEquallyDistributed()
		{

			using (_mocks.Record())
			{

			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.Decide(_cache1, _cache2);
			}
      
			Assert.AreSame(_cache1, result);
		}

	}
}