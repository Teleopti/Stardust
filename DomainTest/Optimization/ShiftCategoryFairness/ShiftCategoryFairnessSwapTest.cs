using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
    [TestFixture]
    public class ShiftCategoryFairnessSwapTest
    {
        private IShiftCategoryFairnessCompareResult _group1, _group2;
        private IShiftCategory _shiftCategory1, _shiftCategory2;
        private IShiftCategoryFairnessSwap _target1, _target2;

        [SetUp]
        public void Setup()
        {
            _shiftCategory1 = new ShiftCategory("Day");
            _shiftCategory2 = new ShiftCategory("Night");

            _group1 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson>
                                                    {
                                                        new Person(),
                                                        new Person()
                                                    }
                          };

            _group2 = new ShiftCategoryFairnessCompareResult
                          {
                              OriginalMembers = new List<IPerson>
                                                    {
                                                        new Person()
                                                    }
                          };

            _target1 = new ShiftCategoryFairnessSwap
                           {
                               Group1 = _group1,
                               Group2 = _group2,
                               ShiftCategoryFromGroup1 = _shiftCategory1,
                               ShiftCategoryFromGroup2 = _shiftCategory2
                           };
            _target2 = new ShiftCategoryFairnessSwap
                           {
                               Group1 = _group1,
                               Group2 = _group2,
                               ShiftCategoryFromGroup1 = _shiftCategory1,
                               ShiftCategoryFromGroup2 = _shiftCategory2
                           };
        }

        [Test]
        public void IdenticalSwapShouldBeEqual()
        {
            var result = _target1.Equals(_target2);
            Assert.AreEqual(true, result);
        }

		[Test]
		public void ShouldBeFalseIfNull()
		{
			Assert.That(_target1.Equals(null), Is.False);
		}

		[Test]
		public void ShouldBeTrueIfSame()
		{
			Assert.That(_target1.Equals(_target1), Is.True);
		}

        [Test]
        public void SwappedGroupsAndShiftsShouldBeEqual()
        {
            _target2 = new ShiftCategoryFairnessSwap
                           {
                               Group1 = _group2,
                               Group2 = _group1,
                               ShiftCategoryFromGroup1 = _shiftCategory2,
                               ShiftCategoryFromGroup2 = _shiftCategory1
                           };

            var result = _target1.Equals(_target2);
            Assert.AreEqual(true, result);

	        var hashSet = new HashSet<IShiftCategoryFairnessSwap> {_target1, _target2};
			Assert.That(hashSet.Count, Is.EqualTo(1));
        }

        [Test]
        public void ShouldNotReturnTrueWhenItemsAreNotEqual()
        {
            _target2 = new ShiftCategoryFairnessSwap
                           {
                               Group1 = _group1,
                               Group2 = _group2,
                               ShiftCategoryFromGroup1 = _shiftCategory2,
                               ShiftCategoryFromGroup2 = _shiftCategory1
                           };
            var result = _target1.Equals(_target2);
            Assert.AreNotEqual(true, result);
        }

        [Test]
        public void CastingToObjectShouldStillBeEqual()
        {
            var objectTarget2 = (object) _target2;
            var result = _target1.Equals(objectTarget2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void GetHashCodeTest()
        {
            var result = _target1.GetHashCode();
            Assert.AreNotEqual(null, result);
        }
        
    }
}
