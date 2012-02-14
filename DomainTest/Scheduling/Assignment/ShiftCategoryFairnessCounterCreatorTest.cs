using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class ShiftCategoryFairnessTest
    {
		private IShiftCategoryFairness _target;

        [SetUp]
        public void Setup()
        {
            _target = new ShiftCategoryFairness();
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsNotNull(_target.ShiftCategoryFairnessDictionary);
        }

        [Test]
        public void VerifyAddShiftCategoryFairness()
        {
            IShiftCategory fm = new ShiftCategory("FM");
            IShiftCategory da = new ShiftCategory("DA");
            IShiftCategory kv = new ShiftCategory("KV");

            Dictionary<IShiftCategory, int> shiftDic1 = new Dictionary<IShiftCategory, int>();
            shiftDic1.Add(fm, 5);
            shiftDic1.Add(da, 7);

			IShiftCategoryFairness holder1 = new ShiftCategoryFairness(shiftDic1, new FairnessValueResult());

            Dictionary<IShiftCategory, int> shiftDic2 = new Dictionary<IShiftCategory, int>();
            shiftDic2.Add(da, 3);
            shiftDic2.Add(kv, 5);
			IShiftCategoryFairness holder2 = new ShiftCategoryFairness(shiftDic2, new FairnessValueResult());

            IShiftCategoryFairness result = _target.Add(holder1);
            Assert.AreEqual(2, result.ShiftCategoryFairnessDictionary.Keys.Count);
            Assert.AreEqual(5, result.ShiftCategoryFairnessDictionary[fm]);
            Assert.AreEqual(7, result.ShiftCategoryFairnessDictionary[da]);

            result = result.Add(holder2);
            Assert.AreEqual(3, result.ShiftCategoryFairnessDictionary.Keys.Count);
            Assert.AreEqual(5, result.ShiftCategoryFairnessDictionary[fm]);
            Assert.AreEqual(10, result.ShiftCategoryFairnessDictionary[da]);
            Assert.AreEqual(5, result.ShiftCategoryFairnessDictionary[kv]);
        }

        [Test]
        public void VerifyEquals()
        {
            IShiftCategory fm = new ShiftCategory("FM");
            IShiftCategory da = new ShiftCategory("DA");

            Dictionary<IShiftCategory, int> shiftDic1 = new Dictionary<IShiftCategory, int>();
            shiftDic1.Add(fm, 5);
            shiftDic1.Add(da, 7);

            _target = new ShiftCategoryFairness(shiftDic1, new FairnessValueResult());

            Dictionary<IShiftCategory, int> shiftDic2 = new Dictionary<IShiftCategory, int>();
            shiftDic2.Add(fm, 5);
            shiftDic2.Add(da, 7);

			IShiftCategoryFairness holder2 = new ShiftCategoryFairness(shiftDic2, new FairnessValueResult());

            Assert.AreEqual(_target, holder2);
            Assert.AreNotEqual(_target, new ShiftCategoryFairness());


			HashSet<IShiftCategoryFairness> hashSet = new HashSet<IShiftCategoryFairness>();
            hashSet.Add(holder2);
            hashSet.Add(new ShiftCategoryFairness());
            hashSet.Add(holder2);

            Assert.AreEqual(2, hashSet.Count);
        }

    }



}
