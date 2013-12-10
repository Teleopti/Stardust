using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    /// <summary>
    /// Tests for ViewBaseHelper
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture, SetCulture("sv-SE"), SetUICulture("en-US")]
    public class ViewBaseHelperTest
    {
        private IPerson _agent;
        private IScenario _scenario;
        private Rectangle _destRect;
        private Rectangle _expectedRect;
        private int _minutes;
        private int _hourWidth;
        private MockRepository _mockRep;

        private ScheduleParameters _param;
        private ScheduleRange _scheduleRange;
        private IPersonAssignment _ass1;
        private IPersonAssignment _ass2;
        private IPersonAssignment _ass3;
        private IPersonAssignment _ass4;

        private IPersonAbsence _abs1;
        private PersonDayOff _dayOff1;
        private IMeeting _meeting1;
        private IMeeting _meeting2;
        private IAbsence _absence;
        private TimeSpan _nightlyRest;
        private IContract _contract;

        private DateTimePeriod _periodBounds;

        private DateTimePeriod _periodEndsToday = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc),
                                                                        new DateTime(2000, 1, 2, 4, 0, 0, DateTimeKind.Utc));
        private readonly DateTimePeriod _periodBeginsToday = new DateTimePeriod(new DateTime(2000, 1, 2, 20, 0, 0, DateTimeKind.Utc),
                                                                    new DateTime(2000, 1, 3, 2, 0, 0, DateTimeKind.Utc));
        private readonly DateTimePeriod _periodBeginsAndEndsToday = new DateTimePeriod(new DateTime(2000, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                                                                        new DateTime(2000, 1, 2, 15, 0, 0, DateTimeKind.Utc));
        private readonly DateTimePeriod _periodWholeDay = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc),
                                                                new DateTime(2000, 1, 3, 2, 0, 0, DateTimeKind.Utc));
        private IScheduleDictionary _dic;

        private IScheduleDay _schedulePart1;
        private IScheduleDay _schedulePart2;
        private IScheduleDay _schedulePart3;
        private IProjectionService _projectionService;
        private IPersonDayOff _personDayOff;
        private ReadOnlyCollection<IPersonDayOff> _personDayOffCollection;
        private IPersonAssignment _personAssignment;
        private IPersonAbsence _personAbsence;
        private ReadOnlyCollection<IPersonAbsence> _personAbsenceCollection;
        private IList<IVisualLayer> _visualLayers;
        private IVisualLayer _visualLayer;
        private IVisualLayerCollection _visualLayerCollection;
        private VisualLayerFactory _layerFactory;
        private DateOnly _baseDateTime;
        private IDictionary<IPerson, IScheduleRange> _underlyingDictionary;
			
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
        public void Setup()
        {
            _layerFactory = new VisualLayerFactory();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            IPerson person = PersonFactory.CreatePerson();
            _agent = PersonFactory.CreatePersonWithPersonPeriod(person, new DateOnly(1999,1,1), new List<ISkill>());
            _mockRep = new MockRepository();
            _schedulePart1 = _mockRep.StrictMock<IScheduleDay>();
            _schedulePart2 = _mockRep.StrictMock<IScheduleDay>();
            _schedulePart3 = _mockRep.StrictMock<IScheduleDay>();
            _projectionService = _mockRep.StrictMock<IProjectionService>();
            _personDayOff = PersonDayOffFactory.CreatePersonDayOff(_agent, _scenario, new DateTime(2008, 11, 24, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(24), TimeSpan.Zero, TimeSpan.FromHours(12));
            _personDayOffCollection = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { _personDayOff });
            _personAssignment = CreatePersonAssignment();
            _personAbsence = CreatePersonAbsence();
            _personAbsenceCollection = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence> { _personAbsence });
            _visualLayers = new List<IVisualLayer>();
            IVisualLayer actLayer = _layerFactory.CreateShiftSetupLayer(ActivityFactory.CreateActivity("activity"), _personAbsence.Period, _agent);
            _visualLayer = _layerFactory.CreateAbsenceSetupLayer(_personAbsence.Layer.Payload, actLayer, _personAbsence.Period);
            _visualLayerCollection = new VisualLayerCollection(_agent, _visualLayers, new ProjectionPayloadMerger());

            _underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
            _dic = new ScheduleDictionaryForTest(_scenario,
                                                new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2010, 1, 1)),
                                                _underlyingDictionary);
             _param = new ScheduleParameters(_scenario, _agent,
                                           new DateTimePeriod(2000, 1, 1, 2010, 1, 1));
             _scheduleRange = new ScheduleRange(_dic, _param);
        	var act = ActivityFactory.CreateActivity("sdfsdf");
        	act.InWorkTime = true;
				_ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
                              act,
                              _agent,
                              TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 0, 0, 0), new DateTime(2001, 1, 1, 1, 0, 0)),
                              ShiftCategoryFactory.CreateShiftCategory("Morgon"),
                              _scenario);

             _ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
                               ActivityFactory.CreateActivity("sdfsdf"),
                               _agent,
                              TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 2, 0, 0), new DateTime(2001, 1, 1, 3, 0, 0)),
                               ShiftCategoryFactory.CreateShiftCategory("Morgon"),
                               _scenario);

             _ass3 = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
                                ActivityFactory.CreateActivity("sdfsdf"),
                                _agent,
                              TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 2, 15, 0), new DateTime(2001, 1, 1, 2, 30, 0)),
                                ShiftCategoryFactory.CreateShiftCategory("Morgon"),
                                _scenario);

             _ass4 = PersonAssignmentFactory.CreateAssignmentWithMainShift(
                                 ActivityFactory.CreateActivity("sdfsdf"),
                                 _agent,
                              TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 5, 0, 0), new DateTime(2001, 1, 2, 6, 0, 0)),
                                 ShiftCategoryFactory.CreateShiftCategory("Morgon"),
                                 _scenario);
            

            //create absences
            _absence = AbsenceFactory.CreateAbsence("Tjänsteresa", "TJ", Color.DimGray);
            

            _abs1 = new PersonAbsence(_agent, _scenario,
                                     new AbsenceLayer(_absence, new DateTimePeriod(2001, 1, 1, 2006, 1, 1)));

            //create day offs
            var dayOff = new DayOffTemplate(new Description("test")) {Anchor = TimeSpan.Zero};
            dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(1));
            _dayOff1 = new PersonDayOff(_agent, _scenario, dayOff, new DateOnly(2001, 1, 1));

            _meeting1 = new Meeting(_agent, new List<IMeetingPerson>(), "meeting1", "location1", "description1", ActivityFactory.CreateActivity("activity1"), _scenario)
                            {
                                StartDate = new DateOnly(2006, 1, 1),
                                EndDate = new DateOnly(2006, 1, 1),
                                StartTime = TimeSpan.FromHours(8),
                                EndTime = TimeSpan.FromHours(9)
                            };
            _meeting2 = new Meeting(_agent, new List<IMeetingPerson>(), "meeting2", "location2", "description2", ActivityFactory.CreateActivity("activity2"), _scenario)
                            {
                                StartDate = new DateOnly(2006, 1, 1),
                                EndDate = new DateOnly(2006, 1, 1),
                                StartTime = TimeSpan.FromHours(18),
                                EndTime = TimeSpan.FromHours(19)
                            };

            IMeetingPerson meetingPersonRequired = new MeetingPerson(_agent, false);
            IMeetingPerson meetingPersonOptional = new MeetingPerson(_agent, true);

            _meeting1.AddMeetingPerson(meetingPersonRequired);
            _meeting2.AddMeetingPerson(meetingPersonOptional);


            //add to schedule
            _scheduleRange.Add(_ass1);
            _scheduleRange.Add(_abs1);
            _scheduleRange.Add(_dayOff1);
            _scheduleRange.AddRange(_meeting1.GetPersonMeetings(_agent));
            _scheduleRange.AddRange(_meeting2.GetPersonMeetings(_agent));

            var start = new DateTime(2007, 10, 1, 6, 0, 0);
            var end = new DateTime(2007, 10, 1, 10, 0, 0);
            _periodBounds = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start,end);

            _nightlyRest = new TimeSpan(8, 0, 0);

            _contract = ContractFactory.CreateContract("for test");
            _contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(40, 0, 0),
                                                               _nightlyRest,
                                                               new TimeSpan(50, 0, 0));
            _contract.MinTimeSchedulePeriod = new TimeSpan(1);
        }

        [Test]
        public void VerifyToolTipDayOff()
        {
			StringAssert.Contains(_scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1)).PersonDayOffCollection()[0].DayOff.Description.Name,
                ViewBaseHelper.GetToolTipDayOff(_scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1))));
        }

        [Test]
        public void VerifyToolTipAssignments()
        {
            _param = new ScheduleParameters(_scenario, _agent,
                                           new DateTimePeriod(2000, 1, 1, 2001, 1, 5));
            _scheduleRange = new ScheduleRange(_dic, _param);

            _scheduleRange.Add(_ass1);
            _scheduleRange.Add(_ass4);
            
            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_scheduleRange.Person, _scheduleRange);

            StringAssert.Contains("Morgon", ViewBaseHelper.GetToolTipAssignments(_scheduleRange.ScheduledDay(new DateOnly(2001,1,1))));
        }

        [Test]
        public void VerifyToolTipConflictingAssignments()
        {
            _scheduleRange = new ScheduleRange(_dic, _param);

            _scheduleRange.Add(_ass1);
            _scheduleRange.Add(_ass2);
            _scheduleRange.Add(_ass3);

            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_scheduleRange.Person, _scheduleRange);

            StringAssert.Contains("Morgon", ViewBaseHelper.GetToolTipConflictingAssignments(_scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1))));
        }

        [Test]
        public void VerifyToolTipAbsences()
        {
           
            IPersonAbsence abs3 = new PersonAbsence(_agent, _scenario, new AbsenceLayer(_absence, new DateTimePeriod(2006, 1, 1, 2006, 1, 3)));

            //rk ändrat här. ska det visas agentens tid eller betraktarens tid?
            string absencePeriod2 = abs3.Layer.Period.StartDateTime.ToShortTimeString() +
                                    " - " + abs3.Layer.Period.EndDateTime.ToShortTimeString();

            _scheduleRange.Add(abs3);

            string expected = "Tjänsteresa" + ": " + absencePeriod2;
           
            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_scheduleRange.Person, _scheduleRange);

            Assert.AreEqual(expected, ViewBaseHelper.GetToolTipAbsences(_scheduleRange.ScheduledDay(new DateOnly(2006, 1, 1))));
            
        }

        [Test]
        public void VerifyToolTipLongAbsences()
        {
            var startTime = new DateTime(2006, 1, 2, 8 ,0, 0, DateTimeKind.Utc);
            var endTime = new DateTime(2006, 1, 5, 17, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(startTime, endTime);
            var partStartperiod = new DateTimePeriod(startTime.Date, startTime.Date.AddDays(1));
            var partMiddlePeriod = new DateTimePeriod(startTime.Date.AddDays(2), startTime.Date.AddDays(3));
            var partEndPeriod = new DateTimePeriod(endTime.Date, endTime.Date.AddDays(1));

            var layer = new AbsenceLayer(_absence, period);
            IPersonAbsence personAbsence = new PersonAbsence(_agent, _scenario, layer);
            var absCollection = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence> { personAbsence });
            var part = _mockRep.StrictMock<IScheduleDay>();
            string expectedStart = "Tjänsteresa: " + TimeZoneHelper.ConvertFromUtc(startTime).ToShortTimeString() + 
                                            " - " + partStartperiod.EndDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone).ToShortTimeString();

            string expectedMiddle = "Tjänsteresa: " + partStartperiod.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone).ToShortTimeString() + 
                                            " - " + partStartperiod.EndDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone).ToShortTimeString();

            string expectedEnd = "Tjänsteresa: " + partStartperiod.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone).ToShortTimeString() + 
                                            " - " + TimeZoneHelper.ConvertFromUtc(endTime).ToShortTimeString();

            using (_mockRep.Record())
            {
                Expect.Call(part.PersonAbsenceCollection()).Return(absCollection).Repeat.AtLeastOnce();
                Expect.Call(part.TimeZone).Return(TimeZoneHelper.CurrentSessionTimeZone).Repeat.AtLeastOnce();
                Expect.Call(part.Period).Return(partStartperiod);
                Expect.Call(part.Period).Return(partMiddlePeriod);
                Expect.Call(part.Period).Return(partEndPeriod);
				Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(partStartperiod.StartDateTime), TeleoptiPrincipal.Current.Regional.TimeZone));
				Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(partMiddlePeriod.StartDateTime), TeleoptiPrincipal.Current.Regional.TimeZone));
				Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(partEndPeriod.StartDateTime), TeleoptiPrincipal.Current.Regional.TimeZone));
            }

            using (_mockRep.Playback())
            {
                Assert.AreEqual(expectedStart, ViewBaseHelper.GetToolTipAbsences(part));  
                Assert.AreEqual(expectedMiddle, ViewBaseHelper.GetToolTipAbsences(part));
                Assert.AreEqual(expectedEnd, ViewBaseHelper.GetToolTipAbsences(part));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyConfidentialAbsence()
        {
            IAbsence confidentialAbsence = AbsenceFactory.CreateAbsence("Confidential", "CF", Color.DimGray);
            confidentialAbsence.Confidential = true;

            IPersonAbsence confidentialPersonAbsence = new PersonAbsence(_agent, _scenario,
                                    new AbsenceLayer(confidentialAbsence, new DateTimePeriod(2006, 1, 2, 2006, 1, 3)));

            _agent.SetId(Guid.NewGuid());
            _param = new ScheduleParameters(_scenario, _agent,
                               new DateTimePeriod(2006, 1, 1, 2006, 1, 10));

            _scheduleRange = new ScheduleRange(_dic, _param);

            _scheduleRange.Add(confidentialPersonAbsence);

            //rk ändrat här. ska det visas agentens tid eller betraktarens tid?
            string absencePeriod = confidentialPersonAbsence.Layer.Period.StartDateTime.ToShortTimeString() +
                                    " - " + confidentialPersonAbsence.Layer.Period.EndDateTime.ToShortTimeString();


            string expected = ConfidentialPayloadValues.Description.Name + ": " + absencePeriod;

            var authorization = _mockRep.StrictMock<IPrincipalAuthorization>();

            using(_mockRep.Record())
            {
                Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
                                                      DateOnly.Today, _agent)).Return(false);
                Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)).
                    Return(true);
            }
            using (_mockRep.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    Assert.AreEqual(expected,
                                    ViewBaseHelper.GetToolTip(_scheduleRange.ScheduledDay(new DateOnly(2006, 1, 2))));
                }
            }
        }

        [Test]
        public void VerifyToolTipMeetings()
        {
            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_scheduleRange.Person, _scheduleRange);

            var res = ViewBaseHelper.GetToolTipMeetings(_scheduleRange.ScheduledDay(new DateOnly(2006, 1, 1)));
            
            StringAssert.Contains("meeting1", res);
            StringAssert.Contains("meeting2", res);
        }
        [Test]
        public void VerifyToolTipOvertime()
        {
            var period = new DateTimePeriod(new DateTime(2008, 1, 1, 17, 0, 0, DateTimeKind.Utc), new DateTime(2008, 1, 1, 18, 0, 0, DateTimeKind.Utc));
            IList<IPersonPeriod> personPeriods = _agent.PersonPeriods(period.ToDateOnlyPeriod(_agent.PermissionInformation.DefaultTimeZone()));
            IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
                MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Paid Overtime",
                                                                                   MultiplicatorType.Overtime);
            personPeriods[0].PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);

            var personAssignment = new PersonAssignment(_agent, _scheduleRange.Scenario);
            IActivity activity = ActivityFactory.CreateActivity("Overtime activity");
            var overtimeShift = new OvertimeShift();
            IOvertimeShiftActivityLayer layer =
                new OvertimeShiftActivityLayer(activity, period,
                                               multiplicatorDefinitionSet);
            personAssignment.AddOvertimeShift(overtimeShift);
            overtimeShift.LayerCollection.Add(layer);

            var part = _scheduleRange.ScheduledDay(new DateOnly(2008, 1, 1));
            part.Add(personAssignment);

            //rk ändrat här. ska det visas agentens tid eller betraktarens tid?
            string expected = string.Concat(multiplicatorDefinitionSet.Name, ": ", activity.Name, ": ", "17:00 - 18:00");

            Assert.AreEqual(expected, ViewBaseHelper.GetToolTipOvertime(part));
        }

        [Test]
        public void VerifyGetToolTip()
        {
            _scheduleRange.Add(_ass2);

            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_scheduleRange.Person,_scheduleRange);

            StringAssert.Contains("Morgon", ViewBaseHelper.GetToolTip(_scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1))));
        }

        [Test]
        public void VerifyEmptyToolTip()
        {
            _param = new ScheduleParameters(_scenario, _agent,
                                           new DateTimePeriod(2000, 12, 31, 2001, 1, 1));

            _scheduleRange = new ScheduleRange(_dic, _param);

            string expected = string.Empty;

            Assert.AreEqual(expected, ViewBaseHelper.GetToolTip(_scheduleRange.ScheduledDay(new DateOnly(2000, 1, 1))));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyRuleConflictToolTip()
        {
            var schedulePeriod = new DateTimePeriod(2000, 11, 1, 2001, 5, 1);
            _param = new ScheduleParameters(_scenario, _agent,
                                           schedulePeriod);

            _scheduleRange = new ScheduleRange(_dic, _param);
            _underlyingDictionary[_agent] = _scheduleRange;
            ITeam team = TeamFactory.CreateSimpleTeam();

            _agent.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
                                       new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
                                       team));


            IPersonAssignment noRest = CreateNoRestAss();

            _scheduleRange.Add(noRest);
            _scheduleRange.Add(_dayOff1);
            _scheduleRange.Add(_ass1);
            
            var newRules = NewBusinessRuleCollection.Minimum();
			newRules.Add(new NewNightlyRestRule(new WorkTimeStartEndExtractor()));
            _dic.ValidateBusinessRulesOnPersons(new List<IPerson>{_agent} ,CultureInfo.CurrentUICulture, newRules);

            var parts = _scheduleRange.ScheduledDay(new DateOnly(2001,1,1));

            string tt = ViewBaseHelper.GetToolTip(parts);
            int pos = tt.IndexOf("There must be a daily rest of at least",StringComparison.OrdinalIgnoreCase );
            Assert.AreNotEqual(-1, pos);
        }


        [Test]
        public void VerifyMinWidth()
        {
            _minutes = 1;
            _hourWidth = 60;
            Assert.AreEqual(1,ViewBaseHelper.GetMinWidth(_minutes,_hourWidth));

            _minutes = 30;
            _hourWidth = 60;
            Assert.AreEqual(30, ViewBaseHelper.GetMinWidth(_minutes, _hourWidth));


            _minutes = 40;
            _hourWidth = 50;
            Assert.AreEqual(33, ViewBaseHelper.GetMinWidth(_minutes, _hourWidth));
            
        }

        [Test]
        public void VerifyGetLayerRectangle()
        {
            _destRect = new Rectangle(0, 0, 400, 40);
            _expectedRect = new Rectangle(0, 2, 400, 36);

            Rectangle layerRect = ViewBaseHelper.GetLayerRectangle(_periodBounds,_destRect,_periodBounds, false);

            Assert.AreEqual(_expectedRect, layerRect);     
        }

        [Test]
        public void VerifyGetLayerRectangleWhenNoIntersection()
        {
            var start = new DateTime(2007, 10, 1, 23, 0, 0);
            var end = new DateTime(2007, 10, 2, 3, 0, 0);
            var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(start), TimeZoneHelper.ConvertToUtc(end));

            _destRect = new Rectangle(0, 0, 400, 40);
            
            Rectangle layerRect = ViewBaseHelper.GetLayerRectangle(_periodBounds, _destRect, period, false);

            Assert.IsTrue(layerRect.IsEmpty);
        }

        [Test]
        public void VerifyGetLayerRectangleRightToLeft()
        {
            var start = new DateTime(2007, 10, 1, 7, 0, 0);
            var end = new DateTime(2007, 10, 1, 10, 0, 0);
            var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(start), TimeZoneHelper.ConvertToUtc(end));

            _destRect = new Rectangle(0, 0, 400, 40);
            _expectedRect = new Rectangle(0, 2, 300, 36);

            Rectangle layerRect = ViewBaseHelper.GetLayerRectangle(_periodBounds, _destRect, period, true);

            Assert.AreEqual(_expectedRect, layerRect);
        }

        [Test]
        public void CanReturnDictionaryWithFirstDatesOfEverySelectedWeek()
        {
            var startDate = new DateOnly(2007, 12, 02);
            var endDate = new DateOnly(2008, 01, 01);
            var period = new DateOnlyPeriod(startDate, endDate);
            Dictionary<int, DateOnly> returnDic = ViewBaseHelper.AddWeekDates(period);
            Assert.AreEqual(new DateOnly(2007, 12, 2), returnDic[48]);
            Assert.AreEqual(new DateOnly(2007, 12, 3), returnDic[49]);
            Assert.AreEqual(new DateOnly(2007, 12, 10), returnDic[50]);
            Assert.AreEqual(new DateOnly(2007, 12, 17), returnDic[51]);
            Assert.AreEqual(new DateOnly(2007, 12, 24), returnDic[52]);
        }

        [Test]
        public void CanReturnWeekHeaderString()
        {
            var startDate = new DateOnly(2007, 12, 02);
            var endDate = new DateOnly(2007, 12, 19);
            var expectedPeriod = new DateOnlyPeriod(2007, 12, 10, 2007, 12, 16);
            var expectedPeriod2 =new DateOnlyPeriod(2007, 12, 17, 2007, 12, 19);
            
            var period = new DateOnlyPeriod(startDate, endDate);
            Assert.AreEqual(expectedPeriod.StartDate, ViewBaseHelper.WeekHeaderDates(50, period).StartDate);
            Assert.AreEqual(expectedPeriod2.StartDate, ViewBaseHelper.WeekHeaderDates(51, period).StartDate);
        }

        private IPersonAssignment CreateNoRestAss()
        {
        	var act = ActivityFactory.CreateActivity("sdfsdf");
        	act.InWorkTime = true;
            IPersonAssignment noRest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
                  act,
                  _agent, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 7, 0, 0), new DateTime(2001, 1, 2, 0, 0, 0)),
                  ShiftCategoryFactory.CreateShiftCategory("Morgon"),
                  _scenario);

            return noRest;
        }

        [Test]
        public void VerifyStyleCurrentContractTimeCellSetsCellCorrectWithCachedValue()
        {
            IVisualLayerCollection visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(new Person(),
                                                            TimeSpan.FromHours(8), TimeSpan.FromHours(14),
                                                            new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(11)));
        	var totalContractTime = visualLayerCollection.ContractTime();

            var range = _mockRep.StrictMock<IScheduleRange>();
            var style = new GridStyleInfo();

            using (_mockRep.Record())
            {
                Expect.Call(range.CalculatedContractTimeHolder).Return(totalContractTime).Repeat.Twice();
            }

            using (_mockRep.Playback())
            {
                ViewBaseHelper.StyleCurrentContractTimeCell(style, range, new DateOnlyPeriod());
            }

            Assert.AreEqual(totalContractTime, style.CellValue);
        }

        [Test]
        public void ShouldReturnCurrentContractTime()
        {
            var visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(new Person(),TimeSpan.FromHours(8), TimeSpan.FromHours(14),new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(11)));
            var totalContractTime = visualLayerCollection.ContractTime();

            var range = _mockRep.StrictMock<IScheduleRange>();

            using (_mockRep.Record())
            {
                Expect.Call(range.CalculatedContractTimeHolder).Return(totalContractTime).Repeat.Twice();
            }

            using (_mockRep.Playback())
            {
                var currentContractTime = ViewBaseHelper.CurrentContractTime(range, new DateOnlyPeriod());
                Assert.AreEqual(totalContractTime, currentContractTime);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullRange()
        {
            ViewBaseHelper.CurrentContractTime(null, new DateOnlyPeriod());    
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void CurrentContractTimeCellShouldNotCountInvalidDays()
        {
            IPerson person = _mockRep.StrictMock<IPerson>();

            IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(), (TimeZoneInfo.Utc));

            TimeSpan totalContractTime = TimeSpan.Zero;

            var part = _mockRep.StrictMock<IScheduleDay>();
            var range = _mockRep.StrictMock<IScheduleRange>();
            var style = new GridStyleInfo();

            using (_mockRep.Record())
            {
                Expect.Call(range.CalculatedContractTimeHolder).Return(null);
                range.CalculatedContractTimeHolder = TimeSpan.FromHours(0);
                LastCall.IgnoreArguments();
                Expect.Call(range.CalculatedContractTimeHolder).Return(totalContractTime);
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod())).Return(new List<IScheduleDay> {part}).
                    Repeat.AtLeastOnce();
                Expect.Call(part.Person).Return(person);
                Expect.Call(part.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
                Expect.Call(person.Period(new DateOnly())).Return(null);

            }

            using (_mockRep.Playback())
            {
                ViewBaseHelper.StyleCurrentContractTimeCell(style, range, new DateOnlyPeriod());
            }

            Assert.AreEqual(totalContractTime, style.CellValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyStyleCurrentContractTimeCellSetsCellCorrectWithoutCachedValue()
        {
            IPerson person = _mockRep.StrictMock<IPerson>();
            IVisualLayerCollection visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(person, TimeSpan.FromHours(8), TimeSpan.FromHours(14),
                                                            new TimePeriod(TimeSpan.FromHours(10), TimeSpan.FromHours(11)));

            IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(), (TimeZoneInfo.Utc));

            TimeSpan totalContractTime = visualLayerCollection.ContractTime();

            var part = _mockRep.StrictMock<IScheduleDay>();
            var projectionService = _mockRep.StrictMock<IProjectionService>();
            var range = _mockRep.StrictMock<IScheduleRange>();
            var style = new GridStyleInfo();

            using (_mockRep.Record())
            {
                Expect.Call(part.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
                Expect.Call(range.CalculatedContractTimeHolder).Return(null);
                range.CalculatedContractTimeHolder = TimeSpan.FromHours(5);
                LastCall.IgnoreArguments();
                Expect.Call(range.CalculatedContractTimeHolder).Return(totalContractTime);
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod())).Return(new List<IScheduleDay> {part}).
                    Repeat.AtLeastOnce();
                Expect.Call(part.Person).Return(person);
                Expect.Call(part.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
                Expect.Call(person.Period(new DateOnly())).Return(PersonPeriodFactory.CreatePersonPeriod(new DateOnly()));
                Expect.Call(person.TerminalDate).Return(null);

            }

            using (_mockRep.Playback())
            {
                ViewBaseHelper.StyleCurrentContractTimeCell(style, range, new DateOnlyPeriod());
            }

            Assert.AreEqual(totalContractTime, style.CellValue);
        }

		[Test]
		public void ShouldReturnSchedulePeriodWhenEqualWithOpenPeriod()
		{
			var dateFrom = new DateOnly(2010, 1, 1);
			var dateTo = new DateOnly(2010, 1, 31);
			var openPeriod = new DateOnlyPeriod(dateFrom,dateTo);
			var schedulePeriod = _mockRep.StrictMock<ISchedulePeriod>();
			
			using (_mockRep.Record())
			{
                schedulePeriod.SetParent(_agent);
				Expect.Call(schedulePeriod.DateFrom).Return(dateFrom).Repeat.AtLeastOnce();
				Expect.Call(schedulePeriod.GetSchedulePeriod(dateFrom)).Return(openPeriod);
				Expect.Call(schedulePeriod.GetSchedulePeriod(dateTo)).Return(openPeriod);
			}

			using (_mockRep.Playback())
			{
                _agent.AddSchedulePeriod(schedulePeriod);
				
				Assert.IsTrue(ViewBaseHelper.CheckOpenPeriodMatchSchedulePeriod(_agent, openPeriod));
			}
		}

		[Test]
		public void ShouldSetCellValueTargetTimeToNaWhenSchedulePeriodAndOpenPeriodDoNotMatch()
		{
			using (var style = new GridStyleInfo())
			{
				var baseDateTime = new DateOnly(2001, 1, 1);
				var baseDateTimePeriod = new DateOnlyPeriod(baseDateTime, baseDateTime.AddDays(10));
				var person = _mockRep.StrictMock<IPerson>();
				var schedulePeriod = new SchedulePeriod(new DateOnly(2009, 01, 01), SchedulePeriodType.Week, 4);
				var schedulePeriods = new List<ISchedulePeriod> {schedulePeriod};
				var range = _mockRep.StrictMock<IScheduleRange>();
				var schedulingResultStateHolder = _mockRep.StrictMock<ISchedulingResultStateHolder>();

				using (_mockRep.Record())
				{
					Expect.Call(person.PersonSchedulePeriods(baseDateTimePeriod)).IgnoreArguments().Return(schedulePeriods).Repeat.
					    AtLeastOnce();
				}

				using (_mockRep.Playback())
				{
					ViewBaseHelper.StyleTargetScheduleContractTimeCell(style, person, baseDateTimePeriod,
																	   schedulingResultStateHolder, range);
				}

				Assert.AreEqual(UserTexts.Resources.NA, style.CellValue);
			}

		}

		[Test]
		public void ShouldSetCellValueTargetDayOffToNaWhenSchedulePeriodAndOpenPeriodDoNotMatch()
		{
			using (var style = new GridStyleInfo())
			{
				var baseDateTime = new DateOnly(2001, 1, 1);
				var baseDateTimePeriod = new DateOnlyPeriod(baseDateTime, baseDateTime.AddDays(10));
				var person = _mockRep.StrictMock<IPerson>();
				var schedulePeriod = new SchedulePeriod(new DateOnly(2009, 01, 01), SchedulePeriodType.Week, 4);
				var schedulePeriods = new List<ISchedulePeriod> {schedulePeriod};
				var range = _mockRep.StrictMock<IScheduleRange>();

				using (_mockRep.Record())
				{
					Expect.Call(person.PersonSchedulePeriods(baseDateTimePeriod)).IgnoreArguments().Return(schedulePeriods).Repeat.
						AtLeastOnce();
				}

				using (_mockRep.Playback())
				{
					ViewBaseHelper.StyleTargetScheduleDaysOffCell(style, person, baseDateTimePeriod, range);
				}

				Assert.AreEqual(UserTexts.Resources.NA, style.CellValue);
			}
		}
		
        [Test]
        public void VerifyStyleCurrentDaysOffsetsCellCorrect()
        { 
            IPersonDayOff persondayOff1 = PersonDayOffFactory.CreatePersonDayOff(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.Zero);
            IPersonDayOff persondayOff2 = PersonDayOffFactory.CreatePersonDayOff(new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.Zero);


            var dayOffs = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { persondayOff1, persondayOff2 });

            var range = _mockRep.StrictMock<IScheduleRange>();

            var style = new GridStyleInfo();

            using (_mockRep.Record())
            {
				Expect.Call(range.CalculatedScheduleDaysOff).Return(dayOffs.Count).Repeat.AtLeastOnce();
				Expect.Call(() => range.CalculatedScheduleDaysOff = dayOffs.Count);
            }

            using (_mockRep.Playback())
            {
                ViewBaseHelper.StyleCurrentTotalDayOffCell(style, range, new DateOnlyPeriod());
            }

            Assert.AreEqual(dayOffs.Count, style.CellValue);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullStyle()
        {
            ViewBaseHelper.StyleTargetScheduleDaysOffCell(null, null, new DateOnlyPeriod(), null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullPerson()
        {
            var style = new GridStyleInfo();

            ViewBaseHelper.StyleTargetScheduleDaysOffCell(style, null, new DateOnlyPeriod(), null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullVirtualPeriods()
        {
            ViewBaseHelper.CalculateTargetDaysOff(null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionOnNullVirtualPeriodCalculateTargetTime()
        {
            ViewBaseHelper.CalculateTargetTime(null, null, false);
        }   

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void VerifyStyleCurrentDaysOffsetsCellCorrectWhenNoCachedValue()
        {
            IPerson person = _mockRep.StrictMock<IPerson>();
            IPersonDayOff persondayOff1 = PersonDayOffFactory.CreatePersonDayOff(new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.Zero);
            IPersonDayOff persondayOff2 = PersonDayOffFactory.CreatePersonDayOff(new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc), TimeSpan.FromHours(1), TimeSpan.FromHours(1), TimeSpan.Zero);

            IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(new DateOnly(), (TimeZoneInfo.Utc));
            var dayOffs = new ReadOnlyCollection<IPersonDayOff>(new List<IPersonDayOff> { persondayOff1, persondayOff2 });

            var part = _mockRep.StrictMock<IScheduleDay>();
            var range = _mockRep.StrictMock<IScheduleRange>();
            var style = new GridStyleInfo();

            using (_mockRep.Record())
            {
                Expect.Call(range.CalculatedScheduleDaysOff).Return(null);
                Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod())).Return(new List<IScheduleDay> { part }).
                    Repeat.AtLeastOnce();
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.DayOff);
                Expect.Call(part.Person).Return(person);
                Expect.Call(part.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod);
                range.CalculatedScheduleDaysOff = 2;
                LastCall.IgnoreArguments();
            	Expect.Call(range.CalculatedScheduleDaysOff).Return(2);
                Expect.Call(person.Period(new DateOnly())).Return(PersonPeriodFactory.CreatePersonPeriod(new DateOnly()));
                Expect.Call(person.TerminalDate).Return(null);
            }

            using (_mockRep.Playback())
            {
                ViewBaseHelper.StyleCurrentTotalDayOffCell(style, range, new DateOnlyPeriod());
            }

            Assert.AreEqual(dayOffs.Count, style.CellValue);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"),
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), 
		Test]
        public void VerifyGetsTargetContractTimeFromAllSchedulePeriod()
        {
            var contractTime = TimeSpan.FromHours(1);

            var baseDateTime = new DateOnly(2001, 1, 1);
            var baseDateTimePeriod = new DateOnlyPeriod(baseDateTime, baseDateTime.AddDays(10));
            var style = new GridStyleInfo();
            var person = _mockRep.StrictMock<IPerson>();
            ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2001, 01, 01), SchedulePeriodType.Day, 11);
			schedulePeriod.SetParent(person);
            var schedulePeriods = new List<ISchedulePeriod> { schedulePeriod };
            var vPeriod = _mockRep.StrictMock<IVirtualSchedulePeriod>();
			var range = _mockRep.StrictMock<IScheduleRange>();
            IPermissionInformation permissionInformation = _mockRep.StrictMock<IPermissionInformation>();
			var schedulingResultStateHolder = _mockRep.StrictMock<ISchedulingResultStateHolder>();
        	var contract = _mockRep.StrictMock<IContract>();
        	var scheduleDictionary = _mockRep.StrictMock<IScheduleDictionary>();
        	var scheduleRange = _mockRep.StrictMock<IScheduleRange>();

            using (_mockRep.Record())
            {
                Expect.Call(person.PersonSchedulePeriods(baseDateTimePeriod)).IgnoreArguments().Return(schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(vPeriod).Repeat.AtLeastOnce();
                Expect.Call(vPeriod.IsValid).Return(true).Repeat.AtLeastOnce();
            	Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
            	Expect.Call(range.CalculatedTargetTimeHolder).Return(null).Repeat.Once();
				Expect.Call(range.CalculatedTargetTimeHolder).Return(contractTime).Repeat.AtLeastOnce();
            	Expect.Call(() => range.CalculatedTargetTimeHolder = contractTime).Repeat.AtLeastOnce();
				Expect.Call(vPeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(2009, 2, 2, 2009, 4, 1));
                Expect.Call(person.FirstDayOfWeek).Return(DayOfWeek.Monday).Repeat.Twice();
				Expect.Call(person.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(permissionInformation.Culture()).Return(CultureInfo.CurrentCulture).Repeat.AtLeastOnce();

                Expect.Call(vPeriod.Contract).Return(contract).Repeat.AtLeastOnce();
				Expect.Call(vPeriod.Person).Return(person).Repeat.AtLeastOnce();
				Expect.Call(contract.EmploymentType).Return(EmploymentType.FixedStaffNormalWorkTime);
            	
                Expect.Call(schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
            	Expect.Call(scheduleDictionary[person]).Return(scheduleRange);
            	Expect.Call(vPeriod.Extra).Return(TimeSpan.Zero);
				Expect.Call(vPeriod.BalanceOut).Return(TimeSpan.Zero);
				Expect.Call(vPeriod.BalanceIn).Return(TimeSpan.Zero);
				Expect.Call(vPeriod.PeriodTarget()).Return(contractTime);
                Expect.Call(vPeriod.Seasonality).Return(new Percent(0));
            }
            
            using (_mockRep.Playback())
            {
				ViewBaseHelper.StyleTargetScheduleContractTimeCell(style, person, baseDateTimePeriod, schedulingResultStateHolder, range);
            }

            Assert.AreEqual(contractTime,style.CellValue);
			style.Dispose();
        }

        [Test]
        public void VerifyCheckLoadedAndScheduledPeriodDayOff()
        {
            var personContract = _mockRep.StrictMock<IPersonContract>();

            _baseDateTime = new DateOnly(2009, 6, 1);
            var dateOnlyPeriod = new DateOnlyPeriod(_baseDateTime, _baseDateTime.AddDays(6));
            IPerson person = new Person();
            var info = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            person.PermissionInformation.SetDefaultTimeZone(info);
            var schedulePeriod = new SchedulePeriod(new DateOnly(2009, 6, 1), SchedulePeriodType.Week, 1);
            schedulePeriod.SetDaysOff(2);
            person.AddSchedulePeriod(schedulePeriod);

            var personPeriod = _mockRep.StrictMock<IPersonPeriod>();
            using (_mockRep.Record())
            {
                Expect.Call(personPeriod.StartDate).Return(_baseDateTime).Repeat.AtLeastOnce();
                Expect.Call(() => personPeriod.SetParent(person));
                Expect.Call(personPeriod.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personContract.Contract).Return(_contract).Repeat.AtLeastOnce();
  
            }
            using (_mockRep.Playback())
            {
                person.AddPersonPeriod(personPeriod);
                Assert.IsTrue(ViewBaseHelper.CheckOverrideDayOffAndLoadedAndScheduledPeriod(person, dateOnlyPeriod));

                dateOnlyPeriod = new DateOnlyPeriod(_baseDateTime, _baseDateTime.AddDays(7));
                Assert.IsFalse(ViewBaseHelper.CheckOverrideDayOffAndLoadedAndScheduledPeriod(person, dateOnlyPeriod));

                schedulePeriod.ResetDaysOff();
                Assert.IsTrue(ViewBaseHelper.CheckOverrideDayOffAndLoadedAndScheduledPeriod(person, dateOnlyPeriod));
            }
        }
        [Test]
        public void VerifyCheckLoadedAndScheduledPeriodTargetTime()
        {
            var personContract = _mockRep.StrictMock<IPersonContract>();

            _baseDateTime = new DateOnly(2009, 6, 1);
            var dateOnlyPeriod = new DateOnlyPeriod(_baseDateTime, _baseDateTime.AddDays(6));
            IPerson person = new Person();
            var info = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            person.PermissionInformation.SetDefaultTimeZone(info);
            var schedulePeriod = new SchedulePeriod(new DateOnly(2009, 6, 1), SchedulePeriodType.Week, 1)
                                     {
										 AverageWorkTimePerDayOverride = new TimeSpan(0, 7, 0)
                                     };

            person.AddSchedulePeriod(schedulePeriod);
            var personPeriod = _mockRep.StrictMock<IPersonPeriod>();
            using (_mockRep.Record())
            {
                Expect.Call(personPeriod.StartDate).Return(_baseDateTime).Repeat.AtLeastOnce();
                Expect.Call(() => personPeriod.SetParent(person));
                Expect.Call(personPeriod.PersonContract).Return(personContract).Repeat.AtLeastOnce();
                Expect.Call(personContract.Contract).Return(_contract).Repeat.AtLeastOnce();
 
            }
            using (_mockRep.Playback())
            {
                person.AddPersonPeriod(personPeriod);

                Assert.IsTrue(ViewBaseHelper.CheckOverrideTargetTimeLoadedAndScheduledPeriod(person, dateOnlyPeriod));

                dateOnlyPeriod = new DateOnlyPeriod(_baseDateTime, _baseDateTime.AddDays(7));
                Assert.IsFalse(ViewBaseHelper.CheckOverrideTargetTimeLoadedAndScheduledPeriod(person, dateOnlyPeriod));

                schedulePeriod.ResetAverageWorkTimePerDay();
                Assert.IsTrue(ViewBaseHelper.CheckOverrideTargetTimeLoadedAndScheduledPeriod(person, dateOnlyPeriod));
            }
            
        }

        /// <summary>
        /// Verifies the gets target days off are set to cell style value.
        /// </summary>
        [Test]
        public void VerifyGetsTargetDaysOffAreSetToCellStyleValue()
        {
            const int daysOff = 7;

            var baseDateTime = new DateOnly(2001, 1, 1);
            var baseDateTimePeriod = new DateOnlyPeriod(baseDateTime, baseDateTime.AddDays(10));
            var style = new GridStyleInfo();
            var person = _mockRep.StrictMock<IPerson>();
            var permissionInformation = _mockRep.StrictMock<IPermissionInformation>();
            ISchedulePeriod schedulePeriod = new SchedulePeriod(new DateOnly(2001, 01, 01), SchedulePeriodType.Day, 11);
			schedulePeriod.SetParent(person);
            var schedulePeriods = new List<ISchedulePeriod> { schedulePeriod };
            var vPeriod = _mockRep.StrictMock<IVirtualSchedulePeriod>();
			var range = _mockRep.StrictMock<IScheduleRange>();

            using (_mockRep.Record())
            {
                Expect.Call(person.PermissionInformation).Return(permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(permissionInformation.Culture()).Return(CultureInfo.CurrentCulture).Repeat.AtLeastOnce();
                Expect.Call(person.PersonSchedulePeriods(baseDateTimePeriod)).IgnoreArguments().Return(schedulePeriods).Repeat.AtLeastOnce();
                Expect.Call(person.VirtualSchedulePeriod(new DateOnly())).IgnoreArguments().Return(vPeriod).Repeat.AtLeastOnce();
            	Expect.Call(vPeriod.DaysOff()).Return(daysOff).Repeat.AtLeastOnce();
                Expect.Call(vPeriod.IsValid).Return(true).Repeat.AtLeastOnce();
            	Expect.Call(person.TerminalDate).Return(null).Repeat.AtLeastOnce();
            	Expect.Call(range.CalculatedTargetScheduleDaysOff).Return(null).Repeat.Once();
            	Expect.Call(() => range.CalculatedTargetScheduleDaysOff = daysOff).Repeat.Once();
            	Expect.Call(range.CalculatedTargetScheduleDaysOff).Return(daysOff).Repeat.Once();
            }

            using (_mockRep.Playback())
            {
                ViewBaseHelper.StyleTargetScheduleDaysOffCell(style, person, baseDateTimePeriod, range);
            }

            Assert.AreEqual(daysOff, style.CellValue);

        }

        [Test]
        public void VerifyAbsenceDisplayMode()
        {
            var period = new DateTimePeriod(2000, 1, 2, 2000, 1, 3);
            IList<IVisualLayer> layerCollectionAbscence = new List<IVisualLayer>();
            IList<IVisualLayer> layerCollectionActivity = new List<IVisualLayer>();
            IVisualLayer actLayer = _layerFactory.CreateShiftSetupLayer(ActivityFactory.CreateActivity("underlying"), period, _agent);
            layerCollectionAbscence.Add(_layerFactory.CreateAbsenceSetupLayer(AbsenceFactory.CreateAbsence("test"), actLayer, period));
            IVisualLayerCollection visualLayerCollectionAbsence = new VisualLayerCollection(_agent, layerCollectionAbscence, new ProjectionPayloadMerger());
            IVisualLayerCollection visualLayerCollectionActivity = new VisualLayerCollection(_agent, layerCollectionActivity, new ProjectionPayloadMerger());
            
            IPersonAbsence personAbsenceEndsToday = PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePerson(), _scenario, _periodEndsToday);
            IPersonAbsence personAbsenceBeginsToday = PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePerson(), _scenario, _periodBeginsToday);
            IPersonAbsence personAbsenceBeginsAndEndsToday = PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePerson(), _scenario, _periodBeginsAndEndsToday);

            IScheduleDictionary scheduleDictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period));
            ISchedulePart schedulePart = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, _agent, new DateOnly(2000,1,2));

            Assert.AreEqual(DisplayMode.EndsToday, ViewBaseHelper.GetAbsenceDisplayMode(personAbsenceEndsToday, schedulePart, visualLayerCollectionActivity));
            Assert.AreEqual(DisplayMode.BeginsToday, ViewBaseHelper.GetAbsenceDisplayMode(personAbsenceBeginsToday, schedulePart, visualLayerCollectionActivity));
            Assert.AreEqual(DisplayMode.BeginsAndEndsToday, ViewBaseHelper.GetAbsenceDisplayMode(personAbsenceBeginsAndEndsToday, schedulePart, visualLayerCollectionActivity));
            Assert.AreEqual(DisplayMode.WholeDay, ViewBaseHelper.GetAbsenceDisplayMode(personAbsenceBeginsAndEndsToday, schedulePart, visualLayerCollectionAbsence));


            scheduleDictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period.MovePeriod(TimeSpan.FromDays(10))));
            schedulePart = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, _agent, new DateOnly(2000, 1, 2).AddDays(10));

            Assert.AreEqual(DisplayMode.WholeDay, ViewBaseHelper.GetAbsenceDisplayMode(personAbsenceBeginsAndEndsToday, schedulePart, visualLayerCollectionActivity));
        }

        [Test]
        public void VerifyAssignmentDisplayMode()
        {
            var period = new DateTimePeriod(2000, 1, 2, 2000, 1, 3);

            IPersonAssignment personAssEndsToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario,
                                                    PersonFactory.CreatePerson(), _periodEndsToday);

            IPersonAssignment personAssBeginsToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario,
                                                    PersonFactory.CreatePerson(), _periodBeginsToday);

            IPersonAssignment personAssBeginsAndEndsToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario,
                                                    PersonFactory.CreatePerson(), _periodBeginsAndEndsToday);

            IPersonAssignment personAssWholeToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario,
                                                    PersonFactory.CreatePerson(), _periodWholeDay);

            IScheduleDictionary scheduleDictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period));
            var schedulePart = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, PersonFactory.CreatePerson(), new DateOnly(2000,1,2));

            Assert.AreEqual(DisplayMode.EndsToday, ViewBaseHelper.GetAssignmentDisplayMode(personAssEndsToday, schedulePart));
            Assert.AreEqual(DisplayMode.BeginsToday, ViewBaseHelper.GetAssignmentDisplayMode(personAssBeginsToday, schedulePart));
            Assert.AreEqual(DisplayMode.BeginsAndEndsToday, ViewBaseHelper.GetAssignmentDisplayMode(personAssBeginsAndEndsToday, schedulePart));
            Assert.AreEqual(DisplayMode.WholeDay, ViewBaseHelper.GetAssignmentDisplayMode(personAssWholeToday, schedulePart));
        }

        [Test]
        public void VerifyToLocalStartEndTimeString()
        {
            TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            string expected = string.Concat(_periodEndsToday.StartDateTimeLocal(timeZoneInfo).ToShortTimeString(), " - ",
                                            _periodEndsToday.EndDateTimeLocal(timeZoneInfo).ToShortTimeString());
            Assert.AreEqual(expected, ViewBaseHelper.ToLocalStartEndTimeString(_periodEndsToday, timeZoneInfo));
        }

        [Test]
        public void VerifyInfoTextWeekView()
        {
            var range = _mockRep.StrictMock<IScheduleRange>();
            using (_mockRep.Record())
            {
                Expect.Call(_schedulePart1.AssignmentHighZOrder()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart1.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart1.PersonAbsenceCollection()).Return(_personAbsenceCollection).Repeat.AtLeastOnce();

                Expect.Call(_schedulePart2.AssignmentHighZOrder()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart2.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart2.PersonAbsenceCollection()).Return(_personAbsenceCollection).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart2.PersonDayOffCollection()).Return(_personDayOffCollection).Repeat.AtLeastOnce();

                Expect.Call(_schedulePart3.AssignmentHighZOrder()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart3.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart3.PersonAbsenceCollection()).Return(_personAbsenceCollection).Repeat.AtLeastOnce();

                Expect.Call(_schedulePart3.TimeZone).Return(
                    (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))).Repeat.AtLeastOnce();
            }

            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_agent, range);

            IList<string> infoList = ViewBaseHelper.GetInfoTextWeekView(_schedulePart1, SchedulePartView.FullDayAbsence);
            string infoText = infoList[0];
            string periodText = infoList[1];
            string timeText = infoList[2];
            ViewBaseHelper.GetInfoTextWeekView(_schedulePart1, SchedulePartView.FullDayAbsence);
            Assert.AreEqual("description", infoText);
            Assert.AreEqual("-", periodText);
            Assert.AreEqual("00:00", timeText);

            infoList = ViewBaseHelper.GetInfoTextWeekView(_schedulePart1, SchedulePartView.FullDayAbsence);
            infoText = infoList[0];
            periodText = infoList[1];
            timeText = infoList[2];
            _visualLayers.Add(_visualLayer);
            ViewBaseHelper.GetInfoTextWeekView(_schedulePart1, SchedulePartView.FullDayAbsence);
            Assert.AreEqual("description", infoText);
            Assert.AreEqual("-", periodText);
            Assert.AreEqual("00:00", timeText);

            infoList = ViewBaseHelper.GetInfoTextWeekView(_schedulePart2, SchedulePartView.DayOff);
            infoText = infoList[0];
            periodText = infoList[1];
            timeText = infoList[2];
            ViewBaseHelper.GetInfoTextWeekView(_schedulePart2, SchedulePartView.DayOff);
            Assert.AreEqual(_personDayOff.DayOff.Description.Name, infoText);
            Assert.AreEqual("-", periodText);
            Assert.AreEqual("00:00", timeText);

            infoList = ViewBaseHelper.GetInfoTextWeekView(_schedulePart3, SchedulePartView.MainShift);
            infoText = infoList[0];
            periodText = infoList[1];
            timeText = infoList[2];
            ViewBaseHelper.GetInfoTextWeekView(_schedulePart3, SchedulePartView.MainShift);
            Assert.AreEqual("shiftcategory", infoText);
            Assert.AreEqual(_personAssignment.Period.LocalStartDateTime.ToShortTimeString() + " - " + _personAssignment.Period.LocalEndDateTime.ToShortTimeString(), periodText);
            Assert.AreEqual("00:00", timeText);
        }

		[Test]
		public void ShouldReturnPeriodFromSchedulePeriods()
		{
			var startDate1 = new DateOnly(2011, 1, 1);
			var startDate2 = new DateOnly(2011, 1, 8);
			var dateOnlyPeriod = new DateOnlyPeriod(startDate1, startDate2);

			_agent = PersonFactory.CreatePerson("person");
			var schedulePeriod1 = SchedulePeriodFactory.CreateSchedulePeriod(startDate1, SchedulePeriodType.Week, 1);
			var schedulePeriod2 = SchedulePeriodFactory.CreateSchedulePeriod(startDate2, SchedulePeriodType.Week, 1);



			var period = ViewBaseHelper.PeriodFromSchedulePeriods(new List<IPerson> { _agent }, dateOnlyPeriod);
			Assert.IsFalse(period.HasValue);

			_agent.AddSchedulePeriod(schedulePeriod1);
			_agent.AddSchedulePeriod(schedulePeriod2);

			if (!period.HasValue) return;
			Assert.AreEqual(startDate1, period.Value.StartDate);
			Assert.AreEqual(startDate2.AddDays(6), period.Value.StartDate);
		}

    	private IPersonAssignment CreatePersonAssignment()
        {
            IPersonAssignment personAssignment = new PersonAssignment(PersonFactory.CreatePerson(), _scenario);
            IMainShift mainShift = new MainShift(ShiftCategoryFactory.CreateShiftCategory("shiftcategory"));
            var start = new DateTime(2008, 11, 1, 10, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2008, 11, 1, 12, 0, 0, 0, DateTimeKind.Utc);
            var period = new DateTimePeriod(start, end);
            mainShift.LayerCollection.Add(new MainShiftActivityLayer(ActivityFactory.CreateActivity("activity"), period));
            personAssignment.SetMainShift(mainShift);

            return personAssignment;
        }

        private IPersonAbsence CreatePersonAbsence()
        {
            IPersonAbsence personAbsence = new PersonAbsence(PersonFactory.CreatePerson(), _scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("test"), new DateTimePeriod(2008, 11, 24, 2008, 11, 24)));
            personAbsence.Layer.Payload.Description = new Description("description");
            return personAbsence;
        }
    }
}
