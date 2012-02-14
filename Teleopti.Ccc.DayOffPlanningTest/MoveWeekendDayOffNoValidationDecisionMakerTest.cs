using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class MoveWeekendDayOffNoValidationDecisionMakerTest
    {
        private MoveWeekendDayOffNoValidationDecisionMaker _target;
        private IOfficialWeekendDays _officialWeekendDays;
        private MockRepository _mockRepository;
        private ILogWriter _logWriter;


        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _officialWeekendDays = new OfficialWeekendDays(new CultureInfo("se-SE"));
            _logWriter = _mockRepository.StrictMock<ILogWriter>();
            _target = new MoveWeekendDayOffNoValidationDecisionMaker(_officialWeekendDays, _logWriter);
        }

        [Test]
        public void SimpleTestWhenNoLock()
        {
            IList<double?> values = createValueList();
            ILockableBitArray bitArray = createBitArrayForTest();
            string startArray = BitArrayHelper.ToWeeklySeparatedString(bitArray.ToLongBitArray());
            Assert.AreEqual(startArray, "0000000-1100000-0000000-0010000-0000011-0000011-0000000");

            bool res = _target.Execute(bitArray, values);
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

            bool res = _target.Execute(bitArray, values);

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
            ILockableBitArray ret = new LockableBitArray(35, false, false, null);
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