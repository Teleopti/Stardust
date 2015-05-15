using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ScheduleFairnessCalculatorTest
    {
        private IScheduleFairnessCalculator _target;
        private IScheduleDictionary _dic;
        private IScheduleRange _range;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _resultStateHolder;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _resultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _dic = _mocks.StrictMock<IScheduleDictionary>();
            _range = _mocks.StrictMock<IScheduleRange>();
            _target = new ScheduleFairnessCalculator(()=>_resultStateHolder);
        }

        [Test]
        public void VerifyPersonFairnessWhenPersonHasNoShift()
        {
            IPerson person = PersonFactory.CreatePerson();
            IFairnessValueResult totalValue = new FairnessValueResult {TotalNumberOfShifts = 10, FairnessPoints = 15};
            IFairnessValueResult personValue = new FairnessValueResult();

            using (_mocks.Record())
            {
                Expect.Call(_resultStateHolder.Schedules).Return(_dic).Repeat.Twice();
                Expect.Call(_dic.FairnessPoints()).Return(totalValue);
                Expect.Call(_dic[person]).Return(_range);
                Expect.Call(_range.FairnessPoints()).Return(personValue);
            }

            using (_mocks.Playback())
            {
                double ret = _target.PersonFairness(person);
                Assert.AreEqual(1.0d, ret);
            }
        }

        [Test]
        public void VerifyPersonFairnessWhenDictionaryHasNoShift()
        {
            IPerson person = PersonFactory.CreatePerson();
            IFairnessValueResult totalValue = new FairnessValueResult();
            IFairnessValueResult personValue = new FairnessValueResult();

            using (_mocks.Record())
            {
                Expect.Call(_resultStateHolder.Schedules).Return(_dic).Repeat.Twice();
                Expect.Call(_dic.FairnessPoints()).Return(totalValue);
                Expect.Call(_dic[person]).Return(_range);
                Expect.Call(_range.FairnessPoints()).Return(personValue);
            }

            using (_mocks.Playback())
            {
                double ret = _target.PersonFairness(person);
                Assert.AreEqual(1.0d, ret);
            }
        }
    }
}