using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public class StudentAvailabilityModel
    {
        private DateTime _lastDate;
        private DateTime _firstDateInCurrentPeriod;
        private CultureInfo _currentCultureInfo;
        private CultureInfo _currentUiCultureInfo;
        private Dictionary<int, IStudentAvailabilityCellData> _cellDataCollection;
        private static readonly TimeLengthValidator TimeLengthValidator = new TimeLengthValidator();
        private static readonly TimeOfDayValidator TimeOfDayValidatorStartTime = new TimeOfDayValidator(false);
        private static readonly TimeOfDayValidator TimeOfDayValidatorEndTime = new TimeOfDayValidator(true);

        private readonly PersonDto _loggedOnPerson;
        private readonly IScheduleHelper _scheduleHelper;
        private bool _didGetPeriodLastCall;

        public StudentAvailabilityModel(PersonDto loggedOnPerson, IScheduleHelper scheduleHelper)
        {
            _cellDataCollection = new Dictionary<int, IStudentAvailabilityCellData>();
            _lastDate = DateTime.Today;
            _loggedOnPerson = loggedOnPerson;
            SetCultures();
            _scheduleHelper = scheduleHelper;
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

        public Dictionary<int, IStudentAvailabilityCellData> CellDataCollection
        {
            get { return _cellDataCollection; }
        }

        public DateTime FirstDateCurrentPeriod
        {
            get { return _firstDateInCurrentPeriod; }
        }

        public PersonDto LoggedOnPerson
        {
            get { return _loggedOnPerson; }
        }

        public void LoadPeriod(DateTime dateInPeriod, IScheduleHelper scheduleHelper)
        {
            DateOnly dateOnly = GetDateOnlyInPeriod(dateInPeriod);
            var ret = scheduleHelper.Validate(LoggedOnPerson, dateOnly, true);
            var retList = new Dictionary<int, IStudentAvailabilityCellData>();
            int cellNumber = 0;
            bool firstDateIsSet = false;
            _didGetPeriodLastCall = false;
            foreach (ValidatedSchedulePartDto validatedSchedulePartDto in ret)
            {
                _didGetPeriodLastCall = true;
                var cellData = new StudentAvailabilityCellData
                {
                    TheDate = validatedSchedulePartDto.DateOnly.DateTime
                };
                if(validatedSchedulePartDto.StudentAvailabilityDay!=null)
                {
                    cellData.StudentAvailabilityRestrictions = new List<StudentAvailabilityRestriction>();
                    if (validatedSchedulePartDto.StudentAvailabilityDay.StudentAvailabilityRestrictions != null)
                    foreach(var restriction in validatedSchedulePartDto.StudentAvailabilityDay.StudentAvailabilityRestrictions)
                    {
                        var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
                        studentAvailabilityRestriction.StartTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime, restriction.StartTimeLimitation);
                        studentAvailabilityRestriction.EndTimeLimitation = new TimeLimitation(TimeOfDayValidatorStartTime, restriction.EndTimeLimitation);
                        cellData.StudentAvailabilityRestrictions.Add(studentAvailabilityRestriction);
                    }
                }
                MakeEffective(cellData, validatedSchedulePartDto);
                cellData.Legal = validatedSchedulePartDto.LegalState;
                cellData.IsInsidePeriod = validatedSchedulePartDto.IsInsidePeriod;
                cellData.BalancedPeriodTarget = TimeSpan.FromMinutes(validatedSchedulePartDto.BalancedPeriodTargetInMinutes);
                cellData.WeeklyMax = TimeSpan.FromMinutes(validatedSchedulePartDto.WeekMaxInMinutes);
                cellData.PeriodTarget = TimeSpan.FromMinutes(validatedSchedulePartDto.PeriodTargetInMinutes);
                cellData.Enabled = validatedSchedulePartDto.IsStudentAvailabilityEditable;
                if (validatedSchedulePartDto.IsInsidePeriod)
                {
                    _lastDate = validatedSchedulePartDto.DateOnly.DateTime;
                    if (!firstDateIsSet)
                    {
                        _firstDateInCurrentPeriod = validatedSchedulePartDto.DateOnly.DateTime;
                        firstDateIsSet = true;
                    }
                }
                if (!validatedSchedulePartDto.IsInsidePeriod)
                    cellData.Enabled = false;

                if (!validatedSchedulePartDto.IsStudentAvailabilityEditable)
                    cellData.Enabled = false;

                if (validatedSchedulePartDto.HasPersonalAssignmentOnly)
                {
                    cellData.HasPersonalAssignmentOnly = true;
                    cellData.TipText = validatedSchedulePartDto.TipText;
                }

                cellData.HasShift = validatedSchedulePartDto.HasShift;
                cellData.HasDayOff = validatedSchedulePartDto.HasDayOff;
                cellData.HasAbsence = validatedSchedulePartDto.HasAbsence;
                cellData.HasPreference = validatedSchedulePartDto.PreferenceRestriction != null;

                if (cellData.HasDayOff || cellData.HasShift || cellData.HasAbsence)
                    cellData.Enabled = false;

                if (validatedSchedulePartDto.DisplayColor != null)
                    cellData.DisplayColor = ColorHelper.CreateColorFromDto(validatedSchedulePartDto.DisplayColor);
                else
                    cellData.DisplayColor = Color.SkyBlue;
                cellData.DisplayName = validatedSchedulePartDto.ScheduledItemName;
                cellData.DisplayShortName = validatedSchedulePartDto.ScheduledItemShortName;

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

        public TimePeriod CurrentPeriodTime()
        {
            TimeSpan minTime = TimeSpan.Zero;
            TimeSpan maxTime = TimeSpan.Zero;

            foreach (var studentAvailabilityCellData in _cellDataCollection.Values)
            {
                if (!studentAvailabilityCellData.IsInsidePeriod)
                    continue;
                if (studentAvailabilityCellData.StudentAvailabilityRestrictions != null || studentAvailabilityCellData.HasAbsence || studentAvailabilityCellData.HasDayOff || studentAvailabilityCellData.HasPersonalAssignmentOnly || studentAvailabilityCellData.HasPreference || studentAvailabilityCellData.HasShift)
                {
                    if (studentAvailabilityCellData.EffectiveRestriction != null && !studentAvailabilityCellData.EffectiveRestriction.Invalid)
                    {
                        minTime = minTime.Add(studentAvailabilityCellData.EffectiveRestriction.WorkTimeLimitation.MinTime.GetValueOrDefault());
                        maxTime = maxTime.Add(studentAvailabilityCellData.EffectiveRestriction.WorkTimeLimitation.MaxTime.GetValueOrDefault());
                    }
                }
            }
            return new TimePeriod(minTime, maxTime);
        }

        public bool PeriodIsValid()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (var studentAvailabilityCellData in _cellDataCollection.Values)
                {
                    if (studentAvailabilityCellData.Enabled)
                    {
                        if (studentAvailabilityCellData.EffectiveRestriction.Invalid)
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
        }

        public void GetNextPeriod()
        {
            if (!_didGetPeriodLastCall && LoggedOnPerson.WorkflowControlSet != null)
                _lastDate = LoggedOnPerson.WorkflowControlSet.StudentAvailabilityPeriod.StartDate.DateTime;
            _lastDate = _lastDate.AddDays(1);
            LoadPeriod(_lastDate, _scheduleHelper);
        }

        public void ReloadPeriod()
        {
            LoadPeriod(FirstDateCurrentPeriod, _scheduleHelper);
        }

        private static void MakeEffective(StudentAvailabilityCellData cellData, ValidatedSchedulePartDto validatedSchedulePartDto)
        {
            if (!validatedSchedulePartDto.LegalState)
            {
                cellData.EffectiveRestriction = new EffectiveRestriction();
                    return;
            }
            var workTimeLimitation = new TimeLimitation(TimeLengthValidator)
                                         {
                                             MinTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MinWorkTimeInMinutes),
                                             MaxTime = TimeSpan.FromMinutes(validatedSchedulePartDto.MaxWorkTimeInMinutes)
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

        public TimeSpan BalancedPeriodTargetTime()
        {
            if (_cellDataCollection.Count > 0)
            {
                foreach (var studentAvailabilityCellData in _cellDataCollection.Values)
                {
                    return studentAvailabilityCellData.BalancedPeriodTarget;
                }
            }
            return TimeSpan.Zero;
        }
    }
}