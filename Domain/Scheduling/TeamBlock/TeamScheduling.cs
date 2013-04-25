using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamScheduling
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        void ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IShiftProjectionCache shiftProjectionCache, bool useTeamBlockSameShift, DateOnlyPeriod selectedPeriod);
    }

    public  class TeamScheduling : ITeamScheduling
    {
        private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
	    //private bool _cancelMe;

        public TeamScheduling(IResourceCalculateDelayer  resourceCalculateDelayer,ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _resourceCalculateDelayer = resourceCalculateDelayer;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
        }

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
				//_cancelMe = scheduleServiceBaseEventArgs.Cancel;
			}
		}

        private IScheduleDay assignShiftProjection(DateOnly startDateOfBlock, IShiftProjectionCache shiftProjectionCache,
                                                    List<IScheduleDay> listOfDestinationScheduleDays, IScheduleMatrixPro matrix, DateOnly day)
        {
            var scheduleDayPro = matrix.GetScheduleDayByKey(day);
            if (!matrix.UnlockedDays.Contains(scheduleDayPro)) return null;
            //does that day count as is_scheduled??
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "4#")]
        public void ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IShiftProjectionCache shiftProjectionCache, bool skipOffset, DateOnlyPeriod selectedPeriod)
        {
            if (teamBlockInfo == null) throw new ArgumentNullException("teamBlockInfo");
            if (shiftProjectionCache == null) throw new ArgumentNullException("shiftProjectionCache");
            var startDateOfBlock = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;
            
            if (skipOffset)
                startDateOfBlock = dateOnly;
            var listOfDestinationScheduleDays = new List<IScheduleDay>();
            var tempMatrixList = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(dateOnly ).Where(scheduleMatrixPro => scheduleMatrixPro.Person == person).ToList();
            if (tempMatrixList.Any())
            {
                IScheduleMatrixPro matrix = null;
                foreach (var scheduleMatrixPro in tempMatrixList)
                {
                    if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
                       matrix = scheduleMatrixPro;
                }
                if (matrix == null) return;
                if (matrix.GetScheduleDayByKey(dateOnly).DaySchedulePart().IsScheduled()) return ;
                IScheduleDay destinationScheduleDay = assignShiftProjection(startDateOfBlock, shiftProjectionCache, listOfDestinationScheduleDays, matrix, dateOnly);
                OnDayScheduled(new SchedulingServiceBaseEventArgs(destinationScheduleDay));
                if (destinationScheduleDay != null)
                    _resourceCalculateDelayer.CalculateIfNeeded(destinationScheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                    shiftProjectionCache.WorkShiftProjectionPeriod, listOfDestinationScheduleDays);
            }
            
        }
    }
}