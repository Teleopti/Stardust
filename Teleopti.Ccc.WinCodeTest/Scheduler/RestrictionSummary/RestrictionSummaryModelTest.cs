﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.RestrictionSummary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture]
    public class RestrictionSummaryModelTest
    {
        private RestrictionSummaryModel _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private MockRepository _mocks;
        private RestrictionSchedulingOptions _options;
        private IPerson _person;
        private DateTime _dateTime;
        private ICccTimeZoneInfo _timeZoneInfo;
        private IScenario _scenario;
        private DateTimePeriod _dateTimePeriod;
        private IScheduleRange _range;
        private IDictionary<IPerson, IScheduleRange> _dictionary;
        private IScheduleDateTimePeriod _scheduleDateTimePeriod;
        private IScheduleDictionary _scheduleDictionary;
        private ISchedulerStateHolder _stateHolder;
        private IRuleSetProjectionService _ruleSetProjectionService;
        private ISchedulingResultStateHolder _resultStateHolder;
    	private IPreferenceNightRestChecker _preferenceNightRestChecker;


    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            ((Regional)TeleoptiPrincipal.Current.Regional).Culture = new CultureInfo("sv-SE");
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            _resultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_preferenceNightRestChecker = _mocks.StrictMock<IPreferenceNightRestChecker>();
			_target = new RestrictionSummaryModel(_schedulingResultStateHolder, new RuleSetProjectionService(new ShiftCreatorService()), _stateHolder, _preferenceNightRestChecker);
            _options = new RestrictionSchedulingOptions
                           {
                               UseScheduling = true,
                               UsePreferences = true,
                               UseRotations = true,
                               UseStudentAvailability = true,
                               UseAvailability = true
                           };
            _dateTime = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ISchedulePeriod schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(_dateTime));
            _timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Utc"));

            _person = PersonFactory.CreatePerson("Jens");
            _person.AddSchedulePeriod(schedulePeriod);
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _dateTimePeriod = new DateTimePeriod(_dateTime, _dateTime.AddDays(30));
            _range = _mocks.StrictMock<IScheduleRange>();
            _dictionary = new Dictionary<IPerson, IScheduleRange> {{_person, _range}};
            _scheduleDateTimePeriod = new ScheduleDateTimePeriod(_dateTimePeriod);
            
            _scheduleDictionary = new ScheduleDictionaryForTest(_scenario, _scheduleDateTimePeriod, _dictionary);
            _ruleSetProjectionService = _mocks.StrictMock<IRuleSetProjectionService>();

        }

        [Test]
        public void CanGetCellDataCollection()
        {
            Assert.IsNotNull(_target.CellDataCollection);
        }

        [Test]
        public void VerifyGetNextPeriodSetsLastLoadedHelper()
        {
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(_stateHolder.TimeZoneInfo).Return(_timeZoneInfo).Repeat.Any();
			Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
            _mocks.ReplayAll();
            IPersonPeriod period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateTime.AddDays(-10)));
            _person.AddPersonPeriod(period);
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            _target.GetNextPeriod(helper);
            _mocks.VerifyAll();
        }
       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanLoadPeriod()
        {
            IMainShift mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
            part.AddMainShift(mainShift);
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(_stateHolder.TimeZoneInfo).Return(_timeZoneInfo).Repeat.Any();
        	Expect.Call(() =>_preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
            _mocks.ReplayAll();
            IPersonPeriod period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateTime.AddDays(-10)));
            double maxWeekHours = period.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek.TotalHours;
            _person.AddPersonPeriod(period);
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            _target.LoadPeriod(helper);
            Assert.AreEqual(21, _target.CellDataCollection.Count);
            Assert.AreEqual(maxWeekHours, _target.CellDataCollection[0].WeeklyMax.TotalHours);
            _mocks.VerifyAll();
        }


        [Test]
        public void CanLoadPeriodWithDayOff()
        {
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));

            var dayOff = new DayOffTemplate(new Description("test"));
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
            dayOff.Anchor = TimeSpan.FromHours(12);
            var personDayOff = new PersonDayOff(_person, _scenario, dayOff, new DateOnly(_dateTime));
            part.Add(personDayOff);
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(_stateHolder.TimeZoneInfo).Return(_timeZoneInfo).Repeat.Any();
			Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
            _mocks.ReplayAll(); 
            IPersonPeriod period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateTime.AddDays(-10)));
            _person.AddPersonPeriod(period);
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            _target.LoadPeriod(helper);
            _mocks.VerifyAll();
        }
        [Test]
        public void CanLoadPeriodWithAbsence()
        {
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
            IAbsence absence = AbsenceFactory.CreateAbsence("Sick");
            IAbsenceLayer layer = new AbsenceLayer(absence, _dateTimePeriod);
            part.CreateAndAddAbsence(layer);
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(_stateHolder.TimeZoneInfo).Return(_timeZoneInfo).Repeat.Any();
			Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
            _mocks.ReplayAll();
            IPersonPeriod period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateTime.AddDays(-10)));
            _person.AddPersonPeriod(period);
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            _target.LoadPeriod(helper);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyColorAndDescriptionNonConfidentialAbsence()
        {
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
            IAbsence absence = AbsenceFactory.CreateAbsence("absence", "abs", System.Drawing.Color.Blue);
            IAbsenceLayer layer = new AbsenceLayer(absence, _dateTimePeriod);
            part.CreateAndAddAbsence(layer);

            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(_stateHolder.TimeZoneInfo).Return(_timeZoneInfo).Repeat.Any();
			Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
            _mocks.ReplayAll();
            IPersonPeriod period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateTime.AddDays(-10)));
            _person.AddPersonPeriod(period);
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            _target.LoadPeriod(helper);
            _mocks.VerifyAll();

            Assert.AreEqual(System.Drawing.Color.Blue, _target.CellDataCollection.First().Value.DisplayColor);
            Assert.AreEqual("absence", _target.CellDataCollection.First().Value.DisplayName);
            Assert.AreEqual("abs", _target.CellDataCollection.First().Value.DisplayShortName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyColorAndDescriptionConfidentialAbsence()
        {
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
            IAbsence absence = AbsenceFactory.CreateAbsence("absence", "abs", System.Drawing.Color.Blue);
            absence.Confidential = true;
            IAbsenceLayer layer = new AbsenceLayer(absence, _dateTimePeriod);
            part.CreateAndAddAbsence(layer);
            
            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_scheduleDictionary).Repeat.AtLeastOnce();
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(_stateHolder.TimeZoneInfo).Return(_timeZoneInfo).Repeat.Any();
			Expect.Call(() => _preferenceNightRestChecker.CheckNightlyRest(null)).IgnoreArguments();
            _mocks.ReplayAll();
            IPersonPeriod period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(_dateTime.AddDays(-10)));
            _person.SetId(Guid.NewGuid());
            _person.AddPersonPeriod(period);
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            using(new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                _target.LoadPeriod(helper);
            }
            _mocks.VerifyAll();

            var cellContents = _target.CellDataCollection.First().Value;
            Assert.AreEqual(ConfidentialPayloadValues.DisplayColor, cellContents.DisplayColor);
            Assert.AreEqual(ConfidentialPayloadValues.Description.Name, cellContents.DisplayName);
            Assert.AreEqual(ConfidentialPayloadValues.Description.ShortName, cellContents.DisplayShortName);
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mainshift"), Test]
        public void VerifyGetTotalMainshiftRestriction()
        {
            IMainShift mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            IPersonAssignment assignment = new PersonAssignment(_person, _scenario);
            assignment.SetMainShift(mainShift);
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
            part.Add(assignment);
            
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
            _mocks.ReplayAll();
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            var extractor = new RestrictionExtractor(_resultStateHolder);
                extractor.Extract(helper.Person, helper.Period.Value.StartDate);
            IEffectiveRestriction totalRestriction = extractor.CombinedRestriction(helper.SchedulingOptions);

            totalRestriction = RestrictionSummaryModel.GetMainShiftTotalRestriction(part, totalRestriction);
            Assert.IsNotNull(totalRestriction);
            totalRestriction = RestrictionSummaryModel.GetMainShiftTotalRestriction(part, null);
            Assert.IsNotNull(totalRestriction);
            _mocks.VerifyAll();
        }
        [Test]
        public void VerifyGetTotalMainShiftRestrictionWhenNightshift()
        {
            Activity telephone = ActivityFactory.CreateActivity("Tel");
            var period1 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 21, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 5, 0, 0, DateTimeKind.Utc));

            var layer1 = new MainShiftActivityLayer(telephone, period1);

            MainShift mainShiftShift = MainShiftFactory.CreateMainShift(ShiftCategoryFactory.CreateShiftCategory("TEL"));
            mainShiftShift.LayerCollection.Add(layer1);

            IPersonAssignment assignment = new PersonAssignment(_person, _scenario);
            assignment.SetMainShift(mainShiftShift);
            _person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            var part = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _person, new DateOnly(_dateTime));
            part.Add(assignment);

            var expectedStartTimeLimitation = new StartTimeLimitation(new TimeSpan(22, 0, 0), new TimeSpan(22, 0, 0));
            var expectedEndTimeLimitation = new EndTimeLimitation(new TimeSpan(1, 6, 0, 0), new TimeSpan(1, 6, 0, 0));
            Expect.Call(_resultStateHolder.Schedules).Return(_scheduleDictionary);
            Expect.Call(_range.ScheduledDay(new DateOnly(_dateTime))).IgnoreArguments().Return(part).Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            var helper = new AgentInfoHelper(_person, new DateOnly(_dateTime), _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            var extractor = new RestrictionExtractor(_resultStateHolder);
            extractor.Extract(helper.Person, helper.Period.Value.StartDate);
            IEffectiveRestriction totalRestriction = extractor.CombinedRestriction(helper.SchedulingOptions);
            totalRestriction = RestrictionSummaryModel.GetMainShiftTotalRestriction(part, totalRestriction);
            Assert.IsNotNull(totalRestriction);
            Assert.AreEqual(expectedStartTimeLimitation.StartTime, totalRestriction.StartTimeLimitation.StartTime);
            Assert.AreEqual(expectedStartTimeLimitation.EndTime, totalRestriction.StartTimeLimitation.EndTime);
            Assert.AreEqual(expectedEndTimeLimitation.StartTime, totalRestriction.EndTimeLimitation.StartTime);
            Assert.AreEqual(expectedEndTimeLimitation.EndTime, totalRestriction.EndTimeLimitation.EndTime);
            totalRestriction = RestrictionSummaryModel.GetMainShiftTotalRestriction(part, null);
            Assert.IsNotNull(totalRestriction);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyGetTotalAvailabilityRestriction()
        {
            IAvailabilityRestriction restriction = new AvailabilityRestriction();
            var extractor = _mocks.StrictMock<IRestrictionExtractor>();
            restriction.NotAvailable = true;
            IList<IAvailabilityRestriction> availabilityRestrictions = new List<IAvailabilityRestriction>{restriction};

            Expect.Call(extractor.AvailabilityList).Return(availabilityRestrictions).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(null, null),
                                                                              new EndTimeLimitation(null, null),
                                                                              new WorkTimeLimitation(null, null), null,
                                                                              null, null, new List<IActivityRestriction>());
            totalRestriction = RestrictionSummaryModel.GetTotalAvailabilityRestriction(extractor, totalRestriction);
            Assert.IsNotNull(totalRestriction);
            Assert.IsTrue(totalRestriction.NotAvailable);
            totalRestriction = RestrictionSummaryModel.GetTotalAvailabilityRestriction(extractor, null);
            Assert.IsNotNull(totalRestriction);
            Assert.IsTrue(totalRestriction.NotAvailable);
            _mocks.VerifyAll();
        }
        
        [Test]
        public void VerifyGetTotalRotationRestriction()
        {
            IRotationRestriction restriction = new RotationRestriction();
            var extractor = _mocks.StrictMock<IRestrictionExtractor>();
            restriction.EndTimeLimitation = new EndTimeLimitation(new TimeSpan(17,0,0), null);
            IList<IRotationRestriction> rotationRestrictions = new List<IRotationRestriction>{restriction};

            Expect.Call(extractor.RotationList).Return(rotationRestrictions).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(null, null),
                                                                              new EndTimeLimitation(null, null),
                                                                              new WorkTimeLimitation(null, null), null,
                                                                              null, null, new List<IActivityRestriction>());
            totalRestriction = RestrictionSummaryModel.GetTotalRotationRestriction(extractor, totalRestriction);
            Assert.IsNotNull(totalRestriction);
            Assert.IsTrue(totalRestriction.EndTimeLimitation.HasValue());
            Assert.AreEqual(new TimeSpan(17, 0, 0), totalRestriction.EndTimeLimitation.StartTime);
            totalRestriction = RestrictionSummaryModel.GetTotalRotationRestriction(extractor, null);
            Assert.IsTrue(totalRestriction.EndTimeLimitation.HasValue());
            Assert.AreEqual(new TimeSpan(17, 0, 0), totalRestriction.EndTimeLimitation.StartTime);
            Assert.IsNotNull(totalRestriction);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyGetTotalPreferenceRestriction()
        {
            IPreferenceCellData cellData = new PreferenceCellData();
            IPreferenceRestriction restriction = new PreferenceRestriction();
            restriction.AddActivityRestriction(new ActivityRestriction(ActivityFactory.CreateActivity("Lunch")));
            var extractor = _mocks.StrictMock<IRestrictionExtractor>();
            restriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(9, 0, 0), null);
            IList<IPreferenceRestriction> preferenceRestrictions = new List<IPreferenceRestriction> { restriction };

            Expect.Call(extractor.PreferenceList).Return(preferenceRestrictions).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(new TimeSpan(8,0,0), new TimeSpan(10, 0, 0)),
                                                                              new EndTimeLimitation(null, null),
                                                                              new WorkTimeLimitation(null, null), null,
                                                                              null, null, new List<IActivityRestriction>());
            totalRestriction.IsPreferenceDay = true;
            totalRestriction = RestrictionSummaryModel.GetTotalPreferenceRestriction(extractor, totalRestriction, cellData );
            Assert.IsNotNull(totalRestriction);
            Assert.IsTrue(totalRestriction.StartTimeLimitation.HasValue());
            Assert.AreEqual(new TimeSpan(9, 0, 0), totalRestriction.StartTimeLimitation.StartTime);
            totalRestriction = RestrictionSummaryModel.GetTotalPreferenceRestriction(extractor, null, cellData);
            Assert.IsTrue(totalRestriction.StartTimeLimitation.HasValue());
            Assert.AreEqual(new TimeSpan(9, 0, 0), totalRestriction.StartTimeLimitation.StartTime);
            Assert.IsNotNull(totalRestriction);
            Assert.IsTrue(totalRestriction.ActivityRestrictionCollection.Count == 1);
            Assert.IsTrue(cellData.HasActivityPreference);
            _mocks.VerifyAll();
        }

       

        [Test]
        public void VerifyGetTotalStudentAvailabilityRestriction()
        {
            IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
            var extractor = _mocks.StrictMock<IRestrictionExtractor>();
            restriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(9, 0, 0), null);
            IList<IStudentAvailabilityRestriction> studentAvailabilityRestrictions = new List<IStudentAvailabilityRestriction> { restriction };
            IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(_person, new DateOnly(_dateTime),
                                                                                        studentAvailabilityRestrictions);
            IList<IStudentAvailabilityDay> studentAvailabilityDays = new List<IStudentAvailabilityDay>
                                                                         {studentAvailabilityDay};

            Expect.Call(extractor.StudentAvailabilityList).Return(studentAvailabilityDays).Repeat.AtLeastOnce();
            _mocks.ReplayAll();

            IEffectiveRestriction totalRestriction = new EffectiveRestriction(new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
                                                                              new EndTimeLimitation(null, null),
                                                                              new WorkTimeLimitation(null, null), null,
                                                                              null, null, new List<IActivityRestriction>());
            totalRestriction = RestrictionSummaryModel.GetTotalStudentRestriction(extractor, totalRestriction);
            Assert.IsNotNull(totalRestriction);
            Assert.IsTrue(totalRestriction.StartTimeLimitation.HasValue());
            Assert.AreEqual(new TimeSpan(9, 0, 0), totalRestriction.StartTimeLimitation.StartTime);
            totalRestriction = RestrictionSummaryModel.GetTotalStudentRestriction(extractor, null);
            Assert.IsTrue(totalRestriction.StartTimeLimitation.HasValue());
            Assert.AreEqual(new TimeSpan(9, 0, 0), totalRestriction.StartTimeLimitation.StartTime);
            Assert.IsNotNull(totalRestriction);
            _mocks.VerifyAll();
        }

        [Test]
        public void CanGetCurrentTimePeriod()
        {
            IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Night");
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DO"));
            IPreferenceCellData cellData = new PreferenceCellData();
            IPreferenceCellData cellData2 = new PreferenceCellData();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                  new EndTimeLimitation(),
                                                                                  new WorkTimeLimitation(
                                                                                      new TimeSpan(7, 0, 0),
                                                                                      new TimeSpan(14, 0, 0)), shiftCategory,
                                                                                      dayOffTemplate, null, new List<IActivityRestriction>());

            cellData.EffectiveRestriction = effectiveRestriction;
            Assert.IsNotNull(_target.CurrentPeriodTime());
            cellData2.IsInsidePeriod = true;
            cellData2.EffectiveRestriction = effectiveRestriction;
            _target.CellDataCollection.Add(1, cellData);
            _target.CellDataCollection.Add(2, cellData2);

            Assert.IsNotNull(_target.CurrentPeriodTime());
        }

        [Test]
        public void CanGetPeriodTargetTime()
        {
            IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Night");
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DO"));
            IPreferenceCellData cellData = new PreferenceCellData();
            var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                  new EndTimeLimitation(),
                                                                                  new WorkTimeLimitation(
                                                                                      new TimeSpan(7, 0, 0),
                                                                                      new TimeSpan(14, 0, 0)),
                                                                                      shiftCategory, dayOffTemplate, null,
                                                                                      new List<IActivityRestriction>());

            cellData.EffectiveRestriction = effectiveRestriction;
            _target.CellDataCollection.Add(1, cellData);

            Assert.AreEqual(TimeSpan.Zero, _target.PeriodTargetTime());
            _target.CellDataCollection.Clear();
            Assert.AreEqual(TimeSpan.Zero, _target.PeriodTargetTime());
        }
        [Test]
        public void VerifyPeriodIsValid()
        {
            IPreferenceCellData cellData = new PreferenceCellData {Enabled = true};
            _target.CellDataCollection.Add(1, cellData);

            Assert.IsFalse(_target.PeriodIsValid());
            cellData.Enabled = false;
            _target.CellDataCollection.Add(2, cellData);
            Assert.IsTrue(_target.PeriodIsValid());
        }
        [Test]
        public void CanGetCultureAndUICulture()
        {
            Assert.IsNotNull( _target.CurrentUICultureInfo());
            Assert.IsNotNull( _target.CurrentCultureInfo());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void CanDecideIfExtendedPreference()
        {
            var startTimeLimitation = new StartTimeLimitation();
            var endTimeLimitation = new EndTimeLimitation();
            var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(9), null);
            IEffectiveRestriction restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                                         workTimeLimitation, null, null, null, new List<IActivityRestriction>());
            Assert.IsTrue(RestrictionSummaryModel.DecideIfExtendedPreference(restriction));
            startTimeLimitation = new StartTimeLimitation();
            endTimeLimitation = new EndTimeLimitation();
            workTimeLimitation = new WorkTimeLimitation();
            restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                                         workTimeLimitation, null, null, null, new List<IActivityRestriction>());
            Assert.IsFalse(RestrictionSummaryModel.DecideIfExtendedPreference(restriction));
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void CanDecideIfExtendedPreferenceIfOnlyActivityPreference()
        {
            var startTimeLimitation = new StartTimeLimitation();
            var endTimeLimitation = new EndTimeLimitation();
            var workTimeLimitation = new WorkTimeLimitation();
            IEffectiveRestriction restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                                         workTimeLimitation, null, null, null, new List<IActivityRestriction>());
            restriction.ActivityRestrictionCollection.Add(new ActivityRestriction(ActivityFactory.CreateActivity("Lunch")));
            Assert.IsTrue(RestrictionSummaryModel.DecideIfExtendedPreference(restriction));

            startTimeLimitation = new StartTimeLimitation();
            endTimeLimitation = new EndTimeLimitation();
            workTimeLimitation = new WorkTimeLimitation();
            restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                                         workTimeLimitation, null, null, null, new List<IActivityRestriction>());
            Assert.IsFalse(RestrictionSummaryModel.DecideIfExtendedPreference(restriction));
        }
        [Test]
        public void VerifyCustomizedDayCollection()
        {
            var periodStartDate = new DateOnly(2010, 3, 10);
            ISchedulePeriod period = SchedulePeriodFactory.CreateSchedulePeriod(periodStartDate);
            period.PeriodType = SchedulePeriodType.Week;
            _person.AddSchedulePeriod(period);
            var helper = new AgentInfoHelper(_person, periodStartDate, _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            IList<DateOnly> customizedList = _target.CustomizedDayCollection(helper);
            Assert.AreEqual(28, customizedList.Count);
            var min = customizedList.Min();
            var max = customizedList.Max();
            var expectedMin = new DateOnly(2010, 3, 1);
            var expectedMax = new DateOnly(2010, 3, 28);
            Assert.AreEqual(expectedMax, max);
            Assert.AreEqual(expectedMin, min);
        }

        [Test]
        public void VerifyCustomizedDayCollectionWithGmt()
        {
            var timeZoneBefore = TeleoptiPrincipal.Current.Regional.TimeZone;
            var cultureBefore = TeleoptiPrincipal.Current.Regional.Culture;

            var regional = (Regional)TeleoptiPrincipal.Current.Regional;
            var timeZoneInfoLoggedOnPerson = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
            regional.TimeZone = timeZoneInfoLoggedOnPerson;
            var cultureInfo = new CultureInfo(1033);
            regional.Culture = cultureInfo;

			_target = new RestrictionSummaryModel(_schedulingResultStateHolder, new RuleSetProjectionService(new ShiftCreatorService()), _stateHolder, _preferenceNightRestChecker);

            var timeZoneInfoAgent = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _person.PermissionInformation.SetDefaultTimeZone(timeZoneInfoAgent);
            var periodStartDate = new DateOnly(2009, 2, 2);
            ISchedulePeriod period = SchedulePeriodFactory.CreateSchedulePeriod(periodStartDate);
            period.Number = 4;
            period.PeriodType = SchedulePeriodType.Week;
            _person.AddSchedulePeriod(period);
            var helper = new AgentInfoHelper(_person, periodStartDate, _schedulingResultStateHolder, _options, _ruleSetProjectionService);
            helper.SchedulePeriodData();
            IList<DateOnly> customizedList = _target.CustomizedDayCollection(helper);
            Assert.AreEqual(49, customizedList.Count);
            var min = customizedList.Min();
            var max = customizedList.Max();
            var expectedMin = new DateOnly(2009, 1, 25);
            var expectedMax = new DateOnly(2009, 3, 14);
            Assert.AreEqual(expectedMax, max);
            Assert.AreEqual(expectedMin, min);

            regional.TimeZone = timeZoneBefore;
            regional.Culture = cultureBefore;
        }


        [Test]
        public void VerifySchedulerLoadedPeriod()
        {
            _target.SchedulerLoadedPeriod = _dateTimePeriod;
            Assert.AreEqual(_dateTimePeriod, _target.SchedulerLoadedPeriod);
        }
    }
}
