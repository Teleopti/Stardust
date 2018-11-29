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
	public class CustomShrinkageWrapperTest
	{
		private MockRepository mocks;
		private IBudgetGroup budgetGroup;
		private ICustomShrinkageWrapper target;
		private Guid customShrinkageId;
		private IDictionary<Guid, Percent> customShrinkages;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			budgetGroup = mocks.StrictMock<IBudgetGroup>();
			customShrinkages = new Dictionary<Guid, Percent>();
			target = new CustomShrinkageWrapper(budgetGroup, customShrinkages);
			customShrinkageId = Guid.NewGuid();
		}

		[Test]
		public void ShouldBeAbleToChangeCustomShrinkageValues()
		{
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.IsCustomShrinkage(customShrinkageId)).Return(true);
			}
			using (mocks.Playback())
			{
				target.SetShrinkage(customShrinkageId, new Percent(0.1d));
				Assert.AreEqual(new Percent(0.1d), target.GetShrinkage(customShrinkageId));
			}
		}

		[Test]
		public void ShouldRequirePercentageOfZeroOrMore()
		{
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.IsCustomShrinkage(customShrinkageId)).Return(true);
			}
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				using (mocks.Playback())
				{
					target.SetShrinkage(customShrinkageId, new Percent(-0.1d));
				}
			});

		}

		[Test]
		public void ShouldBeAbleToAddCustomShrinkageValuesTwice()
		{
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.IsCustomShrinkage(customShrinkageId)).Return(true).Repeat.Twice();
			}
			using (mocks.Playback())
			{
				target.SetShrinkage(customShrinkageId, new Percent(0.1d));
				target.SetShrinkage(customShrinkageId, new Percent(0.15d));
				Assert.AreEqual(new Percent(0.15d), target.GetShrinkage(customShrinkageId));
			}
		}

		[Test]
		public void ShouldBeAbleToGetCustomShrinkageValueWhenNoValueSet()
		{
			Assert.AreEqual(new Percent(), target.GetShrinkage(customShrinkageId));
		}

		[Test]
		public void ShouldThrowExceptionGivenNoCustomShrinkageDefinedForBudgetGroup()
		{
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.IsCustomShrinkage(customShrinkageId)).Return(false);
			}
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				using (mocks.Playback())
				{
					target.SetShrinkage(customShrinkageId, new Percent(0.1d));
				}
			});

		}

		[Test]
		public void CanCalculateTotalShrinkageFactor()
		{
			var g1 = Guid.NewGuid();
			var g2 = Guid.NewGuid();

			var customShrinkage = mocks.StrictMock<ICustomShrinkage>();
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.IsCustomShrinkage(g1)).Return(true);
				Expect.Call(budgetGroup.IsCustomShrinkage(g2)).Return(true);
				Expect.Call(budgetGroup.CustomShrinkages).Return(new List<ICustomShrinkage>
																			   {
																				   customShrinkage,
																				   customShrinkage
																			   });
				Expect.Call(customShrinkage.Id).Return(g1).Repeat.Twice();
				Expect.Call(customShrinkage.Id).Return(g2).Repeat.Twice();
			}
			using (mocks.Playback())
			{
				target.SetShrinkage(g1, new Percent(0.1d));
				target.SetShrinkage(g2, new Percent(0.2d));
				var product = Math.Round(target.GetTotal().Value, 2);
				product.Should().Be.EqualTo(Math.Round(0.3, 2));
			}
		}

		[Test]
		public void ShouldBeZeroIfNoShrinkageRows()
		{
			using (mocks.Record())
			{
				Expect.Call(budgetGroup.CustomShrinkages).Return(new List<ICustomShrinkage>(0));
			}
			using (mocks.Playback())
			{
				var product = target.GetTotal().Value;
				product.Should().Be.EqualTo(0);
			}
		}
	}
}
