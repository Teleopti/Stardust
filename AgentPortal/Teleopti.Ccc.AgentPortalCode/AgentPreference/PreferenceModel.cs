using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IPreferenceModel
    {
        Dictionary<int, IPreferenceCellData> CellDataCollection { get; }
        IList<ShiftCategory> ShiftCategories { get; }
        IList<Common.DayOff> DaysOff { get; }
        IList<Absence> Absences { get; }
        DateTime FirstDateCurrentPeriod { get; }
        int NumberOfMustHaveCurrentPeriod { get; }
        int MaxMustHaveCurrentPeriod { get; }
        PersonDto LoggedOnPerson { get; }
        string CalculationInfo { get; }
        void LoadPeriod(DateTime dateInPeriod, IScheduleHelper scheduleHelper);
        DateOnly GetDateOnlyInPeriod(DateTime dateInPeriod);
        void AddMustHave();
        void RemoveMustHave();
        TimePeriod CurrentPeriodTime();
        TimeSpan PeriodTargetTime();
        TimePeriod BalancedTargetTimeWithTolerance();
        int PeriodTargetDayOffs();
        int PeriodDayOffs();
        bool PeriodIsValid();
        void GetPreviousPeriod();
        void GetNextPeriod();
        void ReloadPeriod();
        CultureInfo CurrentCultureInfo();
        CultureInfo CurrentUICultureInfo();
    }

    public class PreferenceModel : IPreferenceModel
    {
        private DateTime _lastDate;
        private DateTime _firstDateInCurrentPeriod;
        private readonly IList<ShiftCategory> _shiftCategories = new List<ShiftCategory>();
        private readonly IList<Common.DayOff> _daysOff = new List<Common.DayOff>();
        private readonly  IList<Absence> _absences = new List<Absence>();
        private CultureInfo _currentCultureInfo;
        private CultureInfo _currentUiCultureInfo;
        private bool _didGetPeriodLastCall;
        private Dictionary<int, IPreferenceCellData> _cellDataCollection;
        private static readonly TimeLengthValidator TimeLengthValidator = new TimeLengthValidator();
        private static readonly TimeOfDayValidator TimeOfDayValidatorStartTime = new TimeOfDayValidator(false);
        private static readonly TimeOfDayValidator TimeOfDayValidatorEndTime = new TimeOfDayValidator(true);
        private int _maxMustHaveCurrentPeriod;
        private int _numberOfMustHaveCurrentPeriod;
        private readonly PersonDto _loggedOnPerson;
        private readonly IScheduleHelper _scheduleHelper;
        private string _calculationInfo;

        public PreferenceModel(PersonDto loggedOnPerson, IScheduleHelper scheduleHelper)
        {
            _cellDataCollection = new Dictionary<int, IPreferenceCellData>();
            _lastDate = DateTime.Today;
            _loggedOnPerson = loggedOnPerson;
            SetCultures();
            _scheduleHelper = scheduleHelper;
            //loadShiftcategoriesAndDaysOff();
        }

        private void SetCultures()
        {
            _currentUiCultureInfo = (LoggedOnPerson.UICultureLanguageId.HasValue
                                        ? CultureInfo.GetCultureInfo(LoggedOnPerson.UICultureLanguageId.Value)
                                        : CultureInfo.CurrentUICulture).FixPersianCulture();
            _currentCultureInfo = (LoggedOnPerson.CultureLanguageId.HasValue
                                      ? CultureInfo.GetCultureInfo(LoggedOnPerson.CultureLanguageId.Value)
                                      : CultureInfo.CurrentCulture).FixPersianCulture();

        }

        public Dictionary<int, IPreferenceCellData> CellDataCollection
        {
            get { return _cellDataCollection; }
        }

        public IList<ShiftCategory> ShiftCategories
        {
            get { return _shiftCategories; }
        }

        public IList<Common.DayOff> DaysOff
        {
            get { return _daysOff; }
        }

        public IList<Absence> Absences
        {
            get { return _absences; }
        }

        public DateTime FirstDateCurrentPeriod
        {
            get { return _firstDateInCurrentPeriod; }
        }

        public int NumberOfMustHaveCurrentPeriod
        {
            get { return _numberOfMustHaveCurrentPeriod; }
        }

        public int MaxMustHaveCurrentPeriod
        {
            get { return _maxMustHaveCurrentPeriod; }
        }

        public PersonDto LoggedOnPerson
        {
            get { return _loggedOnPerson; }
        }

        public string CalculationInfo
        {
            get {
                if ( string.IsNullOrEmpty(_calculationInfo) )
                {
                    if (_cellDataCollection.Count > 0)
                    {
                        var cellData = _cellDataCollection[0];
                        
                        int periodTargetMinutes = (int)cellData.PeriodTarget.TotalMinutes;
                        TimeSpan seasonality = TimeSpan.FromMinutes(Math.Round(periodTargetMinutes*cellData.Seasonality));

                        _calculationInfo = string.Format(CurrentUICultureInfo(), Resources.PreferenceViewCalculationInfo,
                                          TimeHelper.GetLongHourMinuteTimeString(cellData.PeriodTarget, CurrentCultureInfo()),
                                          TimeHelper.GetLongHourMinuteTimeString(seasonality, '+', CurrentCultureInfo()),
                                          TimeHelper.GetLongHourMinuteTimeString(cellData.BalanceIn, '-', CurrentCultureInfo()),
                                          TimeHelper.GetLongHourMinuteTimeString(cellData.Extra, '+', CurrentCultureInfo()),
                                          TimeHelper.GetLongHourMinuteTimeString(cellData.BalanceOut, '+', CurrentCultureInfo()),
                                          TimeHelper.GetLongHourMinuteTimeString(cellData.BalancedPeriodTarget, CurrentCultureInfo()));
                    }
                }
                return _calculationInfo;
                }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void LoadPeriod(DateTime dateInPeriod, IScheduleHelper scheduleHelper)
        {
            _maxMustHaveCurrentPeriod = 0;
            _numberOfMustHaveCurrentPeriod = 0;
            _calculationInfo = "";
            DateOnly dateOnly = GetDateOnlyInPeriod(dateInPeriod);

            var ret = scheduleHelper.Validate(LoggedOnPerson, dateOnly, false);
            var retList = new Dictionary<int, IPreferenceCellData>();
            int cellNumber = 0;
            bool firstDateIsSet = false;
            _didGetPeriodLastCall = false;
            foreach (ValidatedSchedulePartDto validatedSchedulePartDto in ret)
            {
                _didGetPeriodLastCall = true;
                var cellData = new PreferenceCellData
                                   {
                                       TheDate = validatedSchedulePartDto.DateOnly.DateTime
                                   };

                if (validatedSchedulePartDto.PreferenceRestriction != null)
                {
                    cellData.Preference = new Preference();

                    if (validatedSchedulePartDto.PreferenceRestriction.ShiftCategory != null)
                    {
                        Color shiftCategoryColor =
                            ColorHelper.CreateColorFromDto(
                                validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.DisplayColor);
                        cellData.Preference.ShiftCategory =
                            new ShiftCategory(validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.Name, "",
                                              validatedSchedulePartDto.PreferenceRestriction.ShiftCategory.Id.GetValueOrDefault(),
                                              shiftCategoryColor);
                    }
                    if (validatedSchedulePartDto.PreferenceRestriction.DayOff != null)
                    {
                        cellData.Preference.DayOff =
                            new Common.DayOff(
                                validatedSchedulePartDto.PreferenceRestriction.DayOff.Name, "",
                                validatedSchedulePartDto.PreferenceRestriction.DayOff.Id.GetValueOrDefault(), Color.DimGray);
                    }

                    if(validatedSchedulePartDto.PreferenceRestriction.Absence != null)
                    {
                        var absenceColor = ColorHelper.CreateColorFromDto(validatedSchedulePartDto.PreferenceRestriction.Absence.DisplayColor);
                        cellData.Preference.Absence = new Absence(validatedSchedulePartDto.PreferenceRestriction.Absence.Name, 
                            validatedSchedulePartDto.PreferenceRestriction.Absence.ShortName,
                            validatedSchedulePartDto.PreferenceRestriction.Absence.Id.GetValueOrDefault(), absenceColor);    
                    }

                    if (validatedSchedulePartDto.PreferenceRestriction.ActivityRestrictionCollection != null && validatedSchedulePartDto.PreferenceRestriction.ActivityRestrictionCollection.Count > 0)
                    {
                        ActivityRestrictionDto activityRestrictionDto =
                            validatedSchedulePartDto.PreferenceRestriction.ActivityRestrictionCollection.First();
                        var preferenceActivity = new Activity(activityRestrictionDto.Activity.Id.GetValueOrDefault(), activityRestrictionDto.Activity.Description);
                        cellData.Preference.Activity = preferenceActivity;
                        if (activityRestrictionDto.StartTimeLimitation != null)
                            cellData.Preference.ActivityStartTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime, activityRestrictionDto.StartTimeLimitation);
                        if (activityRestrictionDto.EndTimeLimitation != null)
                            cellData.Preference.ActivityEndTimeLimitation = new TimeLimitation(TimeOfDayValidatorEndTime, activityRestrictionDto.EndTimeLimitation);
                        if (activityRestrictionDto.WorkTimeLimitation != null)
                            cellData.Preference.ActivityTimeLimitation = new TimeLimitation(TimeLengthValidator, activityRestrictionDto.WorkTimeLimitation);
                    }
                    cellData.Preference.StartTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime, validatedSchedulePartDto.PreferenceRestriction.StartTimeLimitation);
                    cellData.Preference.EndTimeLimitation = new TimeLimitation(TimeOfDayValidatorEndTime, validatedSchedulePartDto.PreferenceRestriction.EndTimeLimitation);
                    cellData.Preference.WorkTimeLimitation = new TimeLimitation(TimeLengthValidator, validatedSchedulePartDto.PreferenceRestriction.WorkTimeLimitation);
                    cellData.Preference.MustHave = validatedSchedulePartDto.PreferenceRestriction.MustHave;
                    cellData.Preference.TemplateName = validatedSchedulePartDto.PreferenceRestriction.TemplateName;
                    if (cellData.Preference.MustHave && validatedSchedulePartDto.IsInsidePeriod)
                        AddMustHave();
                }
                MakeEffective(cellData, validatedSchedulePartDto);
                cellData.Legal = validatedSchedulePartDto.LegalState;

                cellData.IsInsidePeriod = validatedSchedulePartDto.IsInsidePeriod;
                cellData.WeeklyMax = TimeSpan.FromMinutes(validatedSchedulePartDto.WeekMaxInMinutes);
                cellData.PeriodTarget = TimeSpan.FromMinutes(validatedSchedulePartDto.PeriodTargetInMinutes);
                cellData.Seasonality = validatedSchedulePartDto.Seasonality;

                cellData.PeriodDayOffsTarget = validatedSchedulePartDto.PeriodDayOffsTarget;
                cellData.PeriodDayOffs = validatedSchedulePartDto.PeriodDayOffs;

                cellData.BalancedPeriodTarget =
                    TimeSpan.FromMinutes(validatedSchedulePartDto.BalancedPeriodTargetInMinutes);
                cellData.BalancedPeriodTargetWithTolerance =
                    new TimePeriod(TimeSpan.FromMinutes(validatedSchedulePartDto.TargetTimeNegativeToleranceInMinutes),
                                   TimeSpan.FromMinutes(validatedSchedulePartDto.TargetTimePositiveToleranceInMinutes));
                cellData.BalanceIn = TimeSpan.FromMinutes(validatedSchedulePartDto.BalanceInInMinutes);
                cellData.BalanceOut = TimeSpan.FromMinutes(validatedSchedulePartDto.BalanceOutInMinutes);
                cellData.Extra = TimeSpan.FromMinutes(validatedSchedulePartDto.ExtraInInMinutes);

                cellData.Enabled = validatedSchedulePartDto.IsPreferenceEditable;
                cellData.HasAbsence = validatedSchedulePartDto.HasAbsence;
                if (validatedSchedulePartDto.IsInsidePeriod)
                {
                    cellData.MaxMustHave = validatedSchedulePartDto.MustHave;
                    _lastDate = validatedSchedulePartDto.DateOnly.DateTime;
                    _maxMustHaveCurrentPeriod = validatedSchedulePartDto.MustHave;
                    if (!firstDateIsSet)
                    {
                        _firstDateInCurrentPeriod = validatedSchedulePartDto.DateOnly.DateTime;
                        firstDateIsSet = true;
                    }
                }

                if (!validatedSchedulePartDto.IsInsidePeriod)
                    cellData.Enabled = false;

                if (!validatedSchedulePartDto.IsPreferenceEditable)
                    cellData.Enabled = false;

                if (validatedSchedulePartDto.HasPersonalAssignmentOnly)
                {
                    cellData.HasPersonalAssignmentOnly = true;
                    cellData.TipText = validatedSchedulePartDto.TipText;
                }

                cellData.HasShift = validatedSchedulePartDto.HasShift;
                cellData.HasDayOff = validatedSchedulePartDto.HasDayOff;

                if (cellData.HasDayOff || cellData.HasShift || cellData.HasAbsence)
                    cellData.Enabled = false;

                if (validatedSchedulePartDto.DisplayColor != null)
                    cellData.DisplayColor = ColorHelper.CreateColorFromDto(validatedSchedulePartDto.DisplayColor);
                cellData.DisplayName = validatedSchedulePartDto.ScheduledItemName;
                cellData.DisplayShortName = validatedSchedulePartDto.ScheduledItemShortName;

                cellData.IsWorkday = validatedSchedulePartDto.IsWorkday;
                cellData.ViolatesNightlyRest = validatedSchedulePartDto.ViolatesNightlyRest;

                retList.Add(cellNumber, cellData);
                cellNumber++;
            }
            _cellDataCollection = retList;
        }

        public DateOnly GetDateOnlyInPeriod(DateTime dateInPeriod)
        {
            var dateOnly = new DateOnly(dateInPeriod);
            bool periodExists = false;
            for (int i = 0; i < LoggedOnPerson.PersonPeriodCollection.Count; i++)
            {
                if (LoggedOnPerson.PersonPeriodCollection[i].Period.StartDate.DateTime <= dateOnly && LoggedOnPerson.PersonPeriodCollection[i].Period.EndDate.DateTime >= dateOnly)
                {
                    periodExists = true;
                    break;
                }
            }
            if (!periodExists && LoggedOnPerson.PersonPeriodCollection.Count > 0)
            {
                DateOnlyPeriodDto lastPeriod = LoggedOnPerson.PersonPeriodCollection.Last().Period;

                if (dateInPeriod > lastPeriod.EndDate.DateTime)//Teminal date
                    dateOnly = new DateOnly(lastPeriod.EndDate.DateTime);
                else//Not started yet
                    dateOnly = new DateOnly(lastPeriod.StartDate.DateTime.AddDays(1));
            }
            return dateOnly;
        }

        public void AddMustHave()
        {
            _numberOfMustHaveCurrentPeriod = NumberOfMustHaveCurrentPeriod + 1;
        }
        public void RemoveMustHave()
        {
            _numberOfMustHaveCurrentPeriod = NumberOfMustHaveCurrentPeriod - 1;
        }

        public TimePeriod CurrentPeriodTime()
        {
            TimeSpan minTime = TimeSpan.Zero;
            TimeSpan maxTime = TimeSpan.Zero;

            foreach (PreferenceCellData preferenceCellData in _cellDataCollection.Values)
            {
                if (!preferenceCellData.IsInsidePeriod)
                    continue;
                if (preferenceCellData.EffectiveRestriction != null && !preferenceCellData.EffectiveRestriction.Invalid)
                {
                    minTime = minTime.Add(preferenceCellData.EffectiveRestriction.WorkTimeLimitation.MinTime.GetValueOrDefault(TimeSpan.Zero));
                    maxTime = maxTime.Add(preferenceCellData.EffectiveRestriction.WorkTimeLimitation.MaxTime.GetValueOrDefault(TimeSpan.Zero));
                }
            }
            return new TimePeriod(minTime, maxTime);
        }

        public TimeSpan PeriodTargetTime()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (PreferenceCellData preferenceCellData in _cellDataCollection.Values)
                {
					if (preferenceCellData.IsInsidePeriod)
						return preferenceCellData.PeriodTarget;
                }
            }
            return TimeSpan.Zero;
        }

        public TimePeriod BalancedTargetTimeWithTolerance()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (PreferenceCellData preferenceCellData in _cellDataCollection.Values)
                {
					if (preferenceCellData.IsInsidePeriod)
						return preferenceCellData.BalancedPeriodTargetWithTolerance;
                }
            }
            return new TimePeriod(TimeSpan.Zero, TimeSpan.Zero);
        }


        public int PeriodTargetDayOffs()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (PreferenceCellData preferenceCellData in _cellDataCollection.Values)
                {
					if (preferenceCellData.IsInsidePeriod)
						return preferenceCellData.PeriodDayOffsTarget;
                }
            }
            return 0;
        }

        public int PeriodDayOffs()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (PreferenceCellData preferenceCellData in _cellDataCollection.Values)
                {
					if (preferenceCellData.IsInsidePeriod)
						return preferenceCellData.PeriodDayOffs;
                }
            }
            return 0;
        }

        public bool PeriodIsValid()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (PreferenceCellData preferenceCellData in _cellDataCollection.Values)
                {
                    if (preferenceCellData.Enabled && preferenceCellData.IsInsidePeriod)
                    {
                        if (preferenceCellData.EffectiveRestriction.Invalid)
                            return false;
                    }
                }
            }
            return true;
        }
        public void GetPreviousPeriod()
        {
            if (!_didGetPeriodLastCall)
            {
                if (FirstDateCurrentPeriod > DateTime.MinValue.AddDays(27))
                {
                    _firstDateInCurrentPeriod = _firstDateInCurrentPeriod.AddDays(-27);
                }
            }

            if (FirstDateCurrentPeriod > DateTime.MinValue)
                LoadPeriod(FirstDateCurrentPeriod.AddDays(-1), _scheduleHelper);
            else
            {
                LoadPeriod(FirstDateCurrentPeriod, _scheduleHelper);
            }
            loadShiftcategoriesAndDaysOff();
        }

        public void GetNextPeriod()
        {
            if (!_didGetPeriodLastCall && LoggedOnPerson.WorkflowControlSet != null)
                _lastDate = LoggedOnPerson.WorkflowControlSet.PreferencePeriod.StartDate.DateTime;
            _lastDate = _lastDate.AddDays(1);
            LoadPeriod(_lastDate, _scheduleHelper);
            loadShiftcategoriesAndDaysOff();
        }

        public void ReloadPeriod()
        {
            LoadPeriod(FirstDateCurrentPeriod, _scheduleHelper);
        }

        private static void MakeEffective(PreferenceCellData cellData, ValidatedSchedulePartDto validatedSchedulePartDto)
        {
            if (!validatedSchedulePartDto.LegalState)
            {
                if (validatedSchedulePartDto.PreferenceRestriction == null)
                {
                    cellData.EffectiveRestriction = new EffectiveRestriction();
                    return;
                }
                if (validatedSchedulePartDto.PreferenceRestriction.DayOff == null)
                {
                    cellData.EffectiveRestriction = new EffectiveRestriction();
                    return;
                }
            }

            var workTimeLimitation = new TimeLimitation(TimeLengthValidator)
                                         {
											 MinTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MinContractTimeInMinutes),
                                             MaxTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MaxContractTimeInMinutes)
                                         };

            var startTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime)
                                          {
                                              MinTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MinStartTimeMinute),
                                              MaxTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MaxStartTimeMinute)
                                          };

            var endTimeLimitation = new TimeLimitation(TimeOfDayValidatorEndTime)
                                        {
                                            MinTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MinEndTimeMinute),
                                            MaxTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MaxEndTimeMinute)
                                        };

            cellData.EffectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation);
        }

        public CultureInfo CurrentCultureInfo()
        {
            return _currentCultureInfo;
        }

        public CultureInfo CurrentUICultureInfo()
        {
            return _currentUiCultureInfo;
        }

        private void loadShiftcategoriesAndDaysOff()
        {
            _shiftCategories.Clear();
            _daysOff.Clear();
            _absences.Clear();
            AgentScheduleStateHolder.Instance().FillShiftCategoryDictionary(_firstDateInCurrentPeriod);

            foreach (ShiftCategory shiftCategory in AgentScheduleStateHolder.Instance().ShiftCategories)
            {
                _shiftCategories.Add(shiftCategory);
            }

            foreach (Common.DayOff dayOff in AgentScheduleStateHolder.Instance().DaysOff)
            {
                _daysOff.Add(dayOff);
            }

            foreach (var absence in AgentScheduleStateHolder.Instance().Absences)
            {
                _absences.Add(absence);    
            }
        }
    }
}