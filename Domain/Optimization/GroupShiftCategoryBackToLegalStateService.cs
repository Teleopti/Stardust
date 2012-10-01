using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupShiftCategoryBackToLegalStateService
    {
        bool Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes);
    }

    public class GroupShiftCategoryBackToLegalStateService : IGroupShiftCategoryBackToLegalStateService
    {
        private readonly IGroupRemoveShiftCategoryBackToLegalService _shiftCategoryBackToLegalService;
        private readonly IGroupSchedulingService _scheduleService;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;

        public GroupShiftCategoryBackToLegalStateService(IGroupRemoveShiftCategoryBackToLegalService shiftCategoryBackToLegalService,
            IGroupSchedulingService scheduleService,
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _shiftCategoryBackToLegalService = shiftCategoryBackToLegalService;
            _scheduleService = scheduleService;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
        }

        public bool Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions,IList<IScheduleMatrixPro> allMatrixes)
        {
            var result = true;
            var resultList = new List<IScheduleDayPro>();
            foreach (IShiftCategoryLimitation limitation in schedulePeriod.ShiftCategoryLimitationCollection())
            {
                resultList.AddRange(_shiftCategoryBackToLegalService.Execute(limitation, schedulingOptions));
            }

            foreach (var scheduleDayPro in resultList)
            {
                var schedulePart = scheduleDayPro.DaySchedulePart();
                var scheduleDate = schedulePart.DateOnlyAsPeriod.DateOnly;
                var person = schedulePart.Person;
                var groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(person,scheduleDate);
                result = result & _scheduleService.ScheduleOneDayOnOnePerson(scheduleDate, person, schedulingOptions, groupPerson,
                                                                    allMatrixes);
            }
            return result;
        }
    }
}