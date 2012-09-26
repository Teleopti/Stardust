using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupShiftCategoryBackToLegalStateService
    {
    }

    public class GroupShiftCategoryBackToLegalStateService : IGroupShiftCategoryBackToLegalStateService
    {
        private readonly IRemoveShiftCategoryBackToLegalService _shiftCategoryBackToLegalService;
        private readonly IGroupSchedulingService _scheduleService;

        public GroupShiftCategoryBackToLegalStateService(IRemoveShiftCategoryBackToLegalService shiftCategoryBackToLegalService, IGroupSchedulingService scheduleService)
        {
            _shiftCategoryBackToLegalService = shiftCategoryBackToLegalService;
            _scheduleService = scheduleService;
        }
    }
}