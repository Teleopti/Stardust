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

        public TeamScheduling(IResourceCalculateDelayer  resourceCalculateDelayer)
        {
            _resourceCalculateDelayer = resourceCalculateDelayer;
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
                    var matrixPro =
                        matrixList.First(scheduleMatrixPro => scheduleMatrixPro.Person == person);
                    scheduleDay = matrixPro.GetScheduleDayByKey(day).DaySchedulePart();
                    scheduleDay.AddMainShift(shiftProjectionCache.TheMainShift);
                    
                }
                if(scheduleDay != null)
                    _resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                   shiftProjectionCache.WorkShiftProjectionPeriod, new List<IScheduleDay> {scheduleDay });
            }
            
        }
    }
}