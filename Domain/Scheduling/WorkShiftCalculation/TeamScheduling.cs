using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
    public interface ITeamScheduling
    {
        void Execute(IList<DateOnly> daysInBlock, IList<IScheduleMatrixPro> matrixList, IGroupPerson groupPerson, IEffectiveRestriction effectiveRestriction, IShiftProjectionCache shiftProjectionCache);
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
                IGroupPerson groupPerson,IEffectiveRestriction effectiveRestriction, IShiftProjectionCache shiftProjectionCache   )
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");

            foreach(var day in daysInBlock )
            {
                IScheduleDay  scheduleDay = null;
                foreach(var person in groupPerson.GroupMembers  )
                {
                    IPerson tmpPerson = person;
                    var tempMatrixList = matrixList.Where(scheduleMatrixPro => scheduleMatrixPro.Person == tmpPerson).ToList();
                    if(tempMatrixList.Any())
                    {
                        IScheduleMatrixPro matrixPro = tempMatrixList[0];
                        scheduleDay = matrixPro.GetScheduleDayByKey(day).DaySchedulePart();
                        scheduleDay.AddMainShift((IMainShift) shiftProjectionCache.TheMainShift.EntityClone());
                        _schedulePartModifyAndRollbackService.Modify(scheduleDay);

                    }
                    
                }
                if(scheduleDay != null)
                    _resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                   shiftProjectionCache.WorkShiftProjectionPeriod, new List<IScheduleDay> {scheduleDay });
            }
            
        }
    }
}