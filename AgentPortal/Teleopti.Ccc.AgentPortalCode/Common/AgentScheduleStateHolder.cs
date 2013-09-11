using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public interface IAgentScheduleStateHolder
    {
        void UpdateOrAddPreference(IList<DateTime> dateCollection, IList<Preference> preferenceCollection);
        void UpdateOrAddPreference(DateTime date, Preference preference);
    }

    public class AgentScheduleStateHolder : IAgentScheduleStateHolder
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
            if (_asmScheduleDictionary.ContainsKey(date))
            {
                _asmScheduleDictionary.Clear(); // fix for 9517.
            }

            _asmScheduleDictionary.Fill(scheduleAppointmentCollection);
        }

        public void FillAgentSchedulePartDictionary(IEnumerable<SchedulePartDto> partCollection)
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
            DateOnlyDto end = new DateOnlyDto();
            end.DateTime = unspecifiedKind.AddDays(1);
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
                    var shiftCategory = new ShiftCategory(categoryDto.Name, categoryDto.ShortName, categoryDto.Id.GetValueOrDefault(), color);
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
                var absence = new Absence(absenceDto.Name, absenceDto.ShortName, absenceDto.Id.GetValueOrDefault(), color);
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
                  var dayOff = new DayOff(dayOffInfoDto.Name, dayOffInfoDto.ShortName, dayOffInfoDto.Id.GetValueOrDefault(), Color.Empty);
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
            dto.Person = Person;
            var restrictionCount = studentAvailabilityRestrictions.Count;
            var studentAvailabilityRestrictionsDto = new StudentAvailabilityRestrictionDto[restrictionCount];

            for (int i = 0; i < restrictionCount; i++)
            {
                var studentAvailabilityRestrictionDto = new StudentAvailabilityRestrictionDto();
                studentAvailabilityRestrictionDto.StartTimeLimitation = studentAvailabilityRestrictions[i].StartTimeLimitation.SetValuesToDto();
                studentAvailabilityRestrictionDto.EndTimeLimitation = studentAvailabilityRestrictions[i].EndTimeLimitation.SetValuesToDto();
                studentAvailabilityRestrictionsDto[i] = studentAvailabilityRestrictionDto;
            }
            dto.StudentAvailabilityRestrictions.Clear();
            foreach (var studentAvailabilityRestrictionDto in studentAvailabilityRestrictionsDto)
            {
                dto.StudentAvailabilityRestrictions.Add(studentAvailabilityRestrictionDto);
            }
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
            preferenceRestrictionDto.DayOff = dayOff;
            preferenceRestrictionDto.ShiftCategory = shiftCategory;
            preferenceRestrictionDto.Absence = absence;
            preferenceRestrictionDto.StartTimeLimitation = preference.StartTimeLimitation.SetValuesToDto();
            preferenceRestrictionDto.EndTimeLimitation = preference.EndTimeLimitation.SetValuesToDto();
            preferenceRestrictionDto.WorkTimeLimitation = preference.WorkTimeLimitation.SetValuesToDto();
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
                activityRestrictionDto.StartTimeLimitation = preference.ActivityStartTimeLimitation.SetValuesToDto();
                activityRestrictionDto.EndTimeLimitation = preference.ActivityEndTimeLimitation.SetValuesToDto();
                activityRestrictionDto.WorkTimeLimitation = preference.ActivityTimeLimitation.SetValuesToDto();
                preferenceRestrictionDto.ActivityRestrictionCollection.Clear();
                preferenceRestrictionDto.ActivityRestrictionCollection.Add(activityRestrictionDto);
            }

            SdkServiceHelper.SchedulingService.SavePreference(preferenceRestrictionDto);
        }

        public void DeletePreference(DateTime date)
        {
            DateTime unspecifiedKind = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            PreferenceRestrictionDto restrictionDto = new PreferenceRestrictionDto();
            restrictionDto.Person = Person;
            restrictionDto.RestrictionDate = new DateOnlyDto();
            restrictionDto.RestrictionDate.DateTime = unspecifiedKind;
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
            IScheduleItemList asmScheduleItemList;
            DateTime currentDateTime = DateTime.Now;
            ICustomScheduleAppointment nextActivity = null;

            if (_asmScheduleDictionary.TryGetValue(currentDateTime.Date, out asmScheduleItemList))
            {
                nextActivity = asmScheduleItemList.GetNextActivity(currentDateTime);
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
            var handler = ActivityChanged;
            if (handler != null)
            {
                handler.Invoke(this, arg);
            }
        }
    }
}
