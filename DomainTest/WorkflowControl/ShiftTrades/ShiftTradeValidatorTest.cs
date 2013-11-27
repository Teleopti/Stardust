using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
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
		}

        private ShiftTradeValidator CreateValidator()
        {
            return new ShiftTradeValidator(shiftTradeLightValidator,new[]{_openShiftTradePeriodSpecification, 
                _shiftTradeSkillSpecification, 
                _shiftTradeTargetTimeSpecification,
                _isWorkflowControlSetNotNullSpecification,
				_shiftTradeAbsenceSpecification,
				_shiftTradePersonalActivitySpecification,
				_shiftTradeMeetingSpecification});
        }

        [Test]
		 public void ShouldCheckLightRule()
        {
        	var shiftTradeSwapDetail = new ShiftTradeSwapDetail(new Person(), new Person(), new DateOnly(), new DateOnly());
        	var result = new ShiftTradeRequestValidationResult(false);
        	shiftTradeLightValidator.Expect(m => m.Validate(null)).IgnoreArguments().Return(result);
        	var request = new ShiftTradeRequest(new []{shiftTradeSwapDetail});
        	var validator = CreateValidator();
        	validator.Validate(request).Should().Be.SameInstanceAs(result);
        }

        [Test]
        public void VerifyThatAllSpecificationsGetsCalled()
        {

            ShiftTradeValidator validator = CreateValidator();
            
            IList<IShiftTradeSwapDetail> details = new List<IShiftTradeSwapDetail>();

            validator.Validate(details);

            Assert.That(_openShiftTradePeriodSpecification.HasBeenCalledWith(details));
            Assert.That(_shiftTradeSkillSpecification.HasBeenCalledWith(details));
            Assert.That(_shiftTradeTargetTimeSpecification.HasBeenCalledWith(details));
            Assert.That(_isWorkflowControlSetNotNullSpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradeAbsenceSpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradePersonalActivitySpecification.HasBeenCalledWith(details));
			Assert.That(_shiftTradeMeetingSpecification.HasBeenCalledWith(details));
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
            validator = new ShiftTradeValidator(MockRepository.GenerateMock<IShiftTradeLightValidator>(), new[]{falseValidator, _shiftTradeSkillSpecification, _shiftTradeTargetTimeSpecification, _isWorkflowControlSetNotNullSpecification, _shiftTradeAbsenceSpecification, _shiftTradePersonalActivitySpecification, _shiftTradeMeetingSpecification});
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
        internal class ValidatorSpecificationForTest : ShiftTradeSpecification
        {
            private readonly bool _isSatisfiedBy;
            private readonly string _denyReason;
	        private IEnumerable<IShiftTradeSwapDetail> _calledWith;
			public bool HasBeenCalledWith(IEnumerable<IShiftTradeSwapDetail> details)
			{
				return details.Equals(_calledWith);
			}

            public ValidatorSpecificationForTest(bool isSatisfiedBy,string denyReason)
            {
                _isSatisfiedBy = isSatisfiedBy;
                _denyReason = denyReason;
            }

				public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
            {
				_calledWith = obj;
                
                return _isSatisfiedBy;

            }

            public override string DenyReason
            {
                get { return _denyReason; }
            }
        }
    }
}
