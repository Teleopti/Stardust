using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class ShiftCategoryFairnessCalculatorTest
    {
        private ShiftCategoryFairnessCalculator _target;
        private ShiftCategoryFairness _groupCategoryFairness;
        private ShiftCategoryFairness _personCategoryFairness;
        private ShiftCategory _m;
        private ShiftCategory _d;
        private ShiftCategory _l;
        private ShiftCategory _n;
        private IScheduleRange _range;
        private IPerson _person;
        private DateOnly _dateOnly;
        private IGroupShiftCategoryFairnessCreator _groupShiftCategoryFairnessCreator;
        private MockRepository _mocks;


        [SetUp]
        public void Setup()
        {
            _m = new ShiftCategory("M");
            _d = new ShiftCategory("D");
            _l = new ShiftCategory("L");
            _n = new ShiftCategory("N");
            _groupCategoryFairness = CreateGroupCategoryFairness();
            _personCategoryFairness = CreatePersonCategoryFairness();
            _mocks = new MockRepository();
            _groupShiftCategoryFairnessCreator = _mocks.DynamicMock<IGroupShiftCategoryFairnessCreator>();
            _range = _mocks.DynamicMock<IScheduleRange>();
            _person = _mocks.DynamicMock<IPerson>();
            _dateOnly = new DateOnly(2012,08,08);
            _target = new ShiftCategoryFairnessCalculator(_range,_person,_dateOnly,_groupShiftCategoryFairnessCreator );
        }

        [Test]
        public void VerifyShiftCategoryFairnessFactors()
        {
            using(_mocks.Record() )
            {
                Expect.Call(_groupShiftCategoryFairnessCreator.CalculateGroupShiftCategoryFairness(_person, _dateOnly)).
                IgnoreArguments().Return(_groupCategoryFairness);

                Expect.Call(_range.CachedShiftCategoryFairness()).IgnoreArguments().Return(_personCategoryFairness);
            }
            
            using (_mocks.Playback())
            {
                IShiftCategoryFairnessFactors result = _target.ShiftCategoryFairnessFactors();
                Assert.AreEqual(0.96d, result.FairnessFactor(_m), 0.01d);
                Assert.AreEqual(1.21d, result.FairnessFactor(_d), 0.01d);
                Assert.AreEqual(0.81d, result.FairnessFactor(_l), 0.01d);
                Assert.AreEqual(1.0404d, result.FairnessFactor(_n), 0.01);
            }

            
        }

		[Test]
		public void ShouldReturnOneForFirstShiftCategoryForPerson()
		{
            Dictionary<IShiftCategory, int> shiftDictionary = new Dictionary<IShiftCategory, int>();
            shiftDictionary.Add(_m, 1);
            _groupCategoryFairness = new ShiftCategoryFairness(shiftDictionary, new FairnessValueResult());
            _personCategoryFairness = new ShiftCategoryFairness();
            using (_mocks.Record())
            {
                
                Expect.Call(_groupShiftCategoryFairnessCreator.CalculateGroupShiftCategoryFairness(_person, _dateOnly)).
                IgnoreArguments().Return(_groupCategoryFairness);

                Expect.Call(_range.CachedShiftCategoryFairness()).IgnoreArguments().Return(_personCategoryFairness);
            }

            using (_mocks.Playback())
            {
                IShiftCategoryFairnessFactors result = _target.ShiftCategoryFairnessFactors();
                Assert.AreEqual(1d, result.FairnessFactor(_m));
            }
		    
		}

    	[Test] public void ShouldBeHundredPercentFair()
    	{
            Dictionary<IShiftCategory, int> shiftDictionary = new Dictionary<IShiftCategory, int>();
            shiftDictionary.Add(_m, 1);
            _groupCategoryFairness = new ShiftCategoryFairness(shiftDictionary, new FairnessValueResult());
            _personCategoryFairness = new ShiftCategoryFairness();
            using (_mocks.Record())
            {
                Expect.Call(_groupShiftCategoryFairnessCreator.CalculateGroupShiftCategoryFairness(_person, _dateOnly)).
                IgnoreArguments().Return(_groupCategoryFairness);

                Expect.Call(_range.CachedShiftCategoryFairness()).IgnoreArguments().Return(_personCategoryFairness);
            }

            using (_mocks.Playback())
            {
                IShiftCategoryFairnessFactors result = _target.ShiftCategoryFairnessFactors();
                Assert.AreEqual(0, result.FairnessFactor(_d));
            }
            
			
    	}

        private ShiftCategoryFairness CreateGroupCategoryFairness()
        {
            Dictionary<IShiftCategory, int> shiftDictionary = new Dictionary<IShiftCategory, int>();
            shiftDictionary.Add(_m, 28);
            shiftDictionary.Add(_d, 50);
            shiftDictionary.Add(_l, 20);
            shiftDictionary.Add(_n, 2);
            return new ShiftCategoryFairness(shiftDictionary, new FairnessValueResult());
        }

        private ShiftCategoryFairness CreatePersonCategoryFairness()
        {
            Dictionary<IShiftCategory, int> shiftDictionary = new Dictionary<IShiftCategory, int>();
            shiftDictionary.Add(_m, 3);
            shiftDictionary.Add(_d, 4);
            shiftDictionary.Add(_l, 3);
            return new ShiftCategoryFairness(shiftDictionary, new FairnessValueResult());
        }
    }
}
