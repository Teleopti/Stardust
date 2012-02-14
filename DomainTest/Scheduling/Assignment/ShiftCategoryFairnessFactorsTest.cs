using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class ShiftCategoryFairnessFactorsTest
    {
        private IShiftCategoryFairnessFactors _target;
        private IShiftCategory _category1;
        private IShiftCategory _category2;

        [SetUp]
        public void Setup()
        {
            IDictionary<IShiftCategory, double> dictionary = new Dictionary<IShiftCategory, double>();
            _category1 = ShiftCategoryFactory.CreateShiftCategory("Category1");
            _category2 = ShiftCategoryFactory.CreateShiftCategory("Category2");
            dictionary.Add(_category1, 0.5d);
            dictionary.Add(_category2, 1.5d);
            _target = new ShiftCategoryFairnessFactors(dictionary, 0);
        }

        [Test]
        public void VerifyShiftCategoryFairnessFactor()
        {
            Assert.AreEqual(0.5d, _target.FairnessFactor(_category1));
            Assert.AreEqual(1.5d, _target.FairnessFactor(_category2));
        }

		[Test]
		public void ShouldReturnZeroIfKeyNotFound()
		{
			IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("Hej");
			Assert.AreEqual(0d, _target.FairnessFactor(category));
		}
    }
}