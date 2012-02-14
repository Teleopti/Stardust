using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{


    public class OpenShiftTradePeriodSpecification : ShiftTradeSpecification, IOpenShiftTradePeriodSpecification
    {
        public override string DenyReason
        {
            get { return "OpenShiftTradePeriodDenyReason"; }
        }


        public override bool IsSatisfiedBy(IList<IShiftTradeSwapDetail> obj)
        {
            foreach (var shiftTradeDetail in obj)
            {
                IWorkflowControlSet controlSetFrom = shiftTradeDetail.PersonFrom.WorkflowControlSet;
                IWorkflowControlSet controlSetTo = shiftTradeDetail.PersonTo.WorkflowControlSet;
                if (controlSetFrom == null || controlSetTo == null)
                    return false;

                DateOnly currentDate = new DateOnly(DateTime.Now);

                DateOnlyPeriod openPeriodFrom = new DateOnlyPeriod(new DateOnly(currentDate.AddDays(controlSetFrom.ShiftTradeOpenPeriodDaysForward.Minimum)), new DateOnly(currentDate.AddDays(controlSetFrom.ShiftTradeOpenPeriodDaysForward.Maximum)));
                DateOnlyPeriod openPeriodTo = new DateOnlyPeriod(new DateOnly(currentDate.AddDays(controlSetTo.ShiftTradeOpenPeriodDaysForward.Minimum)), new DateOnly(currentDate.AddDays(controlSetTo.ShiftTradeOpenPeriodDaysForward.Maximum)));

                if (!openPeriodFrom.Contains(shiftTradeDetail.DateFrom))
                    return false;
                if (!openPeriodTo.Contains(shiftTradeDetail.DateTo))
                    return false;
            }

            return true;
        }
    }
}
