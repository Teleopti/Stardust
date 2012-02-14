using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{



    public class ShiftTradeValidator : IShiftTradeValidator
    {
        //todo: refact to inject a list with common interface
        private readonly IOpenShiftTradePeriodSpecification _openShiftTradePeriodSpecification;
        private readonly IShiftTradeSkillSpecification _shiftTradeSkillSpecification;
        private readonly IShiftTradeTargetTimeSpecification _shiftTradeTargetTimeSpecification;
        private readonly IIsWorkflowControlSetNullSpecification _isWorkflowControlSetNullSpecification;
    	private readonly IShiftTradeAbsenceSpecification _shiftTradeAbsenceSpecification;
    	private readonly IShiftTradePersonalActivitySpecification _shiftTradePersonalActivitySpecification;
    	private readonly IShiftTradeMeetingSpecification _shiftTradeMeetingSpecification;

        public ShiftTradeValidator(IOpenShiftTradePeriodSpecification openShiftTradePeriodSpecification,
                                    IShiftTradeSkillSpecification shiftTradeSkillSpecification,
                                    IShiftTradeTargetTimeSpecification shiftTradeTargetTimeSpecification,
                                    IIsWorkflowControlSetNullSpecification isWorkflowControlSetNullSpecification,
									IShiftTradeAbsenceSpecification shiftTradeAbsenceSpecification,
									IShiftTradePersonalActivitySpecification shiftTradePersonalActivitySpecification,
									IShiftTradeMeetingSpecification shiftTradeMeetingSpecification)
        {
            _openShiftTradePeriodSpecification = openShiftTradePeriodSpecification;
            _shiftTradeSkillSpecification = shiftTradeSkillSpecification;
            _shiftTradeTargetTimeSpecification = shiftTradeTargetTimeSpecification;
            _isWorkflowControlSetNullSpecification = isWorkflowControlSetNullSpecification;
        	_shiftTradeAbsenceSpecification = shiftTradeAbsenceSpecification;
        	_shiftTradePersonalActivitySpecification = shiftTradePersonalActivitySpecification;
        	_shiftTradeMeetingSpecification = shiftTradeMeetingSpecification;
        }

        public ShiftTradeRequestValidationResult Validate(IList<IShiftTradeSwapDetail> shiftTradeDetails)
        {
            var result = _isWorkflowControlSetNullSpecification.Validate(shiftTradeDetails);
            if (!result.Value) return result;

            result = _openShiftTradePeriodSpecification.Validate(shiftTradeDetails);
            if (!result.Value) return result;

			result = _shiftTradeAbsenceSpecification.Validate(shiftTradeDetails);
			if (!result.Value) return result;

        	result = _shiftTradePersonalActivitySpecification.Validate(shiftTradeDetails);
			if (!result.Value) return result;

        	result = _shiftTradeMeetingSpecification.Validate(shiftTradeDetails);
			if (!result.Value) return result;

            result = _shiftTradeTargetTimeSpecification.Validate(shiftTradeDetails);
            if (!result.Value) return result;

            result = _shiftTradeSkillSpecification.Validate(shiftTradeDetails);
            if (!result.Value) return result;

           return  new ShiftTradeRequestValidationResult(true);
        }

        public ShiftTradeRequestValidationResult Validate(IShiftTradeRequest shiftTradeRequest)
        {
            if (new IsShiftTradeRequestNotNullSpecification().IsSatisfiedBy(shiftTradeRequest))
            {
                return Validate(shiftTradeRequest.ShiftTradeSwapDetails);
            }
            return new ShiftTradeRequestValidationResult(false);
        }
    }
}
