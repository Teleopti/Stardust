﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class ShiftCategoryFairnessComparerTest
    {
        private ShiftCategoryFairnessComparer _target;
        private ShiftCategoryFairnessCompareValue _compareValueTarget;
        private ShiftCategoryFairnessCompareResult _resultTarget;

        [SetUp]
        public void Setup()
        {
            _target = new ShiftCategoryFairnessComparer();
            _compareValueTarget = new ShiftCategoryFairnessCompareValue
                                      {
                                          ComparedTo = 0,
                                          Original = 0,
                                          ShiftCategory = new ShiftCategory("Day")
                                      };
            _resultTarget = new ShiftCategoryFairnessCompareResult
                                {
                                    OriginalMembers = new List<IPerson> { new Person() },
                                    ShiftCategoryFairnessCompareValues = new List<IShiftCategoryFairnessCompareValue> { _compareValueTarget },
                                    StandardDeviation = 0.1
                                };
        }

        [Test]
        public void ShouldContainAllCategories()
        {
            var cat = ShiftCategoryFactory.CreateShiftCategory("Morning");
            var cat2 = ShiftCategoryFactory.CreateShiftCategory("Day");
            var cat3 = ShiftCategoryFactory.CreateShiftCategory("Late");

            var catDic = new Dictionary<IShiftCategory, int> {{cat, 2}, {cat2, 2}};
            var catDic2 = new Dictionary<IShiftCategory, int> {{cat2, 2}, {cat3, 2}};

            var fairness = new ShiftCategoryFairness(catDic, new FairnessValueResult());
            var fairness2 = new ShiftCategoryFairness(catDic2, new FairnessValueResult());

            var result = _target.Compare(fairness, fairness2, new List<IShiftCategory> {cat, cat2, cat3});
            Assert.That(result.ShiftCategoryFairnessCompareValues.Count, Is.EqualTo(3));
            Assert.That(result.ShiftCategoryFairnessCompareValues[0].Original, Is.EqualTo(.50));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].Original, Is.EqualTo(.50));
            Assert.That(result.ShiftCategoryFairnessCompareValues[2].Original, Is.EqualTo(.0));

            Assert.That(result.ShiftCategoryFairnessCompareValues[0].ComparedTo, Is.EqualTo(.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].ComparedTo, Is.EqualTo(.50));
            Assert.That(result.ShiftCategoryFairnessCompareValues[2].ComparedTo, Is.EqualTo(.50));
        }

        // This test is based on ander excel first example
        //                    Distribution, number of per shift assigned					
        //John		ShiftCats	Group	Person	Diff	FactorForShiftEvaluation	Will be between 0 and 4, often around 1!	
        //        M	0,28	0,3	0,02	0,9604		
        //        D	0,5	0,4	-0,1	1,21		
        //        L	0,2	0,3	0,1	0,81		
        //        N	0,02	0	-0,02	1,0404		
        //                    0,072111026			

        [Test]
        public void ShouldContainCalculateStandardDeviation()
        {
            var cat = ShiftCategoryFactory.CreateShiftCategory("Morning");
            var cat2 = ShiftCategoryFactory.CreateShiftCategory("Day");
            var cat3 = ShiftCategoryFactory.CreateShiftCategory("Late");
            var cat4 = ShiftCategoryFactory.CreateShiftCategory("Night");

            var catDic = new Dictionary<IShiftCategory, int> {{cat, 30}, {cat2, 40}, {cat3, 30}, {cat4, 0}};
            var catDic2 = new Dictionary<IShiftCategory, int> {{cat, 28}, {cat2, 50}, {cat3, 20}, {cat4, 2}};

            var fairness = new ShiftCategoryFairness(catDic, new FairnessValueResult());
            var fairness2 = new ShiftCategoryFairness(catDic2, new FairnessValueResult());

            var result = _target.Compare(fairness, fairness2, new List<IShiftCategory> {cat, cat2, cat3, cat4});
            Assert.That(result.ShiftCategoryFairnessCompareValues.Count, Is.EqualTo(4));
            Assert.That(result.ShiftCategoryFairnessCompareValues[0].Original, Is.EqualTo(0.3));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].Original, Is.EqualTo(0.4));
            Assert.That(result.ShiftCategoryFairnessCompareValues[2].Original, Is.EqualTo(0.3));
            Assert.That(result.ShiftCategoryFairnessCompareValues[3].Original, Is.EqualTo(0.0));

            Assert.That(result.ShiftCategoryFairnessCompareValues[0].ComparedTo, Is.EqualTo(.28));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].ComparedTo, Is.EqualTo(.50));
            Assert.That(result.ShiftCategoryFairnessCompareValues[2].ComparedTo, Is.EqualTo(.20));
            Assert.That(result.ShiftCategoryFairnessCompareValues[3].ComparedTo, Is.EqualTo(.02));

            Assert.That(System.Math.Round(result.StandardDeviation, 6), Is.EqualTo(0.072111));
        }

        [Test]
        public void ShouldContainCalculateStandardDeviationExample2()
        {
            var cat = ShiftCategoryFactory.CreateShiftCategory("Morning");
            var cat2 = ShiftCategoryFactory.CreateShiftCategory("Day");
            var cat3 = ShiftCategoryFactory.CreateShiftCategory("Late");
            var cat4 = ShiftCategoryFactory.CreateShiftCategory("Night");

            var catDic = new Dictionary<IShiftCategory, int> {{cat, 100}, {cat2, 0}, {cat3, 0}, {cat4, 0}};
            var catDic2 = new Dictionary<IShiftCategory, int> {{cat, 28}, {cat2, 50}, {cat3, 20}, {cat4, 2}};

            var fairness = new ShiftCategoryFairness(catDic, new FairnessValueResult());
            var fairness2 = new ShiftCategoryFairness(catDic2, new FairnessValueResult());

            var result = _target.Compare(fairness, fairness2, new List<IShiftCategory> {cat, cat2, cat3, cat4});
            Assert.That(result.ShiftCategoryFairnessCompareValues.Count, Is.EqualTo(4));
            Assert.That(result.ShiftCategoryFairnessCompareValues[0].Original, Is.EqualTo(1.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].Original, Is.EqualTo(0.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[2].Original, Is.EqualTo(0.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[3].Original, Is.EqualTo(0.0));

            Assert.That(result.ShiftCategoryFairnessCompareValues[0].ComparedTo, Is.EqualTo(.28));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].ComparedTo, Is.EqualTo(.50));
            Assert.That(result.ShiftCategoryFairnessCompareValues[2].ComparedTo, Is.EqualTo(.20));
            Assert.That(result.ShiftCategoryFairnessCompareValues[3].ComparedTo, Is.EqualTo(.02));

            Assert.That(System.Math.Round(result.StandardDeviation, 6), Is.EqualTo(0.449667));
        }

        [Test]
        public void ShouldContainCalculateStandardDeviationExample3()
        {
            var cat = ShiftCategoryFactory.CreateShiftCategory("Morning");
            var cat2 = ShiftCategoryFactory.CreateShiftCategory("Day");
            var cat3 = ShiftCategoryFactory.CreateShiftCategory("Late");
            var cat4 = ShiftCategoryFactory.CreateShiftCategory("Night");

            var catDic = new Dictionary<IShiftCategory, int> {{cat, 100}, {cat2, 0}};
            var catDic2 = new Dictionary<IShiftCategory, int> {{cat3, 60}, {cat4, 40}};

            var fairness = new ShiftCategoryFairness(catDic, new FairnessValueResult());
            var fairness2 = new ShiftCategoryFairness(catDic2, new FairnessValueResult());

            var result = _target.Compare(fairness, fairness2, new List<IShiftCategory> {cat, cat2, cat3, cat4});
            Assert.That(result.ShiftCategoryFairnessCompareValues.Count, Is.EqualTo(4));
            Assert.That(result.ShiftCategoryFairnessCompareValues[0].Original, Is.EqualTo(1.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[0].ComparedTo, Is.EqualTo(0.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].Original, Is.EqualTo(0.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[1].ComparedTo, Is.EqualTo(0.0));

            Assert.That(result.ShiftCategoryFairnessCompareValues[2].Original, Is.EqualTo(0.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[2].ComparedTo, Is.EqualTo(0.6));
            Assert.That(result.ShiftCategoryFairnessCompareValues[3].Original, Is.EqualTo(0.0));
            Assert.That(result.ShiftCategoryFairnessCompareValues[3].ComparedTo, Is.EqualTo(0.4));

            Assert.That(System.Math.Round(result.StandardDeviation, 6), Is.EqualTo(0.616441));
        }

        #region ShiftCategoryFairnessCompareValue
        [Test]
        public void ShouldReturnFalseWhenNull()
        {
            Assert.That(_compareValueTarget.Equals(null), Is.False);
        }

        [Test]
        public void ShouldReturnSameWithSameItems()
        {
            var compareValueTarget2 = new ShiftCategoryFairnessCompareValue
                         {
                             ComparedTo = 0,
                             Original = 0,
                             ShiftCategory = new ShiftCategory("Day")
                         };

            var result = _compareValueTarget.Equals(compareValueTarget2);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void ShouldReturnTrueIfSame()
        {
            Assert.That(_compareValueTarget.Equals(_compareValueTarget), Is.True);
        }

        [Test]
        public void ShouldReturnTrueIfCopy()
        {
            var compareValue = _compareValueTarget;
            Assert.That(_compareValueTarget.Equals(compareValue), Is.True);
        }

        [Test]
        public void CastingToObjectShouldStillBeEqual()
        {
            var compareValueTarget2 = new ShiftCategoryFairnessCompareValue
                         {
                             ComparedTo = 0,
                             Original = 0,
                             ShiftCategory = new ShiftCategory("Day")
                         };
            var objectTarget2 = (object) compareValueTarget2;
            Assert.That(_compareValueTarget.Equals(objectTarget2), Is.True);
        }

        [Test]
        public void CastingToObjectShouldNotBeEqualForDifferentItems()
        {
            object variable = null;
            Assert.That(_compareValueTarget.Equals(variable), Is.False);
        }

        [Test]
        public void CastingSameItemToObjectShouldReturnFalse()
        {
            var objectValue = (object) _compareValueTarget;
            Assert.That(_compareValueTarget.Equals(objectValue), Is.True);
        }

        [Test]
        public void GetHashCodeTest()
        {
            var result = _compareValueTarget.GetHashCode();
            Assert.AreNotEqual(null, result);
        }

        #endregion

        # region ShiftCategoryFairnessCompareResult
        [Test]
        public void EqualsNullRefForObjectShouldBeFalse()
        {
            ShiftCategoryFairnessCompareResult compare = null;
            var nullCompare = (object) compare;
            Assert.That(_resultTarget.Equals(nullCompare), Is.False);
        }

        [Test]
        public void EqualsNullRefShouldBeFalse()
        {
            ShiftCategoryFairnessCompareResult compare = null;
            Assert.That(_resultTarget.Equals(compare), Is.False);
        }

        [Test]
        public void SameRefShouldBeTrue()
        {
            var compare = _resultTarget;
            Assert.That(_resultTarget.Equals(compare), Is.True);
        }

        #endregion

    }
}