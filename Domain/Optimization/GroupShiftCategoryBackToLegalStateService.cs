using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupShiftCategoryBackToLegalStateService
    {
        List<IScheduleMatrixPro> Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes, IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup);
    }

    public class GroupShiftCategoryBackToLegalStateService : IGroupShiftCategoryBackToLegalStateService
    {
        private readonly IRemoveShiftCategoryBackToLegalService _shiftCategoryBackToLegalService;
        private readonly IGroupSchedulingService _scheduleService;
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
        public GroupShiftCategoryBackToLegalStateService(IRemoveShiftCategoryBackToLegalService shiftCategoryBackToLegalService,
            IGroupSchedulingService scheduleService,
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            _shiftCategoryBackToLegalService = shiftCategoryBackToLegalService;
            _scheduleService = scheduleService;
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
        }

        public List<IScheduleMatrixPro> Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes, IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup)
        {
            var resultList = new List<IScheduleDayPro>();
            foreach (IShiftCategoryLimitation limitation in schedulePeriod.ShiftCategoryLimitationCollection())
            {
                resultList.AddRange(_shiftCategoryBackToLegalService.Execute(limitation, schedulingOptions));
            }
            var removeList = new List<IScheduleMatrixPro>();
            foreach (var scheduleDayPro in resultList)
            {
                var schedulePart = scheduleDayPro.DaySchedulePart();
                var scheduleDate = schedulePart.DateOnlyAsPeriod.DateOnly;
                var person = schedulePart.Person;
                var matrixs = groupOptimizerFindMatrixesForGroup.Find(person, scheduleDate);
                
                var memberList = new List<IScheduleMatrixPro>();
                foreach (var scheduleMatrixPro in matrixs)
                {
                    foreach (var matrix in allMatrixes)
                    {
                        if (matrix.Person == scheduleMatrixPro.Person && matrix.SchedulePeriod.DateOnlyPeriod.Contains(scheduleDate))
                        {
                            if (!removeList.Contains(matrix))
                                removeList.Add(matrix);
                            memberList.Add(matrix);
                        }
                    }
                }
                var groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(person,scheduleDate);
                
                _scheduleService.ScheduleOneDay(scheduleDate, schedulingOptions, groupPerson, allMatrixes);
            }
            return removeList;
        }
    }
}