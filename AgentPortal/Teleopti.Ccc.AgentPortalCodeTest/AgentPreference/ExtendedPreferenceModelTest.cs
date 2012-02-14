using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentPreference;
using Teleopti.Ccc.AgentPortalCode.AgentPreference.Limitation;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class ExtendedPreferenceModelTest
    {
        private ExtendedPreferenceModel _target;
        private MockRepository _mocks;
        private ISessionData _sessionData;
        private IPermissionService _permissionService;
        private IApplicationFunctionHelper _applicationFunctionHelper;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _sessionData = _mocks.StrictMock<ISessionData>();
            _permissionService = _mocks.StrictMock<IPermissionService>();
            _applicationFunctionHelper = _mocks.StrictMock<IApplicationFunctionHelper>();
            _target = new ExtendedPreferenceModel(_sessionData, _permissionService, _applicationFunctionHelper);
        }

        [Test]
        public void ShouldInitializeWithPreferenceValues()
        {
            var preference = new Preference();
            preference.DayOff = null;
            preference.Absence = null;
            preference.ShiftCategory = new ShiftCategory("shiftC", "sc", "id", Color.Black);
            //preference.Templates = new Templates("templateC", "tp", "idtp", Color.Blue);
            preference.StartTimeLimitation = new TimeLimitation(null)
                                                 {MaxTime = TimeSpan.FromHours(5), MinTime = TimeSpan.FromHours(10)};
            preference.EndTimeLimitation = new TimeLimitation(null)
                                               {MaxTime = TimeSpan.FromHours(16), MinTime = TimeSpan.FromHours(17)};
            preference.WorkTimeLimitation = new TimeLimitation(null)
                                                {MaxTime = TimeSpan.FromHours(1), MinTime = TimeSpan.FromHours(10)};
            preference.Activity = new Activity(Guid.NewGuid().ToString(), "act");
            preference.ActivityStartTimeLimitation = new TimeLimitation(null)
                                                         {
                                                             MaxTime = TimeSpan.FromHours(10),
                                                             MinTime = TimeSpan.FromHours(13)
                                                         };
            preference.ActivityEndTimeLimitation = new TimeLimitation(null)
                                                       {
                                                           MaxTime = TimeSpan.FromHours(11),
                                                           MinTime = TimeSpan.FromHours(14)
                                                       };
            preference.ActivityTimeLimitation = new TimeLimitation(null)
                                                    {MaxTime = TimeSpan.FromHours(1), MinTime = TimeSpan.FromHours(2)};

            _target.SetPreference(preference);

            Assert.That(_target.DayOff, Is.Null);
            Assert.That(_target.Absence, Is.Null);
            Assert.That(_target.ShiftCategory, Is.EqualTo(preference.ShiftCategory));
            //Assert.That(_target.Templates, Is.Null);
            Assert.That(_target.StartTimeLimitationMin, Is.EqualTo(preference.StartTimeLimitation.MinTime));
            Assert.That(_target.StartTimeLimitationMax, Is.EqualTo(preference.StartTimeLimitation.MaxTime));
            Assert.That(_target.EndTimeLimitationMin, Is.EqualTo(preference.EndTimeLimitation.MinTime));
            Assert.That(_target.EndTimeLimitationMinNextDay, Is.False);
            Assert.That(_target.EndTimeLimitationMax, Is.EqualTo(preference.EndTimeLimitation.MaxTime));
            Assert.That(_target.EndTimeLimitationMaxNextDay, Is.False);
            Assert.That(_target.WorkTimeLimitationMax, Is.EqualTo(preference.WorkTimeLimitation.MaxTime));
            Assert.That(_target.WorkTimeLimitationMin, Is.EqualTo(preference.WorkTimeLimitation.MinTime));
            Assert.That(_target.Activity, Is.Not.SameAs(preference.Activity));
            Assert.That(_target.Activity.Id, Is.EqualTo(preference.Activity.Id));
            Assert.That(_target.ActivityStartTimeLimitationMin,
                        Is.EqualTo(preference.ActivityStartTimeLimitation.MinTime));
            Assert.That(_target.ActivityStartTimeLimitationMax,
                        Is.EqualTo(preference.ActivityStartTimeLimitation.MaxTime));
            Assert.That(_target.ActivityEndTimeLimitationMin, Is.EqualTo(preference.ActivityEndTimeLimitation.MinTime));
            Assert.That(_target.ActivityEndTimeLimitationMax, Is.EqualTo(preference.ActivityEndTimeLimitation.MaxTime));
            Assert.That(_target.ActivityTimeLimitationMin, Is.EqualTo(preference.ActivityTimeLimitation.MinTime));
            Assert.That(_target.ActivityTimeLimitationMax, Is.EqualTo(preference.ActivityTimeLimitation.MaxTime));
        }

        [Test]
        public void ShouldSetPreferenceValues()
        {
            _target.SetPreference(new Preference());

            _target.DayOff = new DayOff("dayoff", "do", "id", Color.Black);
            _target.Absence = new Absence("name", "shortName", "id", Color.Empty);
            _target.StartTimeLimitationMax = TimeSpan.FromHours(10);
            _target.StartTimeLimitationMin = TimeSpan.FromHours(6);
            _target.EndTimeLimitationMin = TimeSpan.FromHours(16);
            _target.EndTimeLimitationMinNextDay = false;
            _target.EndTimeLimitationMax = TimeSpan.FromHours(20);
            _target.EndTimeLimitationMaxNextDay = false;
            _target.ShiftCategory = new ShiftCategory("shiftC", "sc", "id", Color.Black);
            //_target.Templates = new Templates("tmpC", "tmp", "idtmp", Color.Blue);
            _target.WorkTimeLimitationMax = TimeSpan.FromHours(1);
            _target.WorkTimeLimitationMin = TimeSpan.FromHours(10);
            _target.Activity = new Activity(Guid.NewGuid().ToString(), "act");
            _target.ActivityStartTimeLimitationMin = TimeSpan.FromHours(10);
            _target.ActivityStartTimeLimitationMax = TimeSpan.FromHours(13);
            _target.ActivityEndTimeLimitationMin = TimeSpan.FromHours(11);
            _target.ActivityEndTimeLimitationMax = TimeSpan.FromHours(14);
            _target.ActivityTimeLimitationMin = TimeSpan.FromHours(1);
            _target.ActivityTimeLimitationMax = TimeSpan.FromHours(2);

            _target.SetValuesToPreference();
            var preference = _target.Preference;

            Assert.That(preference.DayOff, Is.EqualTo(_target.DayOff));
            Assert.That(preference.Absence, Is.EqualTo(_target.Absence));
            Assert.That(preference.ShiftCategory, Is.EqualTo(_target.ShiftCategory));
            //Assert.That(preference.Templates, Is.Null);
            Assert.That(preference.StartTimeLimitation.MinTime, Is.EqualTo(_target.StartTimeLimitationMin));
            Assert.That(preference.StartTimeLimitation.MaxTime, Is.EqualTo(_target.StartTimeLimitationMax));
            Assert.That(preference.EndTimeLimitation.MinTime, Is.EqualTo(_target.EndTimeLimitationMin));
            Assert.That(preference.EndTimeLimitation.MaxTime, Is.EqualTo(_target.EndTimeLimitationMax));
            Assert.That(preference.WorkTimeLimitation.MaxTime, Is.EqualTo(_target.WorkTimeLimitationMax));
            Assert.That(preference.WorkTimeLimitation.MinTime, Is.EqualTo(_target.WorkTimeLimitationMin));
            Assert.That(preference.Activity, Is.EqualTo(_target.Activity));
            Assert.That(preference.Activity.Id, Is.EqualTo(_target.Activity.Id));
            Assert.That(preference.ActivityStartTimeLimitation.MinTime,
                        Is.EqualTo(_target.ActivityStartTimeLimitationMin));
            Assert.That(preference.ActivityStartTimeLimitation.MaxTime,
                        Is.EqualTo(_target.ActivityStartTimeLimitationMax));
            Assert.That(preference.ActivityEndTimeLimitation.MinTime, Is.EqualTo(_target.ActivityEndTimeLimitationMin));
            Assert.That(preference.ActivityEndTimeLimitation.MaxTime, Is.EqualTo(_target.ActivityEndTimeLimitationMax));
            Assert.That(preference.ActivityTimeLimitation.MinTime, Is.EqualTo(_target.ActivityTimeLimitationMin));
            Assert.That(preference.ActivityTimeLimitation.MaxTime, Is.EqualTo(_target.ActivityTimeLimitationMax));
        }

        [Test]
        public void ShouldSetFieldValuesFromPreference()
        {
            var preference = new Preference
                                 {
                                     StartTimeLimitation = new TimeLimitation(null) {MaxTime = TimeSpan.FromHours(5), MinTime = TimeSpan.FromHours(10)},
                                     EndTimeLimitation = new TimeLimitation(null) {MaxTime = TimeSpan.FromHours(6), MinTime = TimeSpan.FromHours(11)},
                                     WorkTimeLimitation = new TimeLimitation(null) {MaxTime = TimeSpan.FromHours(7), MinTime = TimeSpan.FromHours(12)},
                                     Activity = new Activity("id", "act"),
                                     ActivityStartTimeLimitation = new TimeLimitation(null) {MaxTime = TimeSpan.FromHours(8), MinTime = TimeSpan.FromHours(13)},
                                     ActivityEndTimeLimitation = new TimeLimitation(null) {MaxTime = TimeSpan.FromHours(9), MinTime = TimeSpan.FromHours(14)},
                                     ActivityTimeLimitation = new TimeLimitation(null) {MaxTime = TimeSpan.FromHours(10), MinTime = TimeSpan.FromHours(15)}
                                 };

            _target.SetPreference(preference);

            Assert.That(_target.StartTimeLimitationMin, Is.EqualTo(preference.StartTimeLimitation.MinTime));
            Assert.That(_target.StartTimeLimitationMax, Is.EqualTo(preference.StartTimeLimitation.MaxTime));
            Assert.That(_target.EndTimeLimitationMin, Is.EqualTo(preference.EndTimeLimitation.MinTime));
            Assert.That(_target.EndTimeLimitationMax, Is.EqualTo(preference.EndTimeLimitation.MaxTime));
            Assert.That(_target.WorkTimeLimitationMax, Is.EqualTo(preference.WorkTimeLimitation.MaxTime));
            Assert.That(_target.WorkTimeLimitationMin, Is.EqualTo(preference.WorkTimeLimitation.MinTime));
            Assert.That(_target.Activity, Is.EqualTo(_target.Activity));
            Assert.That(_target.Activity.Id, Is.EqualTo(_target.Activity.Id));
            Assert.That(_target.ActivityStartTimeLimitationMin, Is.EqualTo(preference.ActivityStartTimeLimitation.MinTime));
            Assert.That(_target.ActivityStartTimeLimitationMax, Is.EqualTo(preference.ActivityStartTimeLimitation.MaxTime));
            Assert.That(_target.ActivityEndTimeLimitationMin, Is.EqualTo(preference.ActivityEndTimeLimitation.MinTime));
            Assert.That(_target.ActivityEndTimeLimitationMax, Is.EqualTo(preference.ActivityEndTimeLimitation.MaxTime));
            Assert.That(_target.ActivityTimeLimitationMin, Is.EqualTo(preference.ActivityTimeLimitation.MinTime));
            Assert.That(_target.ActivityTimeLimitationMax, Is.EqualTo(preference.ActivityTimeLimitation.MaxTime));

            preference = new Preference
                                {
                                    DayOff = new DayOff("dayoff", "do", "id", Color.Black)
                                };

            _target.SetPreference(preference);

            Assert.That(_target.DayOff, Is.EqualTo(preference.DayOff));

            preference = new Preference
                                {
                                    Absence = new Absence("name", "shortName", "id", Color.Empty)
                                };

            _target.SetPreference(preference);

            Assert.That(_target.Absence, Is.EqualTo(preference.Absence));

            preference = new Preference
                                {
                                    ShiftCategory = new ShiftCategory("shiftC", "sc", "id", Color.Black)
                                };

            _target.SetPreference(preference);

            Assert.That(_target.ShiftCategory, Is.EqualTo(preference.ShiftCategory));  
        }

        [Test]
        public void ShouldClearDayOffAndAbsenceWhenSettingShiftCategory()
        {
            _target.Absence = new Absence("name", "shortName", "id", Color.Empty);
            _target.ShiftCategory = new ShiftCategory("name", "shortName", "id", Color.Empty);
            
            Assert.IsNull(_target.Absence);

            _target.DayOff = new DayOff("name", "shortName", "id", Color.Empty);
            _target.ShiftCategory = new ShiftCategory("name", "shortName", "id", Color.Empty);

            Assert.IsNull(_target.DayOff);
        }

        [Test]
        public void ShouldClearAllWhenSettingDayOff()
        {
            _target.StartTimeLimitationMax = TimeSpan.FromHours(10);
            _target.StartTimeLimitationMin = TimeSpan.FromHours(6);
            _target.EndTimeLimitationMin = TimeSpan.FromHours(16);
            _target.EndTimeLimitationMinNextDay = false;
            _target.EndTimeLimitationMax = TimeSpan.FromHours(20);
            _target.EndTimeLimitationMaxNextDay = false;
            _target.WorkTimeLimitationMax = TimeSpan.FromHours(1);
            _target.WorkTimeLimitationMin = TimeSpan.FromHours(10);
            _target.Activity = new Activity(Guid.NewGuid().ToString(), "act");
            _target.ActivityStartTimeLimitationMin = TimeSpan.FromHours(10);
            _target.ActivityStartTimeLimitationMax = TimeSpan.FromHours(13);
            _target.ActivityEndTimeLimitationMin = TimeSpan.FromHours(11);
            _target.ActivityEndTimeLimitationMax = TimeSpan.FromHours(14);
            _target.ActivityTimeLimitationMin = TimeSpan.FromHours(1);
            _target.ActivityTimeLimitationMax = TimeSpan.FromHours(2);

            _target.DayOff = new DayOff("name", "shortName", "id", Color.Empty);

            Assert.IsNull(_target.StartTimeLimitationMax);
            Assert.IsNull(_target.StartTimeLimitationMin);
            Assert.IsNull(_target.EndTimeLimitationMin);
            Assert.IsNull(_target.EndTimeLimitationMax);
            Assert.IsNull(_target.WorkTimeLimitationMax);
            Assert.IsNull(_target.WorkTimeLimitationMin);
            Assert.IsNull(_target.Activity);
            Assert.IsNull(_target.ActivityStartTimeLimitationMin);
            Assert.IsNull(_target.ActivityStartTimeLimitationMax);
            Assert.IsNull(_target.ActivityEndTimeLimitationMin);
            Assert.IsNull(_target.ActivityEndTimeLimitationMax);
            Assert.IsNull(_target.ActivityTimeLimitationMin);
            Assert.IsNull(_target.ActivityTimeLimitationMax);

            _target.Absence = new Absence("name", "shortName", "id", Color.Empty);
            _target.DayOff = new DayOff("name", "shortName", "id", Color.Empty);

            Assert.IsNull(_target.Absence);

            _target.ShiftCategory = new ShiftCategory("name", "shortName", "id", Color.Empty);
            _target.DayOff = new DayOff("name", "shortName", "id", Color.Empty);

            Assert.IsNull(_target.ShiftCategory);
        }

        [Test]
        public void ShouldClearAllWhenSettingAbsence()
        {
            _target.StartTimeLimitationMax = TimeSpan.FromHours(10);
            _target.StartTimeLimitationMin = TimeSpan.FromHours(6);
            _target.EndTimeLimitationMin = TimeSpan.FromHours(16);
            _target.EndTimeLimitationMinNextDay = false;
            _target.EndTimeLimitationMax = TimeSpan.FromHours(20);
            _target.EndTimeLimitationMaxNextDay = false;
            _target.WorkTimeLimitationMax = TimeSpan.FromHours(1);
            _target.WorkTimeLimitationMin = TimeSpan.FromHours(10);
            _target.Activity = new Activity(Guid.NewGuid().ToString(), "act");
            _target.ActivityStartTimeLimitationMin = TimeSpan.FromHours(10);
            _target.ActivityStartTimeLimitationMax = TimeSpan.FromHours(13);
            _target.ActivityEndTimeLimitationMin = TimeSpan.FromHours(11);
            _target.ActivityEndTimeLimitationMax = TimeSpan.FromHours(14);
            _target.ActivityTimeLimitationMin = TimeSpan.FromHours(1);
            _target.ActivityTimeLimitationMax = TimeSpan.FromHours(2);

            _target.Absence = new Absence("name", "shortName", "id", Color.Empty);

            Assert.IsNull(_target.StartTimeLimitationMax);
            Assert.IsNull(_target.StartTimeLimitationMin);
            Assert.IsNull(_target.EndTimeLimitationMin);
            Assert.IsNull(_target.EndTimeLimitationMax);
            Assert.IsNull(_target.WorkTimeLimitationMax);
            Assert.IsNull(_target.WorkTimeLimitationMin);
            Assert.IsNull(_target.Activity);
            Assert.IsNull(_target.ActivityStartTimeLimitationMin);
            Assert.IsNull(_target.ActivityStartTimeLimitationMax);
            Assert.IsNull(_target.ActivityEndTimeLimitationMin);
            Assert.IsNull(_target.ActivityEndTimeLimitationMax);
            Assert.IsNull(_target.ActivityTimeLimitationMin);
            Assert.IsNull(_target.ActivityTimeLimitationMax);

            _target.DayOff = new DayOff("name", "shortName", "id", Color.Empty);
            _target.Absence = new Absence("name", "shortName", "id", Color.Empty);
            
            Assert.IsNull(_target.DayOff);

            _target.ShiftCategory = new ShiftCategory("name", "shortName", "id", Color.Empty);
            _target.Absence = new Absence("name", "shortName", "id", Color.Empty);

            Assert.IsNull(_target.ShiftCategory);
        }

        [Test]
        public void ShouldClearAbsenceDayOffWhenSettingActivity()
        {

            _target.Absence = new Absence("name", "shortName", "id", Color.Empty);
            _target.Activity = new Activity(Guid.NewGuid().ToString(), "act");
            Assert.IsNull(_target.Absence);

            _target.DayOff = new DayOff("name", "shortName", "id", Color.Empty);
            _target.Activity = new Activity(Guid.NewGuid().ToString(), "act");
            Assert.IsNull(_target.DayOff); 
        }

        [Test]
        public void ShouldProvideAllowedPreferenceActivities()
        {
            var person = new PersonDto
                             {
                                 WorkflowControlSet = new WorkflowControlSetDto
                                                          {
                                                              AllowedPreferenceActivity = new ActivityDto()
                                                          }
                             };

            _mocks.Record();

            Expect.Call(_sessionData.LoggedOnPerson).Return(person);

            _mocks.ReplayAll();

            var activities = _target.AllowedPreferenceActivities;

            _mocks.VerifyAll();

            Assert.That(activities.Count(), Is.EqualTo(2));
            Assert.That(activities.First().Id, Is.Null);
            Assert.That(activities.First().Name, Is.EqualTo(Resources.None));
            Assert.That(activities.ElementAt(1).Id, Is.EqualTo(person.WorkflowControlSet.AllowedPreferenceActivity.Id));
            Assert.That(activities.ElementAt(1).Name, Is.EqualTo(person.WorkflowControlSet.AllowedPreferenceActivity.Description));
        }

        [Test]
        public void ShouldProvidePermissionIndication()
        {
            _mocks.Record();

            var definedApplicationPahts = new DefinedRaptorApplicationFunctionPathsDto();
            definedApplicationPahts.ModifyExtendedPreferences = "path";

            Expect.Call(_applicationFunctionHelper.DefinedApplicationFunctionPaths)
                .Return(definedApplicationPahts);
            Expect.Call(
                _permissionService.IsPermitted("path"))
                    .Return(true);

            _mocks.ReplayAll();

            var permitted = _target.ModifyExtendedPreferencesIsPermitted;

            _mocks.VerifyAll();

            Assert.That(permitted, Is.True);
        }
    }
}