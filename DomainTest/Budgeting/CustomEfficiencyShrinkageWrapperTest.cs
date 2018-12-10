using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Budgeting
{
    [TestFixture]
    public class CustomEfficiencyShrinkageWrapperTest
    {
        private MockRepository mocks;
        private IBudgetGroup budgetGroup;
        private ICustomEfficiencyShrinkageWrapper target;
        private Guid customShrinkageId;
        private IDictionary<Guid, Percent> customShrinkages;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            budgetGroup = mocks.StrictMock<IBudgetGroup>();
            customShrinkages = new Dictionary<Guid, Percent>();
            target = new CustomEfficiencyShrinkageWrapper(budgetGroup, customShrinkages);
            customShrinkageId = Guid.NewGuid();
        }

        [Test]
        public void ShouldBeAbleToChangeCustomShrinkageValues()
        {
            using (mocks.Record())
            {
                Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(customShrinkageId)).Return(true);
            }
            using (mocks.Playback())
            {
                target.SetEfficiencyShrinkage(customShrinkageId, new Percent(0.1d));
                Assert.AreEqual(new Percent(0.1d), target.GetEfficiencyShrinkage(customShrinkageId));
            }
        }

        [Test]
        public void ShouldRequirePercentageOfZeroOrMore()
        {
            using (mocks.Record())
            {
                Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(customShrinkageId)).Return(true);
            }
	        Assert.Throws<ArgumentOutOfRangeException>(() =>
	        {
				using (mocks.Playback())
				{
					target.SetEfficiencyShrinkage(customShrinkageId, new Percent(-0.1d));
				}
			});
        }

        [Test]
        public void ShouldBeAbleToAddCustomShrinkageValuesTwice()
        {
            using (mocks.Record())
            {
                Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(customShrinkageId)).Return(true).Repeat.Twice();
            }
            using (mocks.Playback())
            {
                target.SetEfficiencyShrinkage(customShrinkageId, new Percent(0.1d));
                target.SetEfficiencyShrinkage(customShrinkageId, new Percent(0.15d));
                Assert.AreEqual(new Percent(0.15d), target.GetEfficiencyShrinkage(customShrinkageId));
            }
        }

        [Test]
        public void ShouldBeAbleToGetCustomShrinkageValueWhenNoValueSet()
        {
            Assert.AreEqual(new Percent(), target.GetEfficiencyShrinkage(customShrinkageId));
        }

        [Test]
        public void ShouldThrowExceptionGivenNoCustomShrinkageDefinedForBudgetGroup()
        {
            using (mocks.Record())
            {
                Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(customShrinkageId)).Return(false);
            }

			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				using (mocks.Playback())
				{
					target.SetEfficiencyShrinkage(customShrinkageId, new Percent(0.1d));
				}
			});
        }

        [Test]
        public void CanCalculateTotalShrinkageFactor()
        {
            var g1 = Guid.NewGuid();
            var g2 = Guid.NewGuid();

            var customEfficiencyShrinkage = mocks.StrictMock<ICustomEfficiencyShrinkage>();
            using (mocks.Record())
            {
                Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(g1)).Return(true);
                Expect.Call(budgetGroup.IsCustomEfficiencyShrinkage(g2)).Return(true);
                Expect.Call(budgetGroup.CustomEfficiencyShrinkages).Return(new List<ICustomEfficiencyShrinkage>
                                                                               {
                                                                                   customEfficiencyShrinkage,
                                                                                   customEfficiencyShrinkage
                                                                               });
                Expect.Call(customEfficiencyShrinkage.Id).Return(g1).Repeat.Twice();
                Expect.Call(customEfficiencyShrinkage.Id).Return(g2).Repeat.Twice();
            }
            using (mocks.Playback())
            {
                target.SetEfficiencyShrinkage(g1, new Percent(0.1d));
                target.SetEfficiencyShrinkage(g2, new Percent(0.2d));

                var product = Math.Round(target.GetTotal().Value, 2);
                product.Should().Be.EqualTo(Math.Round(0.3, 2));
            }
        }

        [Test]
        public void ShouldBeZeroIfNoShrinkageRows()
        {
            using (mocks.Record())
            {
                Expect.Call(budgetGroup.CustomEfficiencyShrinkages).Return(new List<ICustomEfficiencyShrinkage>(0));
            }
            using (mocks.Playback())
            {
                var product = target.GetTotal().Value;
                product.Should().Be.EqualTo(0);
            }
        }
    }
}
