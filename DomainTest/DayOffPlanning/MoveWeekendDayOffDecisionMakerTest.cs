using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class MoveWeekendDayOffDecisionMakerTest
    {
        private MoveWeekendDayOffDecisionMaker _target;
        private MockRepository _mocks;
        private IDayOffLegalStateValidator _validator;
        private IOfficialWeekendDays _officialWeekendDays;
        private ILogWriter _logWriter;


    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _validator = _mocks.StrictMock<IDayOffLegalStateValidator>();
            _officialWeekendDays = new OfficialWeekendDays();
            _logWriter = _mocks.StrictMock<ILogWriter>();
            _target = new MoveWeekendDayOffDecisionMaker(new List<IDayOffLegalStateValidator> { _validator }, _officialWeekendDays, true, _logWriter);
        }

        [Test]
        public void SimpleTestWhenNoLockAllValid()
        {
            IList<double?> values = createValueList();
            ILockableBitArray bitArray = createBitArrayForTest();
            string startArray = BitArrayHelper.ToWeeklySeparatedString(bitArray.ToLongBitArray());
            Assert.AreEqual(startArray, "0000000-1100000-0000000-0010000-0000011-0000011-0000000");

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
            string resultArray = BitArrayHelper.ToWeeklySeparatedString(bitArray.ToLongBitArray());
            Assert.AreEqual(resultArray, "0000000-1100000-0000000-0010011-0000000-0000011-0000000");
        }

        [Test]
        public void SimpleTestWithBestWeekendLocked()
        {
            IList<double?> values = createValueList();
            ILockableBitArray bitArray = createBitArrayForTest();
            string startArray = BitArrayHelper.ToWeeklySeparatedString(bitArray.ToLongBitArray());
            Assert.AreEqual(startArray, "0000000-1100000-0000000-0010000-0000011-0000011-0000000");

            bitArray.Lock(26, true);
            bitArray.Lock(27, true);

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
            string resultArray = BitArrayHelper.ToWeeklySeparatedString(bitArray.ToLongBitArray());
            Assert.AreEqual(resultArray, "0000000-1100000-0000000-0010011-0000011-0000000-0000000");
        }

        private static IList<double?> createValueList()
        {
            return new List<double?>
                       {
                           0,0,0,0,0,10,10,  
                           0,0,0,0,0,5,15,
                           0,0,0,0,0,25,30,
                           0,0,0,0,0,-25,-30,
                           0,0,0,0,0,10,20
                       };
        }

        private static ILockableBitArray createBitArrayForTest()
        {
            ILockableBitArray ret = new LockableBitArray(35, false, false);
            ret.PeriodArea = new MinMax<int>(0, 35);
            ret.Set(0, true);
            ret.Set(1, true);
            ret.Set(16, true);
            ret.Set(26, true);
            ret.Set(27, true);
            ret.Set(33, true);
            ret.Set(34, true);
            return ret;
        }

    }
}