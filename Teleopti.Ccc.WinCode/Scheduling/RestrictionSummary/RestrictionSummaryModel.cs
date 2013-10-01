using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class RestrictionSummaryModel
    {
        private CultureInfo _currentCultureInfo;
        private CultureInfo _currentUICultureInfo;
        private Dictionary<int, IPreferenceCellData> _cellDataCollection;
        private readonly ISchedulingResultStateHolder _scheduleResultStateHolder;
    	private readonly IWorkShiftWorkTime _workShiftWorkTime;
    	private DateTimePeriod _schedulerLoadedPeriod;
        private readonly ISchedulerStateHolder _schedulerState;
    	private readonly IPreferenceNightRestChecker _preferenceNightRestChecker;

    	public RestrictionSummaryModel(ISchedulingResultStateHolder scheduleResultStateHolder, IWorkShiftWorkTime workShiftWorkTime, 
			ISchedulerStateHolder schedulerState, IPreferenceNightRestChecker preferenceNightRestChecker)
        {
            _scheduleResultStateHolder = scheduleResultStateHolder;
    		_workShiftWorkTime = workShiftWorkTime;
    		_cellDataCollection = new Dictionary<int, IPreferenceCellData>();
            _schedulerState = schedulerState;
        	_preferenceNightRestChecker = preferenceNightRestChecker;
        	loggedOnPersonCultures(TeleoptiPrincipal.Current.Regional);
        }

        private void loggedOnPersonCultures(IRegional regional)
        {
            _currentUICultureInfo = regional.UICulture;
            _currentCultureInfo = regional.Culture;
        }

        public Dictionary<int, IPreferenceCellData> CellDataCollection
        {
            get { return _cellDataCollection; }
        }

        public DateTimePeriod SchedulerLoadedPeriod
        {
            get 
            {
                return _schedulerLoadedPeriod;
            }
            set 
            {
                _schedulerLoadedPeriod = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void LoadPeriod(AgentInfoHelper agentInfoHelper)
        {
            var retList = new Dictionary<int, IPreferenceCellData>();
			_cellDataCollection = retList;
			if (agentInfoHelper == null)
				return;

            int cellNumber = 0;

            IList<DateOnly> custmizedDayCollection = CustomizedDayCollection(agentInfoHelper);
            foreach (DateOnly dateOnly in custmizedDayCollection)
            {
                IScheduleDay schedulePart = _scheduleResultStateHolder.Schedules[agentInfoHelper.Person].ScheduledDay(dateOnly);
                IPreferenceCellData cellData = new PreferenceCellData
                                               	{
                                               		SchedulePart = schedulePart,
                                               		TheDate = schedulePart.DateOnlyAsPeriod.DateOnly,
                                               		Enabled =
                                               			_schedulerLoadedPeriod.Contains(
                                               				schedulePart.DateOnlyAsPeriod.DateOnly.Date)  &&  agentInfoHelper.Period.Value.Contains(dateOnly)
                                               	};

            	var extractor = new RestrictionExtractor(_scheduleResultStateHolder);
                extractor.Extract(agentInfoHelper.Person, dateOnly);

                IEffectiveRestriction totalRestriction = extractor.CombinedRestriction(agentInfoHelper.SchedulingOptions);
                var significantPart = schedulePart.SignificantPart();

                if (((RestrictionSchedulingOptions)agentInfoHelper.SchedulingOptions).UseScheduling)
                {
                    if (significantPart == SchedulePartView.MainShift)
                    {
                        totalRestriction = CellDataForMainShift(cellData, schedulePart);
                    }
                    else if (significantPart == SchedulePartView.DayOff)
                    {
                        totalRestriction = CellDataForDayOff(cellData, schedulePart);
                    }
                    else if (significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff)
                    {
                        totalRestriction = CellDataForFullDayAbsence(agentInfoHelper, cellData, schedulePart);
                    }
                }
                if (totalRestriction != null)
                {
                    IWorkTimeMinMax minMaxLength = GetMinMaxLength(agentInfoHelper, dateOnly, totalRestriction);
                    if (minMaxLength != null)
                    {
                        totalRestriction = new EffectiveRestriction(minMaxLength.StartTimeLimitation,
                                                                                        minMaxLength.EndTimeLimitation,
                                                                                        minMaxLength.WorkTimeLimitation,
                                                                                        totalRestriction.ShiftCategory,
                                                                                        totalRestriction.DayOffTemplate,
                                                                                        totalRestriction.Absence
                                                                                        , new List<IActivityRestriction>());

                    }
                    else
                    {
                        if (totalRestriction.IsRestriction && totalRestriction.DayOffTemplate == null && totalRestriction.Absence == null)
                        {
                            cellData.NoShiftsCanBeFound = true;
                            totalRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                        new EndTimeLimitation(),
                                                                                        new WorkTimeLimitation(),
                                                                                        totalRestriction.ShiftCategory,
                                                                                        totalRestriction.DayOffTemplate,
                                                                                        totalRestriction.Absence
                                                                                        , new List<IActivityRestriction>());
                        }
                    }
                    cellData.EffectiveRestriction = totalRestriction;
                    if (totalRestriction.Absence != null)
                        cellDataForPreferredAbsence(agentInfoHelper, cellData);
                }

                cellData.PeriodTarget = agentInfoHelper.SchedulePeriodTargetTime;
                cellData.SchedulingOption = (RestrictionSchedulingOptions)agentInfoHelper.SchedulingOptions;
				cellData.NightlyRest = new TimeSpan(0);
            	bool foundMustHave = false;
            	foreach (var preferenceRestriction in extractor.PreferenceList)
            	{
            		if (preferenceRestriction.MustHave)
            		{
            			foundMustHave = true;
            		}
            	}
            	cellData.MustHavePreference = foundMustHave;
	            var personPeriod = agentInfoHelper.Person.Period(agentInfoHelper.SelectedDate);
	            if (personPeriod == null)
                    cellData.WeeklyMax = TimeSpan.Zero;
                else
                {
                	var worktimeDirective = personPeriod.PersonContract.Contract.WorkTimeDirective;
					cellData.WeeklyMax = worktimeDirective.MaxTimePerWeek;
					cellData.NightlyRest = worktimeDirective.NightlyRest;
                }

                retList.Add(cellNumber, cellData);
                cellNumber++;
            }
			// call to check nightly rest
        	_preferenceNightRestChecker.CheckNightlyRest(retList);
            _cellDataCollection = retList;
        }

        private void cellDataForPreferredAbsence(AgentInfoHelper agentInfoHelper, IPreferenceCellData cellData)
        {
			DateOnly cellDate = cellData.TheDate;

            IVirtualSchedulePeriod virtualSchedulePeriod =
				agentInfoHelper.Person.VirtualSchedulePeriod(cellDate);

			if (!virtualSchedulePeriod.IsValid)
				return;

        	IPerson person = agentInfoHelper.Person;
			IPersonPeriod personPeriod = person.Period(cellDate);
			if (personPeriod == null)
				return;

			DateOnly? schedulePeriodStartDate = person.SchedulePeriodStartDate(cellDate);
			if(!schedulePeriodStartDate.HasValue)
				return;

            cellData.HasAbsenceOnContractDayOff =
				!virtualSchedulePeriod.ContractSchedule.IsWorkday(schedulePeriodStartDate.Value, cellDate);

            TimeSpan time = TimeSpan.Zero;
            if (!cellData.HasAbsenceOnContractDayOff && cellData.EffectiveRestriction.Absence.InContractTime)
                time = virtualSchedulePeriod.AverageWorkTimePerDay;

            cellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(time, CurrentCultureInfo());
            var totalRestriction = new EffectiveRestriction(cellData.EffectiveRestriction.StartTimeLimitation,
                                                                                        cellData.EffectiveRestriction.EndTimeLimitation,
                                                                                        new WorkTimeLimitation(time, time),
                                                                                        cellData.EffectiveRestriction.ShiftCategory,
                                                                                        cellData.EffectiveRestriction.DayOffTemplate,
                                                                                        cellData.EffectiveRestriction.Absence
                                                                                        , new List<IActivityRestriction>());
            cellData.EffectiveRestriction = totalRestriction;
        }

        private IEffectiveRestriction CellDataForFullDayAbsence(AgentInfoHelper agentInfoHelper, IPreferenceCellData cellData, IScheduleDay scheduleDay)
        {
        	var projection = scheduleDay.ProjectionService().CreateProjection();
            TimeSpan timeSpan = projection.Period().Value.ElapsedTime();
            var startTimeLimitation = new StartTimeLimitation(null, null);
            var endTimeLimitation = new EndTimeLimitation(null, null);
        	DateOnly cellDate = cellData.TheDate;

			IVirtualSchedulePeriod virtualSchedulePeriod =
				agentInfoHelper.Person.VirtualSchedulePeriod(cellDate);
			DateOnly? schedulePeriodStart = agentInfoHelper.Person.SchedulePeriodStartDate(cellDate);
			if (virtualSchedulePeriod.IsValid && schedulePeriodStart.HasValue)
            {
                cellData.HasAbsenceOnContractDayOff =
					!virtualSchedulePeriod.ContractSchedule.IsWorkday(schedulePeriodStart.Value, cellDate);
            }
            
            WorkTimeLimitation workTimeLimitation;
            if(cellData.HasAbsenceOnContractDayOff)
            {
                workTimeLimitation = new WorkTimeLimitation(TimeSpan.Zero, TimeSpan.Zero);
            }
            else
            {
                workTimeLimitation = new WorkTimeLimitation(timeSpan, timeSpan);
            }

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                              new WorkTimeLimitation(), null, null, null,
                                                                              new List<IActivityRestriction>());
            totalRestriction =
                totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null,
                                                                  null, null, new List<IActivityRestriction>()));

        	var description =
        		scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDescription(scheduleDay.Person,cellDate);
            cellData.DisplayName = description.Name;
            cellData.DisplayShortName = description.ShortName;
            cellData.DisplayColor = scheduleDay.PersonAbsenceCollection()[0].Layer.Payload.ConfidentialDisplayColor(scheduleDay.Person,cellDate);
            cellData.HasFullDayAbsence = true;
            cellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(), CurrentCultureInfo());
            cellData.StartEndScheduledShift = projection.Period().Value.TimePeriod(_schedulerState.TimeZoneInfo).ToShortTimeString(CurrentCultureInfo());
            
            return totalRestriction;
        }

        private IEffectiveRestriction CellDataForDayOff(IPreferenceCellData cellData, IScheduleDay schedulePart)
        {
        	var startTimeLimitation = new StartTimeLimitation(null, null);
            var endTimeLimitation = new EndTimeLimitation(null, null);
            var workTimeLimitation = new WorkTimeLimitation(null, null);
	        var dayOff = schedulePart.PersonAssignment().DayOff();
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(dayOff.Description);
            dayOffTemplate.SetTargetAndFlexibility(dayOff.TargetLength, dayOff.Flexibility);

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                              new WorkTimeLimitation(), null, null, null,
                                                                              new List<IActivityRestriction>());
                        
            totalRestriction =
                totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null,
                                                                  dayOffTemplate, null, new List<IActivityRestriction>()));
            cellData.DisplayName = dayOff.Description.Name;
            cellData.DisplayShortName = dayOff.Description.ShortName;
            cellData.HasDayOff = true;
            cellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(dayOffTemplate.TargetLength, CurrentCultureInfo());
            return totalRestriction;
        }

        private IEffectiveRestriction CellDataForMainShift(IPreferenceCellData cellData, IScheduleDay schedulePart)
        {
        	var projection = schedulePart.ProjectionService().CreateProjection();
            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
                                                                              new WorkTimeLimitation(), null, null, null,
                                                                              new List<IActivityRestriction>());
            totalRestriction = GetMainShiftTotalRestriction(schedulePart, totalRestriction);
	        var assignment = schedulePart.PersonAssignment();
            cellData.HasShift = true;
            cellData.DisplayName =
                assignment.ShiftCategory.Description.Name;
            cellData.DisplayShortName =
								assignment.ShiftCategory.Description.ShortName;
            cellData.DisplayColor =
								assignment.ShiftCategory.DisplayColor;
            cellData.ShiftLengthScheduledShift = TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(), CurrentCultureInfo());
            cellData.StartEndScheduledShift = projection.Period().Value.TimePeriod(_schedulerState.TimeZoneInfo).ToShortTimeString(CurrentCultureInfo());
            return totalRestriction;
        }

        public IList<DateOnly> CustomizedDayCollection(AgentInfoHelper agentInfoHelper)
        {
            DateOnly firstDate = agentInfoHelper.Period.Value.StartDate;

            DateTime firstDateTime = DateHelper.GetFirstDateInWeek(firstDate.Date, CurrentCultureInfo());
            DateOnly lastDate = agentInfoHelper.Period.Value.EndDate;
            DateTime lastDateTime = DateHelper.GetLastDateInWeek(lastDate.Date, CurrentCultureInfo());
            var start = new DateOnly(firstDateTime).AddDays(-7);
            var end = new DateOnly(lastDateTime).AddDays(7);
            var dateOnlyPeriod = new DateOnlyPeriod(start, end);
            return dateOnlyPeriod.DayCollection();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public IWorkTimeMinMax GetMinMaxLength(AgentInfoHelper agentInfoHelper, DateOnly dateOnly, IEffectiveRestriction totalRestriction)
        {
            IPersonPeriod personPeriod = agentInfoHelper.Person.Period(dateOnly);
            if (personPeriod == null)
                return null;
            IRuleSetBag ruleSetBag = personPeriod.RuleSetBag;
            if (ruleSetBag == null)
                return null;

				return ruleSetBag.MinMaxWorkTime(_workShiftWorkTime, dateOnly, totalRestriction);
        }

        public static IEffectiveRestriction GetMainShiftTotalRestriction(IScheduleDay schedulePart, IEffectiveRestriction totalRestriction)
        {
            var proj = schedulePart.ProjectionService().CreateProjection();
            DateTimePeriod dateTimePeriod = proj.Period().GetValueOrDefault();
            var zone = schedulePart.Person.PermissionInformation.DefaultTimeZone();

            DateTime viewLocalTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.StartDateTime, zone);
            DateTime viewLocalEndTime = TimeZoneHelper.ConvertFromUtc(dateTimePeriod.EndDateTime, zone);

            var startTimeLimitation = new StartTimeLimitation(viewLocalTime.TimeOfDay, viewLocalTime.TimeOfDay);

	        var endTimeOfDay = viewLocalEndTime.Subtract(viewLocalTime.Date);
            var endTimeLimitation = new EndTimeLimitation(endTimeOfDay, endTimeOfDay);

	        var contractTime = proj.ContractTime();
            var workTimeLimitation = new WorkTimeLimitation(contractTime, contractTime);

            if (totalRestriction == null)
            {
                totalRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation,
                                                            null, null, null, new List<IActivityRestriction>());
            }
            else
            {
                totalRestriction =
                    totalRestriction.Combine(new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation, null,
                                                                      null, null, new List<IActivityRestriction>()));
            }
            return totalRestriction;
        }

        public static bool DecideIfExtendedPreference(IEffectiveRestriction restriction)
        {
            return (restriction.WorkTimeLimitation.HasValue() || restriction.StartTimeLimitation.HasValue() || restriction.EndTimeLimitation.HasValue() || restriction.ActivityRestrictionCollection.Count > 0);
        }
        public TimePeriod CurrentPeriodTime()
        {
            var minTime = new TimeSpan();
            var maxTime = new TimeSpan();

            foreach (IPreferenceCellData preferenceCellData in _cellDataCollection.Values)
            {
                if (!preferenceCellData.IsInsidePeriod)
                    continue;
                if (preferenceCellData.EffectiveRestriction != null)
                {
                    if (preferenceCellData.EffectiveRestriction.WorkTimeLimitation.HasValue())
                    {
                        minTime = minTime.Add(preferenceCellData.EffectiveRestriction.WorkTimeLimitation.StartTime.Value);
                        maxTime = maxTime.Add(preferenceCellData.EffectiveRestriction.WorkTimeLimitation.EndTime.Value);
                    }
                }
            }
            return new TimePeriod(minTime, maxTime);
        }

        public TimeSpan PeriodTargetTime()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (IPreferenceCellData preferenceCellData in _cellDataCollection.Values)
                {
                    return preferenceCellData.PeriodTarget;
                }
            }
            return TimeSpan.Zero;
        }
        public bool PeriodIsValid()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (IPreferenceCellData preferenceCellData in _cellDataCollection.Values)
                {
                    if (preferenceCellData.Enabled)
                    {
                        //if (preferenceCellData.EffectiveRestriction.Invalid)
                        return false;
                    }
                }
            }
            return true;
        }

        public void GetNextPeriod(AgentInfoHelper agentInfoHelper)
        {
                LoadPeriod(agentInfoHelper);
        }
        
        public CultureInfo CurrentCultureInfo()
        {
            return _currentCultureInfo;
        }

        public CultureInfo CurrentUICultureInfo()
        {
            return _currentUICultureInfo;
        }
    }
}
