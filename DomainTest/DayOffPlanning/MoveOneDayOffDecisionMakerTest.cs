using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class MoveOneDayOffDecisionMakerTest
    {
        private IDayOffDecisionMaker _target;
        private MockRepository _mocks;
        private IDayOffLegalStateValidator _validator;
        private ILogWriter _logWriter;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _validator = _mocks.StrictMock<IDayOffLegalStateValidator>();
            _logWriter = _mocks.StrictMock<ILogWriter>();
            _target = new MoveOneDayOffDecisionMaker(new List<IDayOffLegalStateValidator> { _validator }, _logWriter);
        }

        [Test]
        public void SimpleTestWhenNoLockAllValid()
        {
            IList<double?> values = new List<double?> { 100, 5, 75, 90, -1, -100, -50 };
            ILockableBitArray bitArray = createArray1();
            using (_mocks.Record())
            {
                Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(true).Repeat.Any();
                _logWriter.LogInfo(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();

            }
            bool res;
            using (_mocks.Playback())
            {
                res = _target.Execute(bitArray, values);
            }
            Assert.IsTrue(res);
            Assert.IsTrue(bitArray[0]);
            Assert.IsFalse(bitArray[5]);
        }

        [Test]
        public void SimpleTestWithBestWorkdayLocked()
        {
            IList<double?> values = new List<double?> { 100, 5, 75, 90, -1, -100, -50 };
            ILockableBitArray bitArray = createArray1();
            bitArray.Lock(0, true);
            using (_mocks.Record())
            {
                Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(true).Repeat.Any();
                _logWriter.LogInfo(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            }
            bool res;
            using (_mocks.Playback())
            {
                res = _target.Execute(bitArray, values);
            }
            Assert.IsTrue(res);
            Assert.IsTrue(bitArray[3]);
            Assert.IsFalse(bitArray[5]);
        }

        [Test]
        public void SimpleTestWithBestDayOffLocked()
        {
            IList<double?> values = new List<double?> { 100, 5, 75, 90, -1, -100, -50 };
            ILockableBitArray bitArray = createArray1();
            bitArray.Lock(5, true);
            using (_mocks.Record())
            {
                Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(true).Repeat.Any();
                _logWriter.LogInfo(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            }
            bool res;
            using (_mocks.Playback())
            {
                res = _target.Execute(bitArray, values);
            }
            Assert.IsTrue(res);
            Assert.IsTrue(bitArray[0]);
            Assert.IsFalse(bitArray[6]);
        }

        [Test]
        public void SimpleTestWithEverythingOverstaffed()
        {
            IList<double?> values = new List<double?> { 200, 105, 175, 190, 0, 1, 50 };
            ILockableBitArray bitArray = createArray1();
            using (_mocks.Record())
            {
                Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(true).Repeat.Any();
                _logWriter.LogInfo(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            }
            bool res;
            using (_mocks.Playback())
            {
                res = _target.Execute(bitArray, values);
            }
            Assert.IsTrue(res);
            Assert.IsTrue(bitArray[0]);
            Assert.IsFalse(bitArray[5]);
        }

        [Test]
        public void SimpleTestWithEverythingUnderstaffed()
        {
            IList<double?> values = new List<double?> { -200, -105, -175, -190, -10, -1000, -500 };
            ILockableBitArray bitArray = createArray1();
            using (_mocks.Record())
            {
                Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(true).Repeat.Any();
                _logWriter.LogInfo(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            }
            bool res;
            using (_mocks.Playback())
            {
                res = _target.Execute(bitArray, values);
            }
            Assert.IsTrue(res);
            Assert.IsTrue(bitArray[4]);
            Assert.IsFalse(bitArray[5]);
        }

        [Test]
        public void VerifyNoMoveIfBestDayOffHasHigherValueThanBestWorkday()
        {
            IList<double?> values = new List<double?> { 2, 5, 75, 90, 0, 100, 500 };
            ILockableBitArray bitArray = createArray1();
            using (_mocks.Record())
            {
                Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(true).Repeat.Any();
                _logWriter.LogInfo(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            }
            bool res;
            using (_mocks.Playback())
            {
                res = _target.Execute(bitArray, values);
            }
            Assert.IsFalse(res);
            Assert.IsTrue(bitArray[5]);
            Assert.IsTrue(bitArray[6]);
        }

        [Test]
        public void VerifyNoMoveIfNoValidStateCouldBeFound()
        {
            IList<double?> values = new List<double?> { 100, 5, 75, 90, -1, -100, -50 };
            ILockableBitArray bitArray = createArray1();
            using (_mocks.Record())
            {
                Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(false).Repeat.Any();
                _logWriter.LogInfo(null);
                LastCall.IgnoreArguments().Repeat.AtLeastOnce();
            }
            bool res;
            using (_mocks.Playback())
            {
                res = _target.Execute(bitArray, values);
            }
            Assert.IsFalse(res);
            Assert.IsTrue(bitArray[5]);
            Assert.IsTrue(bitArray[6]);
        }

		[Test]
		public void Bug27312FlatDemands()
		{
			IList<double?> values = new List<double?> { -50, -50, -50, -50, -50, -50, -50 };
			ILockableBitArray bitArray = new LockableBitArray(7, false, false);
			bitArray.PeriodArea = new MinMax<int>(0, 5);
			bitArray.Set(5, true);

			using (_mocks.Record())
			{
				Expect.Call(_validator.IsValid(new BitArray(0), 0)).IgnoreArguments().Return(false).Repeat.Any();
				_logWriter.LogInfo(null);
				LastCall.IgnoreArguments().Repeat.AtLeastOnce();
			}
			bool res;
			using (_mocks.Playback())
			{
				res = _target.Execute(bitArray, values);
			}
			Assert.IsFalse(res);
			Assert.IsTrue(bitArray[5]);
		}

        private static ILockableBitArray createArray1()
        {
            ILockableBitArray ret = new LockableBitArray(7, false, false);
            ret.PeriodArea = new MinMax<int>(0, 5);
            ret.Set(5, true);
            ret.Set(6, true);
            return ret;
        }
    }
}