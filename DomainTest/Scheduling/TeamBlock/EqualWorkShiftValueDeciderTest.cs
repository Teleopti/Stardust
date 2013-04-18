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
		private ITrueFalseRandomizer _randomizer;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_randomizer = _mocks.StrictMock<ITrueFalseRandomizer>();
			_target = new EqualWorkShiftValueDecider(_randomizer);
			_cache1 = _mocks.StrictMock<IShiftProjectionCache>();
			_cache2 = _mocks.StrictMock<IShiftProjectionCache>();

		}

		[Test]
		public void ShortBreaksShouldBeEquallyDistributed()
		{

			using (_mocks.Record())
			{
				Expect.Call(_randomizer.Randomize(0)).IgnoreArguments().Return(true);
			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.Decide(_cache1, _cache2);
			}
      
			Assert.AreSame(_cache1, result);
		}

		[Test]
		public void ShortBreaksShouldBeEquallyDistributed1()
		{

			using (_mocks.Record())
			{
				Expect.Call(_randomizer.Randomize(0)).IgnoreArguments().Return(false);
			}

			IShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.Decide(_cache1, _cache2);
			}
      
			Assert.AreSame(_cache2, result);
		}

	}
}