using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface ISwapAndModifyService
    {
        IEnumerable<IBusinessRuleResponse> SwapShiftTradeSwapDetails(ReadOnlyCollection<IShiftTradeSwapDetail> shiftTradeSwapDetails, IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTagSetter scheduleTagSetter);
    }
}