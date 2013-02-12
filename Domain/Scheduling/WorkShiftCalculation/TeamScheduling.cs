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
            if (shiftProjectionCache == null) return;
            if (unlockedDays == null) return;

	        foreach(var day in daysInBlock )
	        {
		        if(unlockedDays.Contains(day ))
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
						        if(matrix==null) continue;

                                destinationScheduleDay = AssignShiftProjection(startDateOfBlock, shiftProjectionCache, listOfDestinationScheduleDays, matrix, day);
					        }

				        }
                    if (destinationScheduleDay != null)
                            _resourceCalculateDelayer.CalculateIfNeeded(destinationScheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                        shiftProjectionCache.WorkShiftProjectionPeriod, listOfDestinationScheduleDays) ;
		        }
                
	        }
        }

        private IScheduleDay AssignShiftProjection(DateOnly startDateOfBlock, IShiftProjectionCache shiftProjectionCache,
                                                    List<IScheduleDay> listOfDestinationScheduleDays, IScheduleMatrixPro matrix, DateOnly day)
        {
            var scheduleDayPro = matrix.GetScheduleDayByKey(day);
            if (!matrix.UnlockedDays.Contains(scheduleDayPro)) return null;
            IScheduleDay destinationScheduleDay ;
            destinationScheduleDay = matrix.GetScheduleDayByKey(day).DaySchedulePart();
            var destinationSignificanceType = destinationScheduleDay.SignificantPart();
            if (destinationSignificanceType == SchedulePartView.DayOff ||
                destinationSignificanceType == SchedulePartView.ContractDayOff ||
                destinationSignificanceType == SchedulePartView.FullDayAbsence)
                return destinationScheduleDay;
            listOfDestinationScheduleDays.Add(destinationScheduleDay);
            var sourceScheduleDay = matrix.GetScheduleDayByKey(startDateOfBlock).DaySchedulePart();
            sourceScheduleDay.AddMainShift((IMainShift) shiftProjectionCache.TheMainShift.EntityClone());
            destinationScheduleDay.Merge(sourceScheduleDay, false);

            _schedulePartModifyAndRollbackService.Modify(destinationScheduleDay);
            return destinationScheduleDay;
        }
    }
}