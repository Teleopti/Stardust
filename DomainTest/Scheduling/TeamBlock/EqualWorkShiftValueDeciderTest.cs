using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class EqualWorkShiftValueDeciderTest
	{
		private MockRepository _mocks;
		private IEqualWorkShiftValueDecider _target;
		private ShiftProjectionCache _cache1;
		private ShiftProjectionCache _cache2;
		private ITrueFalseRandomizer _randomizer;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_randomizer = _mocks.StrictMock<ITrueFalseRandomizer>();
			_target = new EqualWorkShiftValueDecider(_randomizer);
			_cache1 = new ShiftProjectionCache(new WorkShift(new ShiftCategory("test1")), new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			_cache2 = new ShiftProjectionCache(new WorkShift(new ShiftCategory("test2")), new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));

		}

		[Test]
		public void ShortBreaksShouldBeEquallyDistributed()
		{

			using (_mocks.Record())
			{
				Expect.Call(_randomizer.Randomize()).IgnoreArguments().Return(true);
			}

			ShiftProjectionCache result;

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
				Expect.Call(_randomizer.Randomize()).IgnoreArguments().Return(false);
			}

			ShiftProjectionCache result;

			using (_mocks.Playback())
			{
				result = _target.Decide(_cache1, _cache2);
			}
      
			Assert.AreSame(_cache2, result);
		}

	}
}