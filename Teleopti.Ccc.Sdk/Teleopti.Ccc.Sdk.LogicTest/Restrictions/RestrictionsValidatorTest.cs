using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class RestrictionsValidatorTest
    {
        private IPerson _person;
        private MockRepository _mocks;
        private DateOnlyPeriod _period;
        private IScheduleRange _range;
        private IScheduleDictionary _dictionary;
        private IScheduleDay _part;
        private IWorkflowControlSet _workflowControlSet;
        private RestrictionsValidator _target;
        private IIsEditablePredicate _isEditablePredicate;
        private IAssembler<IPreferenceDay, PreferenceRestrictionDto> _preferenceDayAssembler;
        private IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> _studentAvailabilityDayAssembler;
        private IMinMaxWorkTimeChecker _minMaxChecker;
    	private IPreferenceNightRestChecker _preferenceNightRestChecker;

    	[SetUp]
        public void Setup()
        {
            _period = new DateOnlyPeriod(2009,2,2,2009,3,1);
            _mocks = new MockRepository();
            _isEditablePredicate = _mocks.StrictMock<IIsEditablePredicate>();
            _preferenceDayAssembler = _mocks.StrictMock<IAssembler<IPreferenceDay, PreferenceRestrictionDto>>();
            _studentAvailabilityDayAssembler = _mocks.StrictMock<IAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto>>();
            _minMaxChecker = _mocks.StrictMock<IMinMaxWorkTimeChecker>();
        	_preferenceNightRestChecker = _mocks.StrictMock<IPreferenceNightRestChecker>();
			_target = new RestrictionsValidator(_isEditablePredicate, _preferenceDayAssembler, _studentAvailabilityDayAssembler, 
				_minMaxChecker, new CultureInfo(1053), _preferenceNightRestChecker);
            _workflowControlSet = new WorkflowControlSet("Normal");
						_person = PersonFactory.CreatePerson("mycket", "hemligt").WithId();
            _person.WorkflowControlSet = _workflowControlSet;
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
            IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2009, 1, 2));
            IRuleSetBag bag = new RuleSetBag();
            personPeriod.RuleSetBag = bag;
            _person.AddPersonPeriod(personPeriod);
            _range = _mocks.StrictMock<IScheduleRange>();
            _dictionary = _mocks.StrictMock<IScheduleDictionary>();
            _part = _mocks.StrictMock<IScheduleDay>();
        }

        [Test]
        public void ShouldThrowIfStateHolderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target.ValidateSchedulePeriod(_period, _period, null, 160, 8, 0, 0, _person, 0, 0, 0, 0, 0, 0, 0, false));
        }

        [Test]
        public void ShouldThrowIfPersonIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target.ValidateSchedulePeriod(_period, _period, _dictionary, 160, 8, 0, 0, null, 0, 0, 0, 0, 0, 0, 0, false));
        }

        [Test]
        public void CanValidatePeriodOnPerson()
        {
            var preferenceDay = new PreferenceDay(_person, new DateOnly(2009,2,2),new PreferenceRestriction() );
            var studentAvailabilityDay = new StudentAvailabilityDay(_person, new DateOnly(2008, 1, 1),new List<IStudentAvailabilityRestriction>());
            IEnumerable<IPersistableScheduleData> data = new List<IPersistableScheduleData>{preferenceDay, studentAvailabilityDay};
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            using (_mocks.Record())
            {
                _period.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsPreferenceEditable(d, _person)).Return(true));
                _period.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsStudentAvailabilityEditable(d, _person)).Return(true));
                Expect.Call(_dictionary[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(new DateOnly())).IgnoreArguments().Repeat.AtLeastOnce().Return(_part);
                Expect.Call(_part.PersistableScheduleDataCollection()).Return(data).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_part.RestrictionCollection()).Return(new List<IRestrictionBase>()).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonRestrictionCollection()).Return(new IScheduleData[0]).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonMeetingCollection()).Return(new IPersonMeeting[0]).Repeat.AtLeastOnce();
                Expect.Call(_part.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_minMaxChecker.MinMaxWorkTime(_part, ruleSetBag, effectiveRestriction, false)).Repeat.AtLeastOnce().IgnoreArguments();

                Expect.Call(_preferenceDayAssembler.DomainEntityToDto(preferenceDay)).Return(
                    new PreferenceRestrictionDto()).Repeat.AtLeastOnce();

                Expect.Call(_studentAvailabilityDayAssembler.DomainEntityToDto(studentAvailabilityDay)).Return(
                    new StudentAvailabilityDayDto()).Repeat.AtLeastOnce();
				Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(new List<ValidatedSchedulePartDto>())).
					IgnoreArguments();
            }

            using(_mocks.Playback())
            {
                IList<ValidatedSchedulePartDto> result = _target.ValidateSchedulePeriod(_period, _period, _dictionary, 160, 8, 0, 0, _person, 0, 0, 0, 0, 0, 0, 0, false);
                Assert.IsNotNull(result);
                Assert.AreNotEqual(0, result.Count);
            }
        }

        [Test]
       public void CanCheckPersonDayOffFromPeriod()
        {
           var partWithDayOff = _mocks.StrictMock<IScheduleDay>();
           var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, new Scenario("d"),new DateOnly(), new DayOffTemplate());
           IEnumerable<IPersistableScheduleData> data = new List<IPersistableScheduleData> ();
           var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
           var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
           using (_mocks.Record())
           {
               _period.DayCollection().ForEach(
                   d => Expect.Call(_isEditablePredicate.IsPreferenceEditable(d, _person)).Return(true));
               _period.DayCollection().ForEach(
                   d => Expect.Call(_isEditablePredicate.IsStudentAvailabilityEditable(d, _person)).Return(true));
               Expect.Call(_dictionary[_person]).Return(_range).Repeat.AtLeastOnce();
               Expect.Call(_range.ScheduledDay(new DateOnly())).IgnoreArguments().Repeat.AtLeastOnce().Return(partWithDayOff);
              
               Expect.Call(partWithDayOff.PersistableScheduleDataCollection()).Return(data).Repeat.AtLeastOnce();
               Expect.Call(partWithDayOff.PersonAssignment()).Return(personDayOff).Repeat.AtLeastOnce();
               Expect.Call(partWithDayOff.RestrictionCollection()).Return(new List<IRestrictionBase>()).Repeat.
                   AtLeastOnce();
               Expect.Call(partWithDayOff.PersonRestrictionCollection()).Return(new IScheduleData[0]).Repeat.AtLeastOnce();
               Expect.Call(partWithDayOff.PersonMeetingCollection()).Return(new IPersonMeeting[0]).Repeat.AtLeastOnce();
               Expect.Call(partWithDayOff.SignificantPartForDisplay()).Return(SchedulePartView.DayOff).Repeat.AtLeastOnce();
               Expect.Call(_minMaxChecker.MinMaxWorkTime(_part, ruleSetBag, effectiveRestriction, false)).Repeat.AtLeastOnce().IgnoreArguments();
			  Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(new List<ValidatedSchedulePartDto>())).
				   IgnoreArguments();
           }

           using (_mocks.Playback())
           {
               IList<ValidatedSchedulePartDto> result = _target.ValidateSchedulePeriod(_period, _period, _dictionary, 160, 8, 0, 0, _person, 0, 0, 0, 0, 0, 0, 0, false);
               Assert.IsNotNull(result);
               Assert.AreNotEqual(0, result.Count);
           }
       }
        [Test]
        public void CanAddFullDayAbsenceFromPeriod()
        {
            IAbsence payload = AbsenceFactory.CreateAbsence("abs");
            payload.Description = new Description("borta", "bra");
            payload.DisplayColor = Color.Green;
            IVisualLayerCollection visualLayerCollection = VisualLayerCollectionFactory.CreateForAbsence(TimeSpan.FromHours(5), TimeSpan.FromHours(17));
            var projService = _mocks.StrictMock<IProjectionService>();
            var partWithFullDayAbcence = _mocks.StrictMock<IScheduleDay>();
            IEnumerable<IPersistableScheduleData> data = new List<IPersistableScheduleData>();
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            using (_mocks.Record())
            {
                _period.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsPreferenceEditable(d, _person)).Return(true));
                _period.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsStudentAvailabilityEditable(d, _person)).Return(true));
                Expect.Call(_dictionary[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(new DateOnly())).IgnoreArguments().Repeat.AtLeastOnce().Return(partWithFullDayAbcence);
                Expect.Call(partWithFullDayAbcence.PersistableScheduleDataCollection()).Return(data).Repeat.AtLeastOnce();
                Expect.Call(partWithFullDayAbcence.PersonAssignment()).Return(null).Repeat.AtLeastOnce();
                Expect.Call(partWithFullDayAbcence.RestrictionCollection()).Return(new List<IRestrictionBase>()).Repeat.AtLeastOnce();
                Expect.Call(partWithFullDayAbcence.PersonRestrictionCollection()).Return(new IScheduleData[0]).Repeat.AtLeastOnce();
                Expect.Call(partWithFullDayAbcence.PersonMeetingCollection()).Return(new IPersonMeeting[0]).Repeat.AtLeastOnce();
                Expect.Call(partWithFullDayAbcence.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence).Repeat.AtLeastOnce();
                Expect.Call(partWithFullDayAbcence.ProjectionService()).Return(projService).Repeat.AtLeastOnce();
                Expect.Call(projService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(_minMaxChecker.MinMaxWorkTime(_part, ruleSetBag, effectiveRestriction, false)).Repeat.AtLeastOnce().IgnoreArguments();
				Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(new List<ValidatedSchedulePartDto>())).
					IgnoreArguments();
            }

            using (_mocks.Playback())
            {
                IList<ValidatedSchedulePartDto> result = _target.ValidateSchedulePeriod(_period, _period, _dictionary, 160, 8, 0, 0, _person, 0, 0, 0, 0, 0, 0, 0, false);
                Assert.IsNotNull(result);
                Assert.AreNotEqual(0, result.Count);
            }
        }

        [Test]
        public void CanTellIfOnlyPersonalAssignmentExists()
        {
        	var dateOnly = new DateOnly(2009, 2, 2);
            var preferenceDay = new PreferenceDay(_person, dateOnly, new PreferenceRestriction());
            IEnumerable<IPersistableScheduleData> data = new List<IPersistableScheduleData> { preferenceDay };
            IMeetingPerson meetingPerson = new MeetingPerson(_person, false);
            IPerson organizer = PersonFactory.CreatePerson("Organizer");
            IList<IMeetingPerson> meetingPeople = new List<IMeetingPerson> { meetingPerson };
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            IActivity activity = ActivityFactory.CreateActivity("Meeting");
            IMeeting meeting = new Meeting(organizer, meetingPeople, "Metting", "", "", activity, scenario);

            var period = new DateTimePeriod(new DateTime(2009, 2, 2, 6, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 7, 0, 0, DateTimeKind.Utc));
            var largePeriod = new DateOnlyPeriod(2009, 1, 26, 2009, 2, 10);
            IPersonMeeting personMeeting = new PersonMeeting(meeting, meetingPerson, period);
            var meetings = new [] { personMeeting };
            IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(_person,
                                                                                                           scenario, activity, period);
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
        	var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(dateOnly,TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

            using (_mocks.Record())
            {
                largePeriod.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsPreferenceEditable(d, _person)).Return(true));
                largePeriod.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsStudentAvailabilityEditable(d, _person)).Return(true));
                Expect.Call(_dictionary[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(dateOnly)).IgnoreArguments().Repeat.AtLeastOnce().Return(_part);
	            Expect.Call(_part.DateOnlyAsPeriod).Return(dateOnlyAsPeriod).Repeat.Any();
                Expect.Call(_part.PersistableScheduleDataCollection()).Return(data).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_part.RestrictionCollection()).Return(new List<IRestrictionBase>()).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonRestrictionCollection()).Return(new IScheduleData[0]).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                Expect.Call(_part.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_minMaxChecker.MinMaxWorkTime(_part, ruleSetBag, effectiveRestriction, false)).Repeat.AtLeastOnce().IgnoreArguments();
                Expect.Call(_preferenceDayAssembler.DomainEntityToDto(preferenceDay)).Return(
                    new PreferenceRestrictionDto()).Repeat.AtLeastOnce();
				Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(new List<ValidatedSchedulePartDto>())).
					IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                IList<ValidatedSchedulePartDto> result = _target.ValidateSchedulePeriod(largePeriod, largePeriod, _dictionary, 160, 8, 0, 0, _person, 0, 0, 0, 0, 0, 0, 0, false);
                Assert.IsNotNull(result);
                for (int i = 0; i < result.Count - 1; i++)
                {
                    Assert.IsTrue(result[i].HasPersonalAssignmentOnly);
                }
                Assert.AreEqual(16, result.Count);
            }    
        }

        [Test]
        public void HasPersonalAssignmentOnlyReturnsFalseWhenPersonalAndShift()
        {
            var preferenceDay = new PreferenceDay(_person, new DateOnly(2009, 2, 2), new PreferenceRestriction());
            IEnumerable<IPersistableScheduleData> data = new List<IPersistableScheduleData> { preferenceDay };
            var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            IWorkTimeMinMax minMax = new WorkTimeMinMax
                                         {
                                             StartTimeLimitation =
                                                 new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(11)),
                                             EndTimeLimitation =
                                                 new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(20)),
                                             WorkTimeLimitation =
                                                 new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
                                         };
            IMeetingPerson meetingPerson = new MeetingPerson(_person, false);
            IPerson organizer = PersonFactory.CreatePerson("Organizer");
            IList<IMeetingPerson> meetingPeople = new List<IMeetingPerson> { meetingPerson };
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            IActivity activity = ActivityFactory.CreateActivity("Meeting");
            IMeeting meeting = new Meeting(organizer, meetingPeople, "Metting", "", "", activity, scenario);

            var period = new DateTimePeriod(new DateTime(2009, 2, 2, 6, 0, 0, DateTimeKind.Utc), new DateTime(2009, 2, 2, 7, 0, 0, DateTimeKind.Utc));
            var largePeriod = new DateOnlyPeriod(2009, 1, 26, 2009, 2, 10);
            IPersonMeeting personMeeting = new PersonMeeting(meeting, meetingPerson, period);
            var meetings = new [] { personMeeting };
            IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("day");
            IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_person, scenario, activity, period, category);

            using (_mocks.Record())
            {
                largePeriod.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsPreferenceEditable(d, _person)).Return(true));
                largePeriod.DayCollection().ForEach(d => Expect.Call(_isEditablePredicate.IsStudentAvailabilityEditable(d, _person)).Return(true));
                Expect.Call(_dictionary[_person]).Return(_range).Repeat.AtLeastOnce();
                Expect.Call(_range.ScheduledDay(new DateOnly())).IgnoreArguments().Repeat.AtLeastOnce().Return(_part);
                Expect.Call(_part.PersistableScheduleDataCollection()).Return(data).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_part.RestrictionCollection()).Return(new List<IRestrictionBase>()).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonRestrictionCollection()).Return(new IScheduleData[0]).Repeat.AtLeastOnce();
                Expect.Call(_part.PersonMeetingCollection()).Return(meetings).Repeat.AtLeastOnce();
                
                Expect.Call(_minMaxChecker.MinMaxWorkTime(_part, ruleSetBag, effectiveRestriction, false)).Return(minMax).
                    Repeat.AtLeastOnce().IgnoreArguments();
                Expect.Call(_part.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
                Expect.Call(_preferenceDayAssembler.DomainEntityToDto(preferenceDay)).Return(
                    new PreferenceRestrictionDto()).Repeat.AtLeastOnce();
            	Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(new List<ValidatedSchedulePartDto>())).
            		IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                IList<ValidatedSchedulePartDto> result = _target.ValidateSchedulePeriod(largePeriod, largePeriod, _dictionary, 160, 8, 0, 0, _person, 0, 0, 0, 0, 0, 0, 0, false);
                Assert.IsNotNull(result);
                Assert.AreEqual("day", result[0].ScheduledItemName);
                Assert.IsTrue(result[0].HasShift);
                for (int i = 0; i < result.Count - 1; i++)
                {
                    Assert.IsFalse(result[i].HasPersonalAssignmentOnly);
                }
                Assert.AreEqual(16, result.Count);
            }
            
        }

        [Test]
        public void CanAddDaysToMatchFirstDayOfWeekIfAgentFirstPersonPeriodStartsOnOtherDay()
        {
            var baseDate = new DateTime(2009, 2, 1);
            var validatedSchedulePartDto = new ValidatedSchedulePartDto
                                               {
												   DateOnly = new DateOnlyDto { DateTime = baseDate.Date }
                                               };
            IList<ValidatedSchedulePartDto> partsOrg = new List<ValidatedSchedulePartDto>{validatedSchedulePartDto};

          
            var culture = new CultureInfo(1053);

			var validator = new RestrictionsValidator(null, null, null, null, culture, _preferenceNightRestChecker);
      
            validator.TrimValidatedPartListToFirstDayOfWeek(partsOrg);

            Assert.AreEqual(DayOfWeek.Monday, culture.DateTimeFormat.FirstDayOfWeek);
            Assert.AreEqual(DayOfWeek.Sunday, baseDate.Date.DayOfWeek);
            Assert.AreEqual(DayOfWeek.Monday, baseDate.AddDays(-6).DayOfWeek);
            Assert.AreEqual(7, partsOrg.Count);
            Assert.AreEqual(baseDate.AddDays(-6), partsOrg[0].DateOnly.DateTime);
            Assert.AreEqual(baseDate.AddDays(-5), partsOrg[1].DateOnly.DateTime);
            Assert.AreEqual(baseDate.AddDays(-4), partsOrg[2].DateOnly.DateTime);
            Assert.AreEqual(baseDate.AddDays(-3), partsOrg[3].DateOnly.DateTime);
            Assert.AreEqual(baseDate.AddDays(-2), partsOrg[4].DateOnly.DateTime);
            Assert.AreEqual(baseDate.AddDays(-1), partsOrg[5].DateOnly.DateTime);
            Assert.AreEqual(baseDate, partsOrg[6].DateOnly.DateTime);
        }
    }
}
