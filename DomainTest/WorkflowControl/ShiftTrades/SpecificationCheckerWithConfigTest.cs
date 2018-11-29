using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	class SpecificationCheckerWithConfigTest
	{
		private ValidatorSpecificationForTest _openShiftTradePeriodSpecification;
		private ValidatorSpecificationForTest _shiftTradeSkillSpecification;
		private ValidatorSpecificationForTest _shiftTradeTargetTimeSpecification;
		private ValidatorSpecificationForTest _isWorkflowControlSetNotNullSpecification;
		private ValidatorSpecificationForTest _shiftTradeAbsenceSpecification;
		private ValidatorSpecificationForTest _shiftTradePersonalActivitySpecification;
		private ValidatorSpecificationForTest _shiftTradeMeetingSpecification;
		private ValidatorSpecificationForTest _shiftTradeMaxSeatsSpecification;

		private IShiftTradeLightValidator shiftTradeLightValidator;

		[SetUp]
		public void Setup()
		{
			shiftTradeLightValidator = MockRepository.GenerateMock<IShiftTradeLightValidator>();
			_openShiftTradePeriodSpecification = new ValidatorSpecificationForTest(true, "_openShiftTradePeriodSpecification");
			_shiftTradeSkillSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeSkillSpecification");
			_shiftTradeTargetTimeSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeTargetTimeSpecification");
			_isWorkflowControlSetNotNullSpecification = new ValidatorSpecificationForTest(true, "_isWorkflowControlSetNotNullSpecification");
			_shiftTradeAbsenceSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeAbsenceSpecification");
			_shiftTradePersonalActivitySpecification = new ValidatorSpecificationForTest(true, "_shiftTradePersonalActivitySpecification");
			_shiftTradeMeetingSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeMettingSpecification");
			_shiftTradeMaxSeatsSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeMaxSeatsSpecification");
		}

		private ShiftTradeValidator createValidator()
		{
			var specifications = new[]
			{
				_openShiftTradePeriodSpecification,
				_shiftTradeSkillSpecification,
				_shiftTradeTargetTimeSpecification,
				_isWorkflowControlSetNotNullSpecification,
				_shiftTradeAbsenceSpecification,
				_shiftTradePersonalActivitySpecification,
				_shiftTradeMeetingSpecification,
				_shiftTradeMaxSeatsSpecification
			};
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings
			{
				BusinessRuleConfigs = new ShiftTradeBusinessRuleConfig[] { }
			});
			var specificationChecker = new SpecificationCheckerWithConfig(specifications, globalSettingDataRepository);
			return new ShiftTradeValidator(shiftTradeLightValidator, specificationChecker);
		}

		[Test]
		public void ShouldCheckLightRule()
		{
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(new Person(), new Person(), new DateOnly(), new DateOnly());
			var result = new ShiftTradeRequestValidationResult(false, true, string.Empty);
			shiftTradeLightValidator.Expect(m => m.Validate(null)).IgnoreArguments().Return(result);
			var request = new ShiftTradeRequest(new[] { (IShiftTradeSwapDetail)shiftTradeSwapDetail });
			var validator = createValidator();
			validator.Validate(request).Should().Be.SameInstanceAs(result);
		}

		[Test]
		public void VerifyThatAllSpecificationsGetsCalled()
		{
			var validator = createValidator();
			var details = new List<IShiftTradeSwapDetail>();

			validator.Validate(details);

			Assert.That(_openShiftTradePeriodSpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradeSkillSpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradeTargetTimeSpecification.HasBeenCalledWith(details));
			Assert.That(_isWorkflowControlSetNotNullSpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradeAbsenceSpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradePersonalActivitySpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradeMeetingSpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradeMaxSeatsSpecification.HasBeenCalledWith(details));
		}

		[Test]
		public void VerifyReturnsFalseIfAnyValidatorIsFalse()
		{
			//not so pretty.. we really need a Ilist of the same interface in the container...
			const string denyReason = "denyReason";
			var validator = createValidator();
			IList<IShiftTradeSwapDetail> details = new List<IShiftTradeSwapDetail>();

			Assert.IsTrue(validator.Validate(details).IsOk);

			var falseValidator = new ValidatorSpecificationForTest(false, denyReason);
			var specifications = new[]
			{
				falseValidator,
				_shiftTradeSkillSpecification,
				_shiftTradeTargetTimeSpecification,
				_isWorkflowControlSetNotNullSpecification,
				_shiftTradeAbsenceSpecification,
				_shiftTradePersonalActivitySpecification,
				_shiftTradeMeetingSpecification
			};
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings
			{
				BusinessRuleConfigs = new ShiftTradeBusinessRuleConfig[] { }
			});
			var specificationChecker = new SpecificationCheckerWithConfig(specifications, globalSettingDataRepository);
			validator = new ShiftTradeValidator(MockRepository.GenerateMock<IShiftTradeLightValidator>(), specificationChecker);
			checkResult(validator.Validate(details), false, true, denyReason);
		}

		[Test]
		public void VerifyValidateReturnsFalseIfRequestIsNull()
		{
			IShiftTradeValidator validator = createValidator();
			Assert.IsFalse(validator.Validate(null).IsOk);

			var request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>());
			Assert.IsTrue(validator.Validate(request).IsOk);
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
