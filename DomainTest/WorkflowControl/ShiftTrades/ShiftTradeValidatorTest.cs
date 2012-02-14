using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class ShiftTradeValidatorTest
    {

        private ValidatorSpecificationForTest _openShiftTradePeriodSpecification;
        private ValidatorSpecificationForTest _shiftTradeSkillSpecification;
        private ValidatorSpecificationForTest _shiftTradeTargetTimeSpecification;
        private ValidatorSpecificationForTest _isWorkflowControlSetNotNullSpecification;
    	private ValidatorSpecificationForTest _shiftTradeAbsenceSpecification;
    	private ValidatorSpecificationForTest _shiftTradePersonalActivitySpecification;
    	private ValidatorSpecificationForTest _shiftTradeMeetingSpecification;

        [SetUp]
        public void Setup()
        {

            _openShiftTradePeriodSpecification = new ValidatorSpecificationForTest(true, "_openShiftTradePeriodSpecification");
            _shiftTradeSkillSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeSkillSpecification");
            _shiftTradeTargetTimeSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeTargetTimeSpecification");
            _isWorkflowControlSetNotNullSpecification = new ValidatorSpecificationForTest(true, "_isWorkflowControlSetNotNullSpecification");
			_shiftTradeAbsenceSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeAbsenceSpecification");
			_shiftTradePersonalActivitySpecification = new ValidatorSpecificationForTest(true, "_shiftTradePersonalActivitySpecification");
			_shiftTradeMeetingSpecification = new ValidatorSpecificationForTest(true, "_shiftTradeMettingSpecification");
		}

        private IShiftTradeValidator CreateValidator()
        {
            return new ShiftTradeValidator(_openShiftTradePeriodSpecification, 
                _shiftTradeSkillSpecification, 
                _shiftTradeTargetTimeSpecification,
                _isWorkflowControlSetNotNullSpecification,
				_shiftTradeAbsenceSpecification,
				_shiftTradePersonalActivitySpecification,
				_shiftTradeMeetingSpecification);
        }

        

        [Test]
        public void VerifyThatAllSpecificationsGetsCalled()
        {

            IShiftTradeValidator validator = CreateValidator();
            
            IList<IShiftTradeSwapDetail> details = new List<IShiftTradeSwapDetail>();

            validator.Validate(details);

            Assert.IsTrue(_openShiftTradePeriodSpecification.HasBeenCalledWith.Equals(details));
            Assert.IsTrue(_shiftTradeSkillSpecification.HasBeenCalledWith.Equals(details));
            Assert.IsTrue(_shiftTradeTargetTimeSpecification.HasBeenCalledWith.Equals(details));
            Assert.IsTrue(_isWorkflowControlSetNotNullSpecification.HasBeenCalledWith.Equals(details));
			Assert.IsTrue(_shiftTradeAbsenceSpecification.HasBeenCalledWith.Equals(details));
			Assert.IsTrue(_shiftTradePersonalActivitySpecification.HasBeenCalledWith.Equals(details));
			Assert.IsTrue(_shiftTradeMeetingSpecification.HasBeenCalledWith.Equals(details));
        }

        [Test]
        public void VerifyReturnsFalseIfAnyValidatorIsFalse()
        {
            //not so pretty.. we really need a Ilist of the same interface in the container...
            const string denyReason = "denyReason"; 
            var validator = CreateValidator();
            IList<IShiftTradeSwapDetail> details = new List<IShiftTradeSwapDetail>();

            Assert.IsTrue(validator.Validate(details).Value);

            var falseValidator = new ValidatorSpecificationForTest(false, denyReason);
            validator = new ShiftTradeValidator(falseValidator, _shiftTradeSkillSpecification, _shiftTradeTargetTimeSpecification, _isWorkflowControlSetNotNullSpecification, _shiftTradeAbsenceSpecification, _shiftTradePersonalActivitySpecification, _shiftTradeMeetingSpecification);
            CheckResult(validator.Validate(details), false, denyReason);

			validator = new ShiftTradeValidator(_openShiftTradePeriodSpecification, falseValidator, _shiftTradeTargetTimeSpecification, _isWorkflowControlSetNotNullSpecification, _shiftTradeAbsenceSpecification, _shiftTradePersonalActivitySpecification, _shiftTradeMeetingSpecification);
            CheckResult(validator.Validate(details), false, denyReason);

			validator = new ShiftTradeValidator(_openShiftTradePeriodSpecification, _shiftTradeSkillSpecification, falseValidator, _isWorkflowControlSetNotNullSpecification, _shiftTradeAbsenceSpecification, _shiftTradePersonalActivitySpecification, _shiftTradeMeetingSpecification);
            CheckResult(validator.Validate(details), false, denyReason);

			validator = new ShiftTradeValidator(_openShiftTradePeriodSpecification, _shiftTradeSkillSpecification, _shiftTradeTargetTimeSpecification, falseValidator, _shiftTradeAbsenceSpecification, _shiftTradePersonalActivitySpecification, _shiftTradeMeetingSpecification);
            CheckResult(validator.Validate(details), false, denyReason);

			validator = new ShiftTradeValidator(_openShiftTradePeriodSpecification, _shiftTradeSkillSpecification, _shiftTradeTargetTimeSpecification, _isWorkflowControlSetNotNullSpecification, falseValidator, _shiftTradePersonalActivitySpecification, _shiftTradeMeetingSpecification);
			CheckResult(validator.Validate(details), false, denyReason);

			validator = new ShiftTradeValidator(_openShiftTradePeriodSpecification, _shiftTradeSkillSpecification, _shiftTradeTargetTimeSpecification, _isWorkflowControlSetNotNullSpecification, _shiftTradeAbsenceSpecification, falseValidator, _shiftTradeMeetingSpecification);
			CheckResult(validator.Validate(details), false, denyReason);

			validator = new ShiftTradeValidator(_openShiftTradePeriodSpecification, _shiftTradeSkillSpecification, _shiftTradeTargetTimeSpecification, _isWorkflowControlSetNotNullSpecification, _shiftTradeAbsenceSpecification, _shiftTradePersonalActivitySpecification, falseValidator);
			CheckResult(validator.Validate(details), false, denyReason);
        }

        private static void CheckResult(ShiftTradeRequestValidationResult result,bool expectedValue,string expectedDenyReason)
        {
            Assert.AreEqual(expectedValue, result.Value,"Value is not expected");
            Assert.AreEqual(expectedDenyReason,result.DenyReason,"Denyreason is not expected");
        }

        [Test]
        public void VerifyValidateReturnsFalseIfRequestIsNull()
        {
            IShiftTradeRequest request = null;

            IShiftTradeValidator validator = CreateValidator();
            Assert.IsFalse(validator.Validate(request).Value);

            request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>());
            Assert.IsTrue(validator.Validate(request).Value);
            
        }

        /// <summary>
        /// Stub so we dont have to mock validators, gets a little bit easier to follow
        /// </summary>
        internal class ValidatorSpecificationForTest : ShiftTradeSpecification, 
            IOpenShiftTradePeriodSpecification , 
            IShiftTradeSkillSpecification,
            IShiftTradeTargetTimeSpecification,
            IIsWorkflowControlSetNullSpecification,
			IShiftTradeAbsenceSpecification,
			IShiftTradePersonalActivitySpecification,
			IShiftTradeMeetingSpecification
        {
            private readonly bool _isSatisfiedBy;
            private readonly string _denyReason;
           

            public IList<IShiftTradeSwapDetail> HasBeenCalledWith { get; private set; }

            public ValidatorSpecificationForTest(bool isSatisfiedBy,string denyReason)
            {
                HasBeenCalledWith = null;
                _isSatisfiedBy = isSatisfiedBy;
                _denyReason = denyReason;
            }
           
            public override bool IsSatisfiedBy(IList<IShiftTradeSwapDetail> obj)
            {
                HasBeenCalledWith = obj;
                
                return _isSatisfiedBy;

            }

            public override string DenyReason
            {
                get { return _denyReason; }
            }
        }
    }
}
