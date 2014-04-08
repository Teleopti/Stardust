using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.DayOffPlanning;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class WorkShiftLegalStateDayIndexCalculatorTest
    {
        private WorkShiftLegalStateDayIndexCalculator _target;

        [SetUp]
        public void Setup()
        {
            _target = new WorkShiftLegalStateDayIndexCalculator();
        }

        [Test]
        public void VerifyReduceIndex()
        {
            ReadOnlyCollection<double> expected = createExpectedReduceIndexList();
            ReadOnlyCollection<double?> result = _target.CalculateIndexForReducing(createInputValueList());
            for (int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }

        [Test]
        public void VerifyReduceIndexNullInputValues()
        {
            ReadOnlyCollection<double?> expected = createExpectedIndexNullList();
            ReadOnlyCollection<double?> result = _target.CalculateIndexForReducing(createInputValueNullList());
            for (int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }

        [Test]
        public void VerifyRaiseIndex()
        {
            ReadOnlyCollection<double?> expected = createExpectedRaiseIndexList();
            ReadOnlyCollection<double?> result = _target.CalculateIndexForRaising(createInputValueList());
            for (int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }

        [Test]
        public void VerifyRaiseIndexNullInputValues()
        {
            ReadOnlyCollection<double?> expected = createExpectedIndexNullList();
            ReadOnlyCollection<double?> result = _target.CalculateIndexForRaising(createInputValueNullList());
            for (int i = 0; i < result.Count(); i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }

        private static ReadOnlyCollection<double?> createExpectedRaiseIndexList()
        {
            return new ReadOnlyCollection<double?>
                (new List<double?>
                       {
                           101,100,98,99,106,1,111
                       });
        }

        private static ReadOnlyCollection<double> createExpectedReduceIndexList()
        {
            return new ReadOnlyCollection<double>
                (new List<double>
                       {
                           11,12,14,13,6,111,1
                       });
        }

        private static ReadOnlyCollection<double?> createExpectedIndexNullList()
        {
            return new ReadOnlyCollection<double?>
                (new List<double?>
                       {
                           null,null,null,null,null,null,null
                       });
        }

        private static IList<double?> createInputValueNullList()
        {
            return new List<double?>
                       {
                           null,null,null,null,null,null,null
                       };
        }

        private static IList<double?> createInputValueList()
        {
            return new List<double?>
                       {
                           0,1,3,2,-5,100,-10
                       };
        }

    }
}
