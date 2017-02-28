using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class SpecificationCheckerTest
	{
		private const string denyReason = "Test deny reason";
		private ValidatorSpecificationForTest satisfiedSpecification;
		private DummySpecification dummySpecification;
		private ShiftTradeSpecification[] specifications;

		[SetUp]
		public void Setup()
		{
			satisfiedSpecification = new ValidatorSpecificationForTest(true, denyReason);
			dummySpecification = new DummySpecification(false);
			specifications = new ShiftTradeSpecification[]
			{
				satisfiedSpecification,
				dummySpecification
			};
		}

		[Test]
		public void ShouldDenyIfSpecificationIsNotConfiguredFailed()
		{
			var businessRuleConfigs = new ShiftTradeBusinessRuleConfig[] { };
			var swapDetails = new List<IShiftTradeSwapDetail>();
			var result = checkSpecification(businessRuleConfigs, swapDetails);

			Assert.That(satisfiedSpecification.HasBeenCalledWith(swapDetails));

			Assert.That(dummySpecification.WasCalled);
			Assert.That(dummySpecification.HasBeenCalledWith(swapDetails));

			checkResult(result, false, true, dummySpecification.DenyReason);
		}

		[Test]
		public void ShouldNotValidateSpecificationDisabled()
		{
			var businessRuleConfigs = new[]
			{
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(DummySpecification).FullName,
					Enabled = false
				}
			};

			IList<IShiftTradeSwapDetail> swapDetails = new List<IShiftTradeSwapDetail>();

			var result = checkSpecification(businessRuleConfigs, swapDetails);

			Assert.That(satisfiedSpecification.HasBeenCalledWith(swapDetails));
			Assert.That(!dummySpecification.WasCalled);
			checkResult(result, true, false, string.Empty);
		}

		[Test]
		public void ShouldDenyIfSpecificationIsConfiguredAsDenyFailed()
		{
			var businessRuleConfigs = new[]
			{
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(DummySpecification).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.AutoDeny
				}
			};

			var swapDetails = new List<IShiftTradeSwapDetail>();
			var result = checkSpecification(businessRuleConfigs, swapDetails);

			Assert.That(dummySpecification.WasCalled);
			Assert.That(dummySpecification.HasBeenCalledWith(swapDetails));
			Assert.That(satisfiedSpecification.HasBeenCalledWith(swapDetails));

			checkResult(result, false, true, dummySpecification.DenyReason);
		}

		[Test]
		public void ShouldPendingIfSpecificationIsConfiguredAsPending()
		{
			var businessRuleConfigs = new[]
			{
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(DummySpecification).FullName,
					Enabled = true,
					HandleOptionOnFailed = RequestHandleOption.Pending
				}
			};

			var swapDetails = new List<IShiftTradeSwapDetail>();
			var result = checkSpecification(businessRuleConfigs, swapDetails);

			Assert.That(dummySpecification.WasCalled);
			Assert.That(dummySpecification.HasBeenCalledWith(swapDetails));
			Assert.That(satisfiedSpecification.HasBeenCalledWith(swapDetails));

			checkResult(result, false, false, dummySpecification.DenyReason);
		}

		private ShiftTradeRequestValidationResult checkSpecification(ShiftTradeBusinessRuleConfig[] specificificatonConfigs, IList<IShiftTradeSwapDetail> swapDetails)
		{
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings
			{
				BusinessRuleConfigs = specificificatonConfigs
			});

			var specificationChecker = new SpecificationCheckerWithConfig(specifications, globalSettingDataRepository);
			return specificationChecker.Check(swapDetails);
		}

		private static void checkResult(ShiftTradeRequestValidationResult result, bool shouldBeOk, bool shouldBeDenied,
			string expectedDenyReason)
		{
			Assert.AreEqual(shouldBeOk, result.IsOk, "Value of IsOk property is not expected");
			Assert.AreEqual(shouldBeDenied, result.ShouldBeDenied, "Value of ShouldBeDenied is not expected");
			Assert.AreEqual(expectedDenyReason, result.DenyReason, "Denyreason is not expected");
		}
	}
}