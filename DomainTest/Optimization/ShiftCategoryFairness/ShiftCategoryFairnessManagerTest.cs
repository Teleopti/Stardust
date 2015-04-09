using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
    [TestFixture]
    public class ShiftCategoryFairnessManagerTest
    {
        private MockRepository _mocks;
        private IScheduleDictionary _dic;
        private IGroupShiftCategoryFairnessCreator _groupCreator;
        private IShiftCategoryFairnessCalculator _fairnessCalc;
        private Person _person;
        private ShiftCategoryFairnessManager _target;
        private ISchedulingResultStateHolder _stateHolder;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _dic = _mocks.StrictMock<IScheduleDictionary>();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _groupCreator = _mocks.StrictMock<IGroupShiftCategoryFairnessCreator>();
            _fairnessCalc = _mocks.StrictMock<IShiftCategoryFairnessCalculator>();
            _target = new ShiftCategoryFairnessManager(()=>_stateHolder, _groupCreator, _fairnessCalc);
            _person = new Person();

        }

        [Test]
        public void ShouldCombineFromDictionaryAndCalculators()
        {
			var range = _mocks.StrictMock<IScheduleRange>();
			var dateOnly = new DateOnly(2011, 4, 19);
			var fairness = new ShiftCategoryFairnessHolder();
			var factors = _mocks.StrictMock<IShiftCategoryFairnessFactors>();
			Expect.Call(_stateHolder.Schedules).Return(_dic);
			Expect.Call(_dic[_person]).Return(range);
			Expect.Call(range.CachedShiftCategoryFairness()).Return(fairness);
			Expect.Call(_groupCreator.CalculateGroupShiftCategoryFairness(_person, dateOnly)).Return(fairness);
			Expect.Call(_fairnessCalc.ShiftCategoryFairnessFactors(fairness, fairness)).Return(factors);
			_mocks.ReplayAll();
			_target.GetFactorsForPersonOnDate(_person, dateOnly);
			_mocks.VerifyAll();
        }
    }

    
}