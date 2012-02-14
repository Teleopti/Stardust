using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class WorkShiftLegalStatePeriodDecisionMakerTest
    {
        private WorkShiftLegalStateWeekDecisionMaker _target;
        private MockRepository _mockRepository;
        private IWorkShiftLegalStateDayIndexCalculator _dayIndexCalculator;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _dayIndexCalculator = _mockRepository.StrictMock<IWorkShiftLegalStateDayIndexCalculator>();
            _target = new WorkShiftLegalStateWeekDecisionMaker(_dayIndexCalculator);
        }

        [Test]
        public void VerifyReturnsTheRightWeekday()
        {
            IList<double?> values = new List<double?>(35);
            ILockableBitArray bitArray = CreateBitArrayForTest();

            using(_mockRepository.Record())
            {
                Expect.Call(_dayIndexCalculator.CalculateIndexForReducing(values))
                    .Return(new ReadOnlyCollection<double?>(CreateNormalizedIndexes()));
            }
            using(_mockRepository.Playback())
            {
                int? result = _target.Execute(bitArray, values);
                Assert.AreEqual(10, result.Value);
            }
        }

        private static IList<double?> CreateNormalizedIndexes()
        {
            return new List<double?>
                { 
                    0,0,0,0,0,0,0,
                    26,27,30,51,36,41,31,
                    0,0,0,0,0,0,0,
                    0,0,0,0,0,0,0,
                    0,0,0,0,0,0,0
                };
        }


        private static ILockableBitArray CreateBitArrayForTest()
        {
            ILockableBitArray ret = new LockableBitArray(35, false, false, null);
            ret.PeriodArea = new MinMax<int>(0, 35);
            ret.Lock(7, false);
            ret.Lock(8, false);
            ret.Lock(9, false);
            ret.Lock(10, false);
            ret.Lock(11, false);
            ret.Lock(12, false);
            ret.Lock(13, false);
            return ret;
        }
    }
}
