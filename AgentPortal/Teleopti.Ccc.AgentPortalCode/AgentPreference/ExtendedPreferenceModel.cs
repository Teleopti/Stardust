using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IExtendedPreferenceModel
    {
        IEnumerable<Activity> AllowedPreferenceActivities { get; }
        bool ModifyExtendedPreferencesIsPermitted { get; }

        TimeSpan? WorkTimeLimitationMin { get; set; }
        TimeSpan? WorkTimeLimitationMax { get; set; }
        bool EndTimeLimitationMaxNextDay { get; set; }
        TimeSpan? StartTimeLimitationMax { get; set; }
        TimeSpan? StartTimeLimitationMin { get; set; }
        bool EndTimeLimitationMinNextDay { get; set; }
        TimeSpan? EndTimeLimitationMax { get; set; }
        TimeSpan? EndTimeLimitationMin { get; set; }
        DayOff DayOff { get; set; }
        ShiftCategory ShiftCategory { get; set; }
        Absence Absence { get; set; }
        Activity Activity { get; set; }
        TimeSpan? ActivityStartTimeLimitationMin { get; set; }
        TimeSpan? ActivityStartTimeLimitationMax { get; set; }
        TimeSpan? ActivityEndTimeLimitationMin { get; set; }
        TimeSpan? ActivityEndTimeLimitationMax { get; set; }
        TimeSpan? ActivityTimeLimitationMin { get; set; }
        TimeSpan? ActivityTimeLimitationMax { get; set; }
        Preference Preference { get; }

        void SetPreference(Preference preference);
        void SetValuesToPreference();
    }

    public class ExtendedPreferenceModel : IExtendedPreferenceModel
    {
        private readonly ISessionData _sessionData;
        private readonly IPermissionService _permissionService;
        private readonly IApplicationFunctionHelper _applicationFunctionHelper;
        private DayOff _dayOff;
        private ShiftCategory _shiftCategory;
        //private Templates _templates;
        private Activity _activity;
        private Absence _absence;

        public ExtendedPreferenceModel(ISessionData sessionData, IPermissionService permissionService,
                                       IApplicationFunctionHelper applicationFunctionHelper)
        {
            _sessionData = sessionData;
            _permissionService = permissionService;
            _applicationFunctionHelper = applicationFunctionHelper;
        }

        public TimeSpan? WorkTimeLimitationMin { get; set; }
        public TimeSpan? WorkTimeLimitationMax { get; set; }
        public bool EndTimeLimitationMaxNextDay { get; set; }
        public TimeSpan? StartTimeLimitationMax { get; set; }
        public TimeSpan? StartTimeLimitationMin { get; set; }
        public bool EndTimeLimitationMinNextDay { get; set; }
        public TimeSpan? EndTimeLimitationMax { get; set; }
        public TimeSpan? EndTimeLimitationMin { get; set; }

        public DayOff DayOff
        {
            get { return _dayOff; }
            set
            {
                if (value != null && string.IsNullOrEmpty(value.Id))
                {
                    value = null;
                }

                _dayOff = value;
                if (_dayOff != null)
                {
                    ShiftCategory = null;
                    Absence = null;
                    StartTimeLimitationMax = null;
                    StartTimeLimitationMin = null;
                    EndTimeLimitationMax = null;
                    EndTimeLimitationMin = null;
                    EndTimeLimitationMaxNextDay = false;
                    EndTimeLimitationMinNextDay = false;
                    WorkTimeLimitationMin = null;
                    WorkTimeLimitationMax = null;
                    Activity = null;
                    ActivityStartTimeLimitationMax = null;
                    ActivityStartTimeLimitationMin = null;
                    ActivityEndTimeLimitationMax = null;
                    ActivityEndTimeLimitationMin = null;
                    ActivityTimeLimitationMax = null;
                    ActivityTimeLimitationMin = null;
                }
            }
        }

        public ShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
            set
            {
                if (value != null && string.IsNullOrEmpty(value.Id))
                {
                    value = null;
                }

                _shiftCategory = value;
                if (_shiftCategory != null)
                {
                    DayOff = null;
                    Absence = null;
                }
            }
        }

        public Absence Absence
        {
            get { return _absence; }
            set
            {
                if(value != null && string.IsNullOrEmpty(value.Id))
                {
                    value = null;
                }

                _absence = value;
                if(_absence != null)
                {
                    DayOff = null;
                    ShiftCategory = null;
                    StartTimeLimitationMax = null;
                    StartTimeLimitationMin = null;
                    EndTimeLimitationMax = null;
                    EndTimeLimitationMin = null;
                    EndTimeLimitationMaxNextDay = false;
                    EndTimeLimitationMinNextDay = false;
                    WorkTimeLimitationMin = null;
                    WorkTimeLimitationMax = null;
                    Activity = null;
                    ActivityStartTimeLimitationMax = null;
                    ActivityStartTimeLimitationMin = null;
                    ActivityEndTimeLimitationMax = null;
                    ActivityEndTimeLimitationMin = null;
                    ActivityTimeLimitationMax = null;
                    ActivityTimeLimitationMin = null;
                }
            }
        }

        /*public Templates Templates
        {
            get { return _templates; }
            set
            {
                if (value != null && string.IsNullOrEmpty(value.Id))
                {
                    value = null;
                }

                _templates = value;
                if (_templates != null)
                {
                    DayOff = null;
                }
            }
        }*/

        public Activity Activity
        {
            get { return _activity; }
            set
            {
                if (value != null && string.IsNullOrEmpty(value.Id))
                {
                    value = null;
                }

                _activity = value;
                if (_activity != null)
                {
                    DayOff = null;
                    Absence = null;
                }
                else
                {
                    ActivityStartTimeLimitationMax = null;
                    ActivityStartTimeLimitationMin = null;
                    ActivityEndTimeLimitationMax = null;
                    ActivityEndTimeLimitationMin = null;
                    ActivityTimeLimitationMax = null;
                    ActivityTimeLimitationMin = null;
                }
            }
        }

        public TimeSpan? ActivityStartTimeLimitationMin { get; set; }
        public TimeSpan? ActivityStartTimeLimitationMax { get; set; }
        public TimeSpan? ActivityEndTimeLimitationMin { get; set; }
        public TimeSpan? ActivityEndTimeLimitationMax { get; set; }
        public TimeSpan? ActivityTimeLimitationMin { get; set; }
        public TimeSpan? ActivityTimeLimitationMax { get; set; }

        public Preference Preference { get; private set; }

        public IEnumerable<Activity> AllowedPreferenceActivities
        {
            get
            {
                var person = _sessionData.LoggedOnPerson;
                if (person.WorkflowControlSet != null)
                {
                    var dto = person.WorkflowControlSet.AllowedPreferenceActivity;
                    if (dto != null)
                    {
                        var activity = new Activity(dto.Id, dto.Description);
                        return new[]
                                   {
                                       new Activity(null, Resources.None),
                                       activity
                                   };
                    }
                }
                return null;
            }
        }

        public bool ModifyExtendedPreferencesIsPermitted
        {
            get
            {
                return
                    _permissionService.IsPermitted(
                        _applicationFunctionHelper.DefinedApplicationFunctionPaths.ModifyExtendedPreferences);
            }
        }

        public void SetPreference(Preference preference)
        {
            if (preference == null)
            {
                preference = new Preference
                                 {
                                     StartTimeLimitation = new TimeLimitation(new TimeOfDayValidator(false)),
                                     EndTimeLimitation = new TimeLimitation(new TimeOfDayValidator(true)),
                                     WorkTimeLimitation = new TimeLimitation(new TimeLengthValidator()),
                                     ActivityStartTimeLimitation = new TimeLimitation(new TimeOfDayValidator(false)),
                                     ActivityEndTimeLimitation = new TimeLimitation(new TimeOfDayValidator(false)),
                                     ActivityTimeLimitation = new TimeLimitation(new TimeLengthValidator()),
                                 };
            }
            Preference = preference;
            SetFieldValues();
        }

        private void SetFieldValues()
        {
            StartTimeLimitationMin = null;
            StartTimeLimitationMax = null;
            EndTimeLimitationMin = null;
            EndTimeLimitationMinNextDay = false;
            EndTimeLimitationMax = null;
            EndTimeLimitationMinNextDay = false;
            WorkTimeLimitationMin = null;
            WorkTimeLimitationMax = null;
            ActivityStartTimeLimitationMin = null;
            ActivityStartTimeLimitationMax = null;
            ActivityEndTimeLimitationMin = null;
            ActivityEndTimeLimitationMax = null;
            ActivityTimeLimitationMin = null;
            ActivityTimeLimitationMax = null;
            Activity = null;
            if (Preference.StartTimeLimitation != null)
            {
                StartTimeLimitationMin = Preference.StartTimeLimitation.MinTime;
                StartTimeLimitationMax = Preference.StartTimeLimitation.MaxTime;
            }
            if (Preference.EndTimeLimitation != null)
            {
                EndTimeLimitationMin = Preference.EndTimeLimitation.MinTime;
                EndTimeLimitationMax = Preference.EndTimeLimitation.MaxTime;
                if (EndTimeLimitationMin.HasValue &&
                    EndTimeLimitationMin.Value >= TimeSpan.FromDays(1))
                {
                    EndTimeLimitationMinNextDay = true;
                    EndTimeLimitationMin = EndTimeLimitationMin.Value.Add(TimeSpan.FromDays(-1));
                }
                if (EndTimeLimitationMax.HasValue &&
                    EndTimeLimitationMax.Value >= TimeSpan.FromDays(1))
                {
                    EndTimeLimitationMaxNextDay = true;
                    EndTimeLimitationMax = EndTimeLimitationMax.Value.Add(TimeSpan.FromDays(-1));
                }
            }
            if (Preference.WorkTimeLimitation != null)
            {
                WorkTimeLimitationMax = Preference.WorkTimeLimitation.MaxTime;
                WorkTimeLimitationMin = Preference.WorkTimeLimitation.MinTime;
            }

            DayOff = Preference.DayOff;
            ShiftCategory = Preference.ShiftCategory;
            Absence = Preference.Absence;

            if (Preference.Activity != null)
            {
                Activity = new Activity(Preference.Activity.Id, Preference.Activity.Name);
            }
            if (Preference.ActivityEndTimeLimitation != null)
            {
                ActivityEndTimeLimitationMin = Preference.ActivityEndTimeLimitation.MinTime;
                ActivityEndTimeLimitationMax = Preference.ActivityEndTimeLimitation.MaxTime;
            }
            if (Preference.ActivityStartTimeLimitation != null)
            {
                ActivityStartTimeLimitationMin = Preference.ActivityStartTimeLimitation.MinTime;
                ActivityStartTimeLimitationMax = Preference.ActivityStartTimeLimitation.MaxTime;
            }
            if (Preference.ActivityTimeLimitation != null)
            {
                ActivityTimeLimitationMin = Preference.ActivityTimeLimitation.MinTime;
                ActivityTimeLimitationMax = Preference.ActivityTimeLimitation.MaxTime;
            }
        }

        public void SetValuesToPreference()
        {
            if (Preference.StartTimeLimitation == null)
                Preference.StartTimeLimitation = new TimeLimitation(new TimeOfDayValidator(false));
            if (Preference.EndTimeLimitation == null)
                Preference.EndTimeLimitation = new TimeLimitation(new TimeOfDayValidator(true));
            if (Preference.WorkTimeLimitation == null)
                Preference.WorkTimeLimitation = new TimeLimitation(new TimeLengthValidator());

            Preference.StartTimeLimitation.MinTime = StartTimeLimitationMin;
            Preference.StartTimeLimitation.MaxTime = StartTimeLimitationMax;
            var endLimitation = EndTimeLimitationMin;
            if (endLimitation.HasValue && EndTimeLimitationMinNextDay)
                endLimitation = endLimitation.Value.Add(TimeSpan.FromDays(1));
            Preference.EndTimeLimitation.MinTime = endLimitation;
            endLimitation = EndTimeLimitationMax;
            if (endLimitation.HasValue && EndTimeLimitationMaxNextDay)
                endLimitation = endLimitation.Value.Add(TimeSpan.FromDays(1));
            Preference.EndTimeLimitation.MaxTime = endLimitation;
            Preference.WorkTimeLimitation.MinTime = WorkTimeLimitationMin;
            Preference.WorkTimeLimitation.MaxTime = WorkTimeLimitationMax;
            Preference.DayOff = DayOff;
            Preference.ShiftCategory = ShiftCategory;
            Preference.Absence = Absence;

            if (Preference.ActivityStartTimeLimitation == null)
                Preference.ActivityStartTimeLimitation = new TimeLimitation(new TimeOfDayValidator(false));
            if (Preference.ActivityEndTimeLimitation == null)
                Preference.ActivityEndTimeLimitation = new TimeLimitation(new TimeOfDayValidator(true));
            if (Preference.ActivityTimeLimitation == null)
                Preference.ActivityTimeLimitation = new TimeLimitation(new TimeLengthValidator());

            Preference.Activity = Activity;
            Preference.ActivityStartTimeLimitation.MinTime = ActivityStartTimeLimitationMin;
            Preference.ActivityStartTimeLimitation.MaxTime = ActivityStartTimeLimitationMax;
            Preference.ActivityEndTimeLimitation.MinTime = ActivityEndTimeLimitationMin;
            Preference.ActivityEndTimeLimitation.MaxTime = ActivityEndTimeLimitationMax;
            Preference.ActivityTimeLimitation.MinTime = ActivityTimeLimitationMin;
            Preference.ActivityTimeLimitation.MaxTime = ActivityTimeLimitationMax;
        }
    }
}