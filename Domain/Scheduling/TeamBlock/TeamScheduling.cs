using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ITeamScheduling
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void Execute(IList<DateOnly> daysInBlock, IList<IScheduleMatrixPro> matrixList, IGroupPerson groupPerson, IShiftProjectionCache shiftProjectionCache,
                     IList<DateOnly> unlockedDays, IList<IPerson> selectedPersons);
		void Execute(ITeamBlockInfo teamBlockInfo, IShiftProjectionCache shiftProjectionCache, bool skipOffset);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Levelling")]
        void ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IShiftProjectionCache shiftProjectionCache, bool useLevellingSameShift, DateOnlyPeriod selectedPeriod);
    }

    public  class TeamScheduling : ITeamScheduling
    {
        private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
	    private bool _cancelMe;

        public TeamScheduling(IResourceCalculateDelayer  resourceCalculateDelayer,ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _resourceCalculateDelayer = resourceCalculateDelayer;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
        }

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "7")]
		public void  Execute(IList<DateOnly> daysInBlock, IList<IScheduleMatrixPro> matrixList,
                IGroupPerson groupPerson, IShiftProjectionCache shiftProjectionCache, IList<DateOnly>  unlockedDays, IList<IPerson> selectedPersons)
        {
            if (matrixList == null) throw new ArgumentNullException("matrixList");
	        if (daysInBlock == null) 
				return;
            if (shiftProjectionCache == null) return;
            if (unlockedDays == null) return;
            if (selectedPersons == null) return;

			var startDateOfBlock = daysInBlock.Min();

	        foreach(var day in daysInBlock )
	        {
		        if(unlockedDays.Contains(day ))
		        {
			        IScheduleDay destinationScheduleDay = null;
		            var listOfDestinationScheduleDays = new List<IScheduleDay>();
                    if (groupPerson != null)
				        foreach (var person in groupPerson.GroupMembers)
				        {
							if (_cancelMe)
								continue;

                            if (!selectedPersons.Contains(person)) continue;
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

                                destinationScheduleDay = assignShiftProjection(startDateOfBlock, shiftProjectionCache, listOfDestinationScheduleDays, matrix, day);
								OnDayScheduled(new SchedulingServiceBaseEventArgs(destinationScheduleDay));
					        }

				        }

					if (_cancelMe)
						return;
                    if (destinationScheduleDay != null)
                            _resourceCalculateDelayer.CalculateIfNeeded(destinationScheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                        shiftProjectionCache.WorkShiftProjectionPeriod, listOfDestinationScheduleDays) ;
		        }
                
	        }
        }

		public void Execute(ITeamBlockInfo teamBlockInfo, IShiftProjectionCache shiftProjectionCache, bool skipOffset)
	    {
	        if (teamBlockInfo == null || shiftProjectionCache == null) return;
            var startDateOfBlock = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;

            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection() )
            {
	            if (skipOffset)
		            startDateOfBlock = day;

                if (teamBlockInfo.TeamInfo.MatrixesForGroup().Any(singleMatrix => singleMatrix.UnlockedDays.Any(schedulePro => schedulePro.Day == day)))
                {
                    IScheduleDay destinationScheduleDay = null;
                    var listOfDestinationScheduleDays = new List<IScheduleDay>();
                    foreach (var person in teamBlockInfo.TeamInfo.GroupPerson.GroupMembers)
                    {
                        if (_cancelMe)
                            continue;

                        //if (!selectedPersons.Contains(person)) continue;
                        IPerson tmpPerson = person;
                        var tempMatrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().Where(scheduleMatrixPro => scheduleMatrixPro.Person == tmpPerson).ToList();
                        if (tempMatrixList.Any())
                        {
                            IScheduleMatrixPro matrix = null;
                            foreach (var scheduleMatrixPro in tempMatrixList)
                            {
                                if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(startDateOfBlock))
                                    matrix = scheduleMatrixPro;
                            }
                            if (matrix == null) continue;
                            if(matrix.GetScheduleDayByKey(day).DaySchedulePart().IsScheduled()) continue; 
                            destinationScheduleDay = assignShiftProjection(startDateOfBlock, shiftProjectionCache, listOfDestinationScheduleDays, matrix, day);
                            OnDayScheduled(new SchedulingServiceBaseEventArgs(destinationScheduleDay));
                        }

                    }

                    if (_cancelMe)
                        return;
                    if (destinationScheduleDay != null)
                        _resourceCalculateDelayer.CalculateIfNeeded(destinationScheduleDay.DateOnlyAsPeriod.DateOnly,
                                                                    shiftProjectionCache.WorkShiftProjectionPeriod, listOfDestinationScheduleDays);
                }

            }
	    }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
				_cancelMe = scheduleServiceBaseEventArgs.Cancel;
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

        public void ExecutePerDayPerPerson(IPerson person, DateOnly dateOnly, ITeamBlockInfo teamBlockInfo, IShiftProjectionCache shiftProjectionCache, bool skipOffset, DateOnlyPeriod selectedPeriod)
        {
            if (teamBlockInfo == null) throw new ArgumentNullException("teamBlockInfo");
            if (shiftProjectionCache == null) throw new ArgumentNullException("shiftProjectionCache");
            var startDateOfBlock = teamBlockInfo.BlockInfo.BlockPeriod.StartDate;
            if (!selectedPeriod.DayCollection().Contains(startDateOfBlock))
                startDateOfBlock = selectedPeriod.StartDate;
            if (skipOffset)
                startDateOfBlock = dateOnly;
            var listOfDestinationScheduleDays = new List<IScheduleDay>();
            var tempMatrixList = teamBlockInfo.TeamInfo.MatrixesForGroup().Where(scheduleMatrixPro => scheduleMatrixPro.Person == person).ToList();
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