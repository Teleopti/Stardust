using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface ITeamScheduling
    {
        void Execute(DateOnly startDateOfBlock, IList<DateOnly> daysInBlock, IList<IScheduleMatrixPro> matrixList, IGroupPerson groupPerson,
                     IEffectiveRestriction effectiveRestriction, IShiftProjectionCache shiftProjectionCache,
                     IList<DateOnly> unlockedDays);
    }

    public  class TeamScheduling : ITeamScheduling
    {
        private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;

        public TeamScheduling(IResourceCalculateDelayer  resourceCalculateDelayer,ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _resourceCalculateDelayer = resourceCalculateDelayer;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
        }


        public void  Execute(DateOnly startDateOfBlock, IList<DateOnly  > daysInBlock, IList<IScheduleMatrixPro> matrixList,
                IGroupPerson groupPerson,IEffectiveRestriction effectiveRestriction, IShiftProjectionCache shiftProjectionCache, IList<DateOnly>  unlockedDays)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
	        if (daysInBlock == null) 
				return;

	        foreach(var day in daysInBlock )
	        {
		        if(unlockedDays != null && unlockedDays.Contains(day ))
		        {
			        IScheduleDay destinationScheduleDay = null;
		            var listOfDestinationScheduleDays = new List<IScheduleDay>();
                    if (groupPerson != null)
				        foreach (var person in groupPerson.GroupMembers)
				        {
					        IPerson tmpPerson = person;
					        var tempMatrixList = matrixList.Where(scheduleMatrixPro => scheduleMatrixPro.Person == tmpPerson).ToList();
					        if (tempMatrixList.Any())
					        {
					            IScheduleMatrixPro matrix = null;
					            foreach (var scheduleMatrixPro in tempMatrixList)
					            {
                                    if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(startDateOfBlock))
                                        matrix = scheduleMatrixPro;

					            }
						        
                                destinationScheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
                                listOfDestinationScheduleDays.Add(destinationScheduleDay );
                                destinationScheduleDay.Merge(matrix.GetScheduleDayByKey(startDateOfBlock).DaySchedulePart(),false );
                                if (shiftProjectionCache != null)
                                    destinationScheduleDay.AddMainShift((IMainShift)shiftProjectionCache.TheMainShift.EntityClone());
						        _schedulePartModifyAndRollbackService.Modify(destinationScheduleDay);
                                
					        }

				        }
                    if (destinationScheduleDay != null)
                        if (shiftProjectionCache != null)
                            _resourceCalculateDelayer.CalculateIfNeeded(destinationScheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                        shiftProjectionCache.WorkShiftProjectionPeriod, listOfDestinationScheduleDays) ;
		        }
                
	        }
        }
    }
}