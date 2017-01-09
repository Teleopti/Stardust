using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class SpecificationCheckerTest
	{
		private IGlobalSettingDataRepository _globalSettingDataRepository;

		[SetUp]
		public void Setup()
		{
			_globalSettingDataRepository = new FakeGlobalSettingDataRepository();
		}

		[Test]
		public void ShouldPendingIfSpecificationIsNotConfigured()
		{
			var dummySpecification = new DummySpecification(false);
			var specifications = new ShiftTradeSpecification[]
			{
				dummySpecification
			};

			var businessRuleConfigs = new ShiftTradeBusinessRuleConfig[] { };

			var swapDetails = new List<IShiftTradeSwapDetail>();
			var result = checkSpecification(specifications, businessRuleConfigs, swapDetails);

			Assert.That(dummySpecification.WasCalled);
			Assert.That(dummySpecification.HasBeenCalledWith(swapDetails));

			checkResult(result, false, false, dummySpecification.DenyReason);
		}

		[Test]
		public void ShouldNotValidateSpecificationIfItIsDisabled()
		{
			const string denyReason = "Test deny reason";
			var falseValidator = new ValidatorSpecificationForTest(false, denyReason);
			var dummySpecification = new DummySpecification(false);
			var specifications = new ShiftTradeSpecification[]
			{
				falseValidator,
				dummySpecification
			};

			var businessRuleConfigs = new[]
			{
				new ShiftTradeBusinessRuleConfig
				{
					BusinessRuleType = typeof(DummySpecification).FullName,
					Enabled = false
				}
			};

			IList<IShiftTradeSwapDetail> swapDetails = new List<IShiftTradeSwapDetail>();

			var result = checkSpecification(specifications, businessRuleConfigs, swapDetails);

			Assert.That(falseValidator.HasBeenCalledWith(swapDetails));
			Assert.That(!dummySpecification.WasCalled);
			checkResult(result, false, false, denyReason);
		}

		[Test]
		public void ShouldDenyIfSpecificationIsConfiguredAsDeny()
		{
			var dummySpecification = new DummySpecification(false);
			var specifications = new ShiftTradeSpecification[]
			{
				dummySpecification
			};

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
			var result = checkSpecification(specifications, businessRuleConfigs, swapDetails);

			Assert.That(dummySpecification.WasCalled);
			Assert.That(dummySpecification.HasBeenCalledWith(swapDetails));

			checkResult(result, false, true, dummySpecification.DenyReason);
		}

		[Test]
		public void ShouldPendingIfSpecificationIsConfiguredAsPending()
		{
			var dummySpecification = new DummySpecification(false);
			var specifications = new ShiftTradeSpecification[]
			{
				dummySpecification
			};

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
			var result = checkSpecification(specifications, businessRuleConfigs, swapDetails);

			Assert.That(dummySpecification.WasCalled);
			Assert.That(dummySpecification.HasBeenCalledWith(swapDetails));

			checkResult(result, false, false, dummySpecification.DenyReason);
		}

		private ShiftTradeRequestValidationResult checkSpecification(IEnumerable<ShiftTradeSpecification> specifications,
			ShiftTradeBusinessRuleConfig[] specificificatonConfigs, IList<IShiftTradeSwapDetail> swapDetails)
		{
			var shiftTradeSetting = new ShiftTradeSettings
			{
				BusinessRuleConfigs = specificificatonConfigs
			};
			_globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, shiftTradeSetting);

			var specificationChecker = new SpecificationCheckerWithConfig(specifications, _globalSettingDataRepository);

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