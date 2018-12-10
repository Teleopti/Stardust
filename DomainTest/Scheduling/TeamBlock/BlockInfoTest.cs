using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
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

		[Test]
		public void ShouldHoldStandardDeviationValues()
		{
			var values = new List<double?> {0.1, 0.2, null, 0.3};
			_target.StandardDeviations = values;

			Assert.That(_target.StandardDeviations, Is.EqualTo(values));
		}

        [Test]
        public void ShouldReturnSameHashIfBlockPeriodAreTheSame()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 27)));
            Assert.AreEqual(blockInfo.GetHashCode(), _target.GetHashCode());
        }

        [Test]
        public void ShouldBeEqualWhenBlockPeriodAreTheSame()
        {
            var blockInfo = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 27)));
            Assert.AreEqual(blockInfo, _target);
        }

		[Test]
		public void ShouldCheckIfEqualsNull()
		{
			IBlockInfo blockInfo = null;
			Assert.IsFalse(_target.Equals(blockInfo));
		}

		[Test]
		public void AllBlockDaysShouldBeUnlockedToStartWith()
		{
			_target = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 28)));
			Assert.AreEqual(2, _target.UnLockedDates().Count);
		}

		[Test]
		public void DatesShouldBeLockable()
		{
			_target = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 28)));
			_target.LockDate(new DateOnly(2013, 2, 28));
			Assert.AreEqual(1, _target.UnLockedDates().Count);
			Assert.AreEqual(new DateOnly(2013, 2, 27), _target.UnLockedDates()[0]);
		}

		[Test]
		public void TryingToLocDateOutsideBlockShouldThrow()
		{
			_target = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 28)));
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.LockDate(new DateOnly(2013, 2, 29)));
		}

		[Test]
		public void LockShouldBeCleared()
		{
			_target = new BlockInfo(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 28)));
			_target.LockDate(new DateOnly(2013, 2, 28));
			Assert.AreEqual(1, _target.UnLockedDates().Count);

			_target.ClearLocks();
			Assert.AreEqual(2, _target.UnLockedDates().Count);
		}
	}
}