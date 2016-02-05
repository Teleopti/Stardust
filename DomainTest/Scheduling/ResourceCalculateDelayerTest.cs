using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
	public class ResourceCalculateDelayerTest
	{
		private IResourceCalculateDelayer _target;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
		}

		[Test]
		public void ShouldNotCalculateIfSameDateAndCounterUnderLimit()
		{
			_target = new ResourceCalculateDelayer(_resourceOptimizationHelper, 3, true);
			using(_mocks.Record())
			{
				
			}

			using(_mocks.Playback())
			{
                Assert.IsFalse(_target.CalculateIfNeeded(new DateOnly(), new DateTimePeriod(), false));
				Assert.IsFalse(_target.CalculateIfNeeded(new DateOnly(), new DateTimePeriod(), false));
			}
		}

		[Test]
		public void ShouldCalculateLastDateAndNextDateIfDifferentDateAndCounterUnderLimit()
		{
			_target = new ResourceCalculateDelayer(_resourceOptimizationHelper, 3, true);
			using (_mocks.Record())
			{
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(), true, false));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly().AddDays(1), true, false));
			}

			using (_mocks.Playback())
			{
                Assert.IsFalse(_target.CalculateIfNeeded(new DateOnly(), new DateTimePeriod(), false));
                Assert.IsTrue(_target.CalculateIfNeeded(new DateOnly().AddDays(7), new DateTimePeriod(), false));
			}
		}

		[Test]
		public void ShouldCalculateThisDateAndNextIfLimitReachedAndAbove1()
		{
			_target = new ResourceCalculateDelayer(_resourceOptimizationHelper, 2, true);
			using (_mocks.Record())
			{
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(), true, false));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly().AddDays(1), true, false));
			}

			using (_mocks.Playback())
			{
                Assert.IsFalse(_target.CalculateIfNeeded(new DateOnly(), new DateTimePeriod(), false));
                Assert.IsTrue(_target.CalculateIfNeeded(new DateOnly(), new DateTimePeriod(), false));
			}
		}

		[Test]
		public void ShouldCalculateThisDateAndNextIfLimitIs1AndPeriodOverMidnight()
		{
			DateOnlyPeriod dop = new DateOnlyPeriod(new DateOnly(), new DateOnly().AddDays(1));
			var dp = dop.ToDateTimePeriod((TimeZoneInfo.Utc));
			_target = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true);
			using (_mocks.Record())
			{
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(), true, false));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly().AddDays(1), true, false));
			}

			using (_mocks.Playback())
			{
                Assert.IsTrue(_target.CalculateIfNeeded(new DateOnly(), dp, false));
			}
		}

		[Test]
		public void ShouldCalculateThisDateIfLimitIs1AndPeriodNotOverMidnight()
		{
			DateOnlyPeriod dop = new DateOnlyPeriod(new DateOnly(), new DateOnly());
			var dp = dop.ToDateTimePeriod((TimeZoneInfo.Utc));
			dp = dp.ChangeEndTime(TimeSpan.FromSeconds(-1));
			_target = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true);
			using (_mocks.Record())
			{
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(), true, false));
			}

			using (_mocks.Playback())
			{
                Assert.IsTrue(_target.CalculateIfNeeded(new DateOnly(), dp, false));
			}
		}
	}
}