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
        public void VerifyPersonFairness()
        {
            IPerson person = PersonFactory.CreatePerson();
            IFairnessValueResult totalValue = new FairnessValueResult {TotalNumberOfShifts = 10, FairnessPoints = 15};
            IFairnessValueResult personValue = new FairnessValueResult {TotalNumberOfShifts = 10, FairnessPoints = 30};

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
                Assert.AreEqual(2.0d, ret);
            }  
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

        [Test]
        public void VerifyScheduleFairness()
        {
            IPerson person1 = PersonFactory.CreatePerson();
            IPerson person2 = PersonFactory.CreatePerson();
            IList<IPerson> personColl = new List<IPerson>{person1, person2};
            IFairnessValueResult totalValue = new FairnessValueResult {TotalNumberOfShifts = 10, FairnessPoints = 15};
            IFairnessValueResult personValue1 = new FairnessValueResult
                                                    {
                                                        TotalNumberOfShifts = 10,
                                                        FairnessPoints = 30
                                                    };
            IFairnessValueResult personValue2 = new FairnessValueResult
                                                    {
                                                        TotalNumberOfShifts = 20,
                                                        FairnessPoints = 30
                                                    };

            using (_mocks.Record())
            {
                Expect.Call(_resultStateHolder.Schedules).Return(_dic).Repeat.Times(3);
                Expect.Call(_dic.FairnessPoints()).Return(totalValue);
                Expect.Call(_dic.Keys).Return(personColl);
                Expect.Call(_dic[person1]).Return(_range);
                Expect.Call(_range.FairnessPoints()).Return(personValue1);
                Expect.Call(_dic[person2]).Return(_range);
                Expect.Call(_range.FairnessPoints()).Return(personValue2);
            }

            using (_mocks.Playback())
            {
                double ret = _target.ScheduleFairness();
                Assert.AreEqual(0.5d, ret);
            }
        }
    }
}