using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
    [TestFixture]
    public class ShiftCategoryFairnessTest
    {
		private IShiftCategoryFairnessHolder _target;

        [SetUp]
        public void Setup()
        {
            _target = new Domain.Optimization.ShiftCategoryFairness.ShiftCategoryFairnessHolder();
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

			IShiftCategoryFairnessHolder holder1 = new Domain.Optimization.ShiftCategoryFairness.ShiftCategoryFairnessHolder(shiftDic1, new FairnessValueResult());

            Dictionary<IShiftCategory, int> shiftDic2 = new Dictionary<IShiftCategory, int>();
            shiftDic2.Add(da, 3);
            shiftDic2.Add(kv, 5);
			IShiftCategoryFairnessHolder holder2 = new Domain.Optimization.ShiftCategoryFairness.ShiftCategoryFairnessHolder(shiftDic2, new FairnessValueResult());

            IShiftCategoryFairnessHolder result = _target.Add(holder1);
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

            _target = new Domain.Optimization.ShiftCategoryFairness.ShiftCategoryFairnessHolder(shiftDic1, new FairnessValueResult());

            Dictionary<IShiftCategory, int> shiftDic2 = new Dictionary<IShiftCategory, int>();
            shiftDic2.Add(fm, 5);
            shiftDic2.Add(da, 7);

			IShiftCategoryFairnessHolder holder2 = new Domain.Optimization.ShiftCategoryFairness.ShiftCategoryFairnessHolder(shiftDic2, new FairnessValueResult());

            Assert.AreEqual(_target, holder2);
            Assert.AreNotEqual(_target, new Domain.Optimization.ShiftCategoryFairness.ShiftCategoryFairnessHolder());


			HashSet<IShiftCategoryFairnessHolder> hashSet = new HashSet<IShiftCategoryFairnessHolder>();
            hashSet.Add(holder2);
            hashSet.Add(new Domain.Optimization.ShiftCategoryFairness.ShiftCategoryFairnessHolder());
            hashSet.Add(holder2);

            Assert.AreEqual(2, hashSet.Count);
        }

    }



}
