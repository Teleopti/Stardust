﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public class AgentScheduleStateHolder
    {
        private static AgentScheduleStateHolder _instance;

        private bool _initialized;
        private IScheduleDictionary _agentScheduleDctionary;
        private IScheduleDictionary _asmScheduleDictionary;
        private IDictionary<DateTime, SchedulePartDto> _schedulePartDictionary;
        private readonly IList<ShiftCategory> _shiftCategories = new List<ShiftCategory>();
        private readonly IList<DayOff> _daysOff = new List<DayOff>();
        private readonly List<Absence> _absences = new List<Absence>();
        private ScheduleAppointmentTypes _visualizingScheduleAppointmentTypes;

        public bool Initialized
        {
            get { return _initialized; }
        }

        public DateTimePeriodDto AgentSchedulePeriod { get; set; }

        public DateTimePeriodDto ScheduleMessengerPeriod { get; set; }

        public IList<ShiftCategory> ShiftCategories
        {
            get { return _shiftCategories; }
        }

        public IList<Absence> Absences
        {
            get { return _absences; }
        }

        public IScheduleDictionary AgentScheduleDictionary
        {
            get { return _agentScheduleDctionary; }
        }

        public IDictionary<DateTime, SchedulePartDto> AgentSchedulePartDictionary
        {
            get { return _schedulePartDictionary; }
        }

        public IScheduleDictionary ScheduleMessengerScheduleDictionary
        {
            get { return _asmScheduleDictionary; }
        }

        public bool IsEndOfTheActivityCheck
        {
            get
            {
                DateTime currentDate = DateTime.Today;
                IScheduleItemList asmScheduleItemList;

                if (_asmScheduleDictionary.TryGetValue(currentDate, out asmScheduleItemList))
                    return (asmScheduleItemList.GetNextActivity(currentDate) == null);
                return true;
            }
        }

        public ScheduleAppointmentTypes VisualizingScheduleAppointmentTypes
        {
            get
            {
                return _visualizingScheduleAppointmentTypes;
            }
            set
            {
                _visualizingScheduleAppointmentTypes = value;
            }
        }

        public ScheduleAppointmentColorTheme ScheduleAppointmentColorTheme { get; set; }

        public PersonDto Person { get; set; }

        public int CurrentResolution { get; set; }

        public int CalendarDayIndex { get; set; }

        public IList<DayOff> DaysOff
        {
            get { return _daysOff; }
        }

        public static bool IsInitialized
        {
            get { return _instance != null; }
        }

        /// <summary>
        /// Occurs when current activity changed .
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-07-26
        /// </remarks>
        public static event EventHandler<ActivityChangedEventArgs> ActivityChanged;

        public static event EventHandler<StateChangedEventArgs> StateChanged;

        public static AgentScheduleStateHolder Instance()
        {
            if (_instance == null)
            {
                _instance = new AgentScheduleStateHolder();
            }
            return _instance;
        }

        public void Initialize(PersonDto person)
        {
            Person = person;
            _agentScheduleDctionary = new ScheduleDictionary();
            _asmScheduleDictionary = new ScheduleDictionary();
            _schedulePartDictionary = new Dictionary<DateTime, SchedulePartDto>();
            _visualizingScheduleAppointmentTypes = ScheduleAppointmentTypes.Absence |
                                                   ScheduleAppointmentTypes.Activity |
                                                   ScheduleAppointmentTypes.DayOff |
                                                   ScheduleAppointmentTypes.Meeting |
                                                   ScheduleAppointmentTypes.PersonalShift |
                                                   ScheduleAppointmentTypes.PublicNote;

            _initialized = true;
        }

        public void FillScheduleDictionary(IList<ICustomScheduleAppointment> scheduleAppointmentCollection)
        {
            _agentScheduleDctionary.Fill(scheduleAppointmentCollection);
        }

        public void FillScheduleMessengerDictionary(IList<ICustomScheduleAppointment> scheduleAppointmentCollection, DateTime date)
        {
            IScheduleItemList list;
            if (_asmScheduleDictionary.TryGetValue(date, out list))
            {

                _asmScheduleDictionary.Clear(); // fix for 9517.
                //list.ScheduleItemCollection.Clear();
            }


            _asmScheduleDictionary.Fill(scheduleAppointmentCollection);
        }

        public void FillAgentSchedulePartDictionary(IList<SchedulePartDto> partCollection)
        {
            _schedulePartDictionary.Clear();

            foreach (SchedulePartDto schedulePartDto in partCollection)
            {
                _schedulePartDictionary.Add(schedulePartDto.Date.DateTime, schedulePartDto);
            }
        }

        public void FillShiftCategoryDictionary(DateTime dateTime)
        {
            DateTime unspecifiedKind = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            _shiftCategories.Clear();
            DateOnlyDto start = new DateOnlyDto();
            start.DateTime = unspecifiedKind;
            start.DateTimeSpecified = true;
            DateOnlyDto end = new DateOnlyDto();
            end.DateTime = unspecifiedKind.AddDays(1);
            end.DateTimeSpecified = true;
           var categories = new List<ShiftCategoryDto>(SdkServiceHelper.SchedulingService.GetShiftCategoriesBelongingToRuleSetBag(Person, start, end));
                
            _shiftCategories.Clear();

            if (Person.WorkflowControlSet == null)
                return;
            
            categories.Sort(new CatComparer());

            foreach (ShiftCategoryDto categoryDto in categories)
            {
                bool found = false;
                foreach (ShiftCategoryDto shiftCategoryDto in Person.WorkflowControlSet.AllowedPreferenceShiftCategories)
                {
                    if(categoryDto.Id.Equals(shiftCategoryDto.Id))
                    {
                        found = true;
                        continue;
                    }
                }
                if(found)
                {
                    Color color = ColorHelper.CreateColorFromDto(categoryDto.DisplayColor);
                    var shiftCategory = new ShiftCategory(categoryDto.Name, categoryDto.ShortName, categoryDto.Id, color);
                    _shiftCategories.Add(shiftCategory);
                }
            }
        }

        internal class CatComparer : IComparer<ShiftCategoryDto>
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
            public int Compare(ShiftCategoryDto x, ShiftCategoryDto y)
            {
                return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentUICulture);
            }
        }

        internal class DayOffComparer : IComparer<DayOffInfoDto>
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
            public int Compare(DayOffInfoDto x, DayOffInfoDto y)
            {
                return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentUICulture);
            }
        }

        internal class AbcenseComparer : IComparer<Absence>
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
            public int Compare(Absence x, Absence y)
            {
                return string.Compare(x.Name, y.Name, true, CultureInfo.CurrentUICulture);
            }
        }

        public void FillAbsences()
        {
            if (Person.WorkflowControlSet == null)
                return;

            _absences.Clear();
            
            foreach (var absenceDto in Person.WorkflowControlSet.AllowedPreferenceAbsences)
            {
                var color = ColorHelper.CreateColorFromDto(absenceDto.DisplayColor);
                var absence = new Absence(absenceDto.Name, absenceDto.ShortName, absenceDto.Id, color);
                _absences.Add(absence);
            }
            _absences.Sort(new AbcenseComparer());
        }

        public void FillDaysOff()
        {
            if (Person.WorkflowControlSet == null)
                return;
            var dayOffs = new List<DayOffInfoDto>(Person.WorkflowControlSet.AllowedPreferenceDayOffs);
            dayOffs.Sort(new DayOffComparer());
            foreach (var dayOffInfoDto in dayOffs)
            {
                  var dayOff = new DayOff(dayOffInfoDto.Name, dayOffInfoDto.ShortName, dayOffInfoDto.Id, Color.Empty);
                _daysOff.Add(dayOff); 
            }
        }

        public void UpdateOrAddStudentAvailability(IList<DateTime> dateCollection, IList<StudentAvailabilityRestriction> studentAvailabilityRestrictions)
        {
            foreach (var dateTime in dateCollection)
            {
                UpdateOrAddStudentAvailability(dateTime, studentAvailabilityRestrictions);
            }
        }

        public void UpdateOrAddStudentAvailability(DateTime date, IList<StudentAvailabilityRestriction> studentAvailabilityRestrictions)
        {
            DateTime unspecifiedKind = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            var dto = new StudentAvailabilityDayDto();
            dto.RestrictionDate = new DateOnlyDto();
            dto.RestrictionDate.DateTime = unspecifiedKind;
            dto.RestrictionDate.DateTimeSpecified = true;
            dto.Person = Person;
            var restrictionCount = studentAvailabilityRestrictions.Count;
            var studentAvailabilityRestrictionsDto = new StudentAvailabilityRestrictionDto[restrictionCount];
            for (int i = 0; i < restrictionCount; i++)
            {
                var studentAvailabilityRestrictionDto = new StudentAvailabilityRestrictionDto();
                var startTimeLimitationDto = new TimeLimitationDto();
                studentAvailabilityRestrictions[i].StartTimeLimitation.SetValuesToDto(startTimeLimitationDto);
                studentAvailabilityRestrictionDto.StartTimeLimitation = startTimeLimitationDto;
                var endTimeLimitationDto = new TimeLimitationDto();
                studentAvailabilityRestrictions[i].EndTimeLimitation.SetValuesToDto(endTimeLimitationDto);
                studentAvailabilityRestrictionDto.EndTimeLimitation = endTimeLimitationDto;
                studentAvailabilityRestrictionsDto[i] = studentAvailabilityRestrictionDto;
            }
            dto.StudentAvailabilityRestrictions = studentAvailabilityRestrictionsDto;
            if (Person.TerminationDate != null && dto.RestrictionDate.DateTime > Person.TerminationDate.DateTime)
                return;
            SdkServiceHelper.SchedulingService.SaveStudentAvailabilityDay(dto);
        }

        public void DeleteStudentAvailability(DateTime date)
        {
            DateTime unspecifiedKind = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            StudentAvailabilityDayDto dto = new StudentAvailabilityDayDto();
            dto.Person = Person;
            dto.RestrictionDate = new DateOnlyDto();
            dto.RestrictionDate.DateTime = unspecifiedKind;
            dto.RestrictionDate.DateTimeSpecified = true;
            SdkServiceHelper.SchedulingService.DeleteStudentAvailabilityDay(dto);
        }

        public void UpdateOrAddPreference(IList<DateTime> dateCollection, IList<Preference> preferenceCollection)
        {
            if (dateCollection == null) return;
            for (var i = 0; i < dateCollection.Count; i++)
            {
                if (preferenceCollection != null && preferenceCollection.Count == dateCollection.Count)
                    UpdateOrAddPreference(dateCollection[i], preferenceCollection[i]);
            }
        }

        public void UpdateOrAddPreference(DateTime date, Preference preference)
        {

            ShiftCategoryDto shiftCategory = null;
            if (preference.ShiftCategory != null)
                shiftCategory = new ShiftCategoryDto
                {
                    Name = preference.ShiftCategory.Name,
                    ShortName = preference.ShiftCategory.ShortName,
                    Id = preference.ShiftCategory.Id
                };
            DayOffInfoDto dayOff = null;
            if (preference.DayOff != null)
                dayOff = new DayOffInfoDto
                {
                    Name = preference.DayOff.Name,
                    ShortName = preference.DayOff.ShortName,
                    Id = preference.DayOff.Id
                };

            AbsenceDto absence = null;
            if(preference.Absence != null)
            {
                absence = new AbsenceDto
                              {
                                  Name = preference.Absence.Name,
                                  ShortName = preference.Absence.ShortName,
                                  Id = preference.Absence.Id
                              };
            }

            DateTime unspecifiedKind = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            var preferenceRestrictionDto = new PreferenceRestrictionDto();
            preferenceRestrictionDto.Person = Person;
            preferenceRestrictionDto.RestrictionDate = new DateOnlyDto();
            preferenceRestrictionDto.RestrictionDate.DateTime = unspecifiedKind;
            preferenceRestrictionDto.RestrictionDate.DateTimeSpecified = true;
            preferenceRestrictionDto.DayOff = dayOff;
            preferenceRestrictionDto.ShiftCategory = shiftCategory;
            preferenceRestrictionDto.Absence = absence;
            preferenceRestrictionDto.StartTimeLimitation = new TimeLimitationDto();
            preference.StartTimeLimitation.SetValuesToDto(preferenceRestrictionDto.StartTimeLimitation);
            preferenceRestrictionDto.EndTimeLimitation = new TimeLimitationDto();
            preference.EndTimeLimitation.SetValuesToDto(preferenceRestrictionDto.EndTimeLimitation);
            preferenceRestrictionDto.WorkTimeLimitation = new TimeLimitationDto();
            preference.WorkTimeLimitation.SetValuesToDto(preferenceRestrictionDto.WorkTimeLimitation);
            preferenceRestrictionDto.MustHave = preference.MustHave;
            preferenceRestrictionDto.TemplateName = preference.TemplateName;

            if (preference.Activity != null)
            {
                var activityRestrictionDto = new ActivityRestrictionDto();
                activityRestrictionDto.Activity = new ActivityDto
                                                      {
                                                          Id = preference.Activity.Id,
                                                          Description = preference.Activity.Name
                                                      };
                activityRestrictionDto.StartTimeLimitation = new TimeLimitationDto();
                preference.ActivityStartTimeLimitation.SetValuesToDto(activityRestrictionDto.StartTimeLimitation);
                activityRestrictionDto.EndTimeLimitation = new TimeLimitationDto();
                preference.ActivityEndTimeLimitation.SetValuesToDto(activityRestrictionDto.EndTimeLimitation);
                activityRestrictionDto.WorkTimeLimitation = new TimeLimitationDto();
                preference.ActivityTimeLimitation.SetValuesToDto(activityRestrictionDto.WorkTimeLimitation);
                preferenceRestrictionDto.ActivityRestrictionCollection = new[] {activityRestrictionDto};
            }

            preferenceRestrictionDto.MustHaveSpecified = true;
            SdkServiceHelper.SchedulingService.SavePreference(preferenceRestrictionDto);
        }

        public void DeletePreference(DateTime date)
        {
            DateTime unspecifiedKind = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            PreferenceRestrictionDto restrictionDto = new PreferenceRestrictionDto();
            restrictionDto.Person = Person;
            restrictionDto.RestrictionDate = new DateOnlyDto();
            restrictionDto.RestrictionDate.DateTime = unspecifiedKind;
            restrictionDto.RestrictionDate.DateTimeSpecified = true;
            SdkServiceHelper.SchedulingService.DeletePreference(restrictionDto);
        }

        public StudentAvailabilityDayDto StudentAvailabilityDayOnDate(DateTime date)
        {
            if (!_schedulePartDictionary.ContainsKey(date))
                return null;
            SchedulePartDto part = _schedulePartDictionary[date];
            return part.StudentAvailabilityDay;
        }

        public bool CanVisualize(ScheduleAppointmentTypes type)
        {
            return ((_visualizingScheduleAppointmentTypes & type) == type);
        }

        public IList<ICustomScheduleAppointment> UnsavedAppointments()
        {
            return _agentScheduleDctionary.UnsavedAppointments();
        }

        public void CheckNextActivity()
        {
            IScheduleItemList asmScheduleItemList = null;
            DateTime currentDateTime = DateTime.Now;
            ICustomScheduleAppointment nextActivity = null;

            if (_asmScheduleDictionary.ContainsKey(currentDateTime.Date))
            {
                asmScheduleItemList = _asmScheduleDictionary[currentDateTime.Date];
            }
            if (asmScheduleItemList != null)
            {
                // shift ended or not yet started
                if (asmScheduleItemList.GetCurrentScheduleItem(currentDateTime) == null)
                {
                    ICustomScheduleAppointment firstScheduleItem = asmScheduleItemList.FirstScheduleItem();
                    if (asmScheduleItemList.ScheduleItemCollection.Count > 0)
                    {
                        if (currentDateTime < firstScheduleItem.StartTime)
                        {
                            //Show first activity
                            nextActivity = firstScheduleItem;
                        }
                    }
                }
                else
                {
                    nextActivity = asmScheduleItemList.GetNextActivity(currentDateTime);
                }
            }
            OnActivityChanged(nextActivity);
        }

        public void OnStateChange(ScheduleAppointmentStatusTypes status, Dto item)
        {
            if (StateChanged != null)
            {
                StateChanged(this, new StateChangedEventArgs(status, item));
            }
        }

        protected void OnActivityChanged(ICustomScheduleAppointment nextScheduleItem)
        {
            ActivityChangedEventArgs arg = new ActivityChangedEventArgs(nextScheduleItem);
            if (ActivityChanged != null)
            {
                ActivityChanged.Invoke(this, arg);
            }
        }
    }
}
