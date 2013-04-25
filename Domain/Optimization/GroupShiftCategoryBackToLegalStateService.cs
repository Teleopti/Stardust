using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupShiftCategoryBackToLegalStateService
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		List<IScheduleMatrixPro> Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes, IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup, ITeamSteadyStateHolder teamSteadyStateHolder, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization);
    }

    public class GroupShiftCategoryBackToLegalStateService : IGroupShiftCategoryBackToLegalStateService
    {
        private readonly IRemoveShiftCategoryBackToLegalService _shiftCategoryBackToLegalService;
        private readonly IGroupSchedulingService _scheduleService;
        private readonly IGroupPersonsBuilder _groupPersonsBuilder;

        public GroupShiftCategoryBackToLegalStateService(IRemoveShiftCategoryBackToLegalService shiftCategoryBackToLegalService,
            IGroupSchedulingService scheduleService,
            IGroupPersonsBuilder groupPersonsBuilder)
        {
            _shiftCategoryBackToLegalService = shiftCategoryBackToLegalService;
            _scheduleService = scheduleService;
            _groupPersonsBuilder = groupPersonsBuilder;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public List<IScheduleMatrixPro> Execute(IVirtualSchedulePeriod schedulePeriod, ISchedulingOptions schedulingOptions, IList<IScheduleMatrixPro> allMatrixes, IGroupOptimizerFindMatrixesForGroup groupOptimizerFindMatrixesForGroup, ITeamSteadyStateHolder teamSteadyStateHolder, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, IScheduleDictionary scheduleDictionary, ISchedulePartModifyAndRollbackService rollbackService, IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
        {
            var resultList = new List<IScheduleDayPro>();
            if (schedulePeriod != null)
                foreach (IShiftCategoryLimitation limitation in schedulePeriod.ShiftCategoryLimitationCollection())
                {
                    resultList.AddRange(_shiftCategoryBackToLegalService.Execute(limitation, schedulingOptions));
                }
            var removeList = new List<IScheduleMatrixPro>();
            if (resultList.Count == 0)
                removeList.Add(_shiftCategoryBackToLegalService.ScheduleMatrixPro);
            var groupMatrixs = new List<IScheduleMatrixPro>();
            foreach (var scheduleDayPro in resultList)
            {
            	var dateOnly = scheduleDayPro.Day;
                var schedulePart = scheduleDayPro.DaySchedulePart();
                var scheduleDate = schedulePart.DateOnlyAsPeriod.DateOnly;
                var person = schedulePart.Person;
                if (groupOptimizerFindMatrixesForGroup != null)
                {
                    groupMatrixs = groupOptimizerFindMatrixesForGroup.Find(person, scheduleDate).ToList();

                    foreach (var scheduleMatrixPro in groupMatrixs)
                    {
                        if (allMatrixes != null)
                            foreach (var matrix in allMatrixes)
                            {
                                if (matrix.Person == scheduleMatrixPro.Person && matrix.SchedulePeriod.DateOnlyPeriod.Contains(scheduleDate))
                                {
                                    if (!removeList.Contains(matrix))
                                        removeList.Add(matrix);
                                }
                            }
                    }
                }
                var groupPerson = _groupPersonsBuilder.BuildListOfGroupPersons(scheduleDate, groupMatrixs.Select(x => x.Person).ToList(), true, schedulingOptions).FirstOrDefault();

				var teamSteadyStateSuccess = false;

				if(teamSteadyStateHolder.IsSteadyState(groupPerson))
				{
					if (teamSteadyStateMainShiftScheduler != null && !teamSteadyStateMainShiftScheduler.ScheduleTeam(dateOnly, groupPerson, _scheduleService, rollbackService,schedulingOptions, groupPersonBuilderForOptimization,allMatrixes, scheduleDictionary))
					{
						teamSteadyStateHolder.SetSteadyState(groupPerson, false);
					}

					else teamSteadyStateSuccess = true;	
				}

            	if(!teamSteadyStateSuccess)
					_scheduleService.ScheduleOneDay(scheduleDate, schedulingOptions, groupPerson, allMatrixes);
            }
            return removeList;
        }
    }
}