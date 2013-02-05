using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface ITeamScheduling
    {
        void Execute(IList<DateOnly> daysInBlock, IList<IScheduleMatrixPro> matrixList, IGroupPerson groupPerson,
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


        public void  Execute(IList<DateOnly  > daysInBlock, IList<IScheduleMatrixPro> matrixList,
                IGroupPerson groupPerson,IEffectiveRestriction effectiveRestriction, IShiftProjectionCache shiftProjectionCache, IList<DateOnly>  unlockedDays)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");

            if (daysInBlock != null)
                foreach(var day in daysInBlock )
                {
                    if(unlockedDays != null && unlockedDays.Contains(day ))
                    {
                        IScheduleDay scheduleDay = null;
                        if (groupPerson != null)
                            foreach (var person in groupPerson.GroupMembers)
                            {
                                IPerson tmpPerson = person;
                                var tempMatrixList = matrixList.Where(scheduleMatrixPro => scheduleMatrixPro.Person == tmpPerson).ToList();
                                if (tempMatrixList.Any())
                                {
                                    IScheduleMatrixPro matrixPro = tempMatrixList[0];
                                    scheduleDay = matrixPro.GetScheduleDayByKey(day).DaySchedulePart();
                                    if (shiftProjectionCache != null)
                                        scheduleDay.AddMainShift((IMainShift)shiftProjectionCache.TheMainShift.EntityClone());
                                    _schedulePartModifyAndRollbackService.Modify(scheduleDay);

                                }

                            }
                        if (scheduleDay != null)
                            if (shiftProjectionCache != null)
                                _resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                            shiftProjectionCache.WorkShiftProjectionPeriod, new List<IScheduleDay> { scheduleDay });
                    }
                
                }
        }
    }
}