using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture, SetCulture("sv-SE"), SetUICulture("en-US")]
    public class ViewBaseHelperTest
    {
        private IPerson _agent;
        private IScenario _scenario;
        private Rectangle _destRect;
        private Rectangle _expectedRect;
        private MockRepository _mockRep;
        private ScheduleParameters _param;
        private ScheduleRange _scheduleRange;
        private IPersonAssignment _ass1;
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
        private IPersonAssignment _personAssignment;
        private IPersonAbsence _personAbsence;
        private IPersonAbsence[] _personAbsenceCollection;
        private IList<IVisualLayer> _visualLayers;
        private IVisualLayer _visualLayer;
        private IVisualLayerCollection _visualLayerCollection;
        private VisualLayerFactory _layerFactory;
        private DateOnly _baseDateTime;
        private IDictionary<IPerson, IScheduleRange> _underlyingDictionary;
	    private IPersistableScheduleDataPermissionChecker _permissionChecker;

		[SetUp]
        public void Setup()
		{
			_layerFactory = new VisualLayerFactory();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
			_permissionChecker = new PersistableScheduleDataPermissionChecker(new FullPermission());
            IPerson person = PersonFactory.CreatePerson();
			
						_agent = PersonFactory.CreatePersonWithPersonPeriod(person, new DateOnly(1999, 1, 1), new List<ISkill>(), new Contract("ctr"), new PartTimePercentage("ptc"));
            _mockRep = new MockRepository();
            _schedulePart1 = _mockRep.StrictMock<IScheduleDay>();
            _schedulePart2 = _mockRep.StrictMock<IScheduleDay>();
            _schedulePart3 = _mockRep.StrictMock<IScheduleDay>();
            _projectionService = _mockRep.StrictMock<IProjectionService>();

            _personAssignment = CreatePersonAssignment();
            _personAbsence = CreatePersonAbsence();
            _personAbsenceCollection = new [] { _personAbsence };
            _visualLayers = new List<IVisualLayer>();
			var actLayer = _layerFactory.CreateShiftSetupLayer(ActivityFactory.CreateActivity("activity"),
				_personAbsence.Period);
			_visualLayer = _layerFactory.CreateAbsenceSetupLayer(_personAbsence.Layer.Payload, actLayer, _personAbsence.Period);
            _visualLayerCollection = new VisualLayerCollection(_visualLayers, new ProjectionPayloadMerger());

            _underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
            _dic = new ScheduleDictionaryForTest(_scenario,
                                                new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2010, 1, 1)),
                                                _underlyingDictionary);
             _param = new ScheduleParameters(_scenario, _agent,
                                           new DateTimePeriod(2000, 1, 1, 2010, 1, 1));
             _scheduleRange = new ScheduleRange(_dic, _param, _permissionChecker, new FullPermission());
        	var act = ActivityFactory.CreateActivity("sdfsdf");
        	act.InWorkTime = true;
			_ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_agent,
				_scenario, act, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2000, 12, 31, 21, 0, 0),
					new DateTime(2000, 12, 31, 22, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), ShiftCategoryFactory.CreateShiftCategory("Morgon"));


            //create absences
            _absence = AbsenceFactory.CreateAbsence("Tjänsteresa", "TJ", Color.DimGray);

            var meeting1 = new Meeting(_agent, new List<IMeetingPerson>(), "meeting1", "location1", "description1", ActivityFactory.CreateActivity("activity1"), _scenario)
                            {
                                StartDate = new DateOnly(2006, 1, 1),
                                EndDate = new DateOnly(2006, 1, 1),
                                StartTime = TimeSpan.FromHours(8),
                                EndTime = TimeSpan.FromHours(9)
                            };
             var meeting2 = new Meeting(_agent, new List<IMeetingPerson>(), "meeting2", "location2", "description2", ActivityFactory.CreateActivity("activity2"), _scenario)
                            {
                                StartDate = new DateOnly(2006, 1, 1),
                                EndDate = new DateOnly(2006, 1, 1),
                                StartTime = TimeSpan.FromHours(18),
                                EndTime = TimeSpan.FromHours(19)
                            };

            IMeetingPerson meetingPersonRequired = new MeetingPerson(_agent, false);
            IMeetingPerson meetingPersonOptional = new MeetingPerson(_agent, true);

            meeting1.AddMeetingPerson(meetingPersonRequired);
            meeting2.AddMeetingPerson(meetingPersonOptional);

			var abs = new PersonAbsence(_agent, _scenario,
									 new AbsenceLayer(_absence, new DateTimePeriod(2001, 1, 1, 2006, 1, 1)));

            //add to schedule
            _scheduleRange.Add(_ass1);
            _scheduleRange.Add(abs);
            _scheduleRange.AddRange(meeting1.GetPersonMeetings(_dic.Period.LoadedPeriod(), _agent));
            _scheduleRange.AddRange(meeting2.GetPersonMeetings(_dic.Period.LoadedPeriod(), _agent));

            var start = new DateTime(2007, 10, 1, 6, 0, 0);
            var end = new DateTime(2007, 10, 1, 10, 0, 0);
            _periodBounds = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start,end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

            _nightlyRest = new TimeSpan(8, 0, 0);

            _contract = ContractFactory.CreateContract("for test");
            _contract.WorkTimeDirective = new WorkTimeDirective(new TimeSpan(0, 0, 0), new TimeSpan(40, 0, 0),
                                                               _nightlyRest,
                                                               new TimeSpan(50, 0, 0));
            _contract.MinTimeSchedulePeriod = new TimeSpan(1);

			TimeZoneGuardForDesktop.Instance_DONTUSE.Set(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
        }

		[TearDown]
		public void Teardown()
		{
			TimeZoneGuardForDesktop.Instance_DONTUSE.Set(null);
		}

		[Test]
        public void VerifyToolTipDayOff()
        {
			var scheduleRange = new ScheduleRange(_dic, _param, _permissionChecker, new FullPermission());
	        var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(_agent,
	                                                                                  _scenario,
	                                                                                  new DateOnly(2001, 1, 1), DayOffFactory.CreateDayOff(new Description("hej", "DÅ")));
			scheduleRange.Add(personAssignment);

			StringAssert.Contains("hej",
                ViewBaseHelper.GetToolTipDayOff(scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1))));
        }

        [Test]
        public void VerifyToolTipAssignments()
        {
            _param = new ScheduleParameters(_scenario, _agent,
                                           new DateTimePeriod(2000, 1, 1, 2001, 1, 5));
            _scheduleRange = new ScheduleRange(_dic, _param, _permissionChecker, new FullPermission());

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_agent,
					_scenario, ActivityFactory.CreateActivity("sdfsdf"), TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 5, 0, 0), new DateTime(2001, 1, 2, 6, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), ShiftCategoryFactory.CreateShiftCategory("Morgon"));

            _scheduleRange.Add(_ass1);
            _scheduleRange.Add(ass);
            
            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_scheduleRange.Person, _scheduleRange);

            StringAssert.Contains("Morgon", ViewBaseHelper.GetToolTipAssignments(_scheduleRange.ScheduledDay(new DateOnly(2001,1,1))));
        }

		[Test]
	    public void ShouldNotIncludePersonalActivityOutsideProjectionInToolTipAssignment()
		{
			var dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 5, 0, 0), new DateTime(2001, 1, 1, 13, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			var dateTimePeriodPersonal = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 3, 0, 0), new DateTime(2001, 1, 1, 4, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			_param = new ScheduleParameters(_scenario, _agent, new DateTimePeriod(2000, 1, 1, 2001, 1, 5));
			_scheduleRange = new ScheduleRange(_dic, _param, _permissionChecker, new FullPermission());

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_agent, _scenario, ActivityFactory.CreateActivity("sdfsdf"), dateTimePeriod, ShiftCategoryFactory.CreateShiftCategory("SC"));
			ass.AddPersonalActivity(ActivityFactory.CreateActivity("personal"), dateTimePeriodPersonal);
			_scheduleRange.Add(ass);
			_underlyingDictionary.Clear();
			_underlyingDictionary.Add(_scheduleRange.Person, _scheduleRange);

			var result =  ViewBaseHelper.GetToolTipAssignments(_scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1)));
			var expectedStart = "SC  " + dateTimePeriod.StartDateTime.ToShortTimeString() + " - " + dateTimePeriod.EndDateTime.ToShortTimeString();
			Assert.IsTrue(result.StartsWith(expectedStart));
		}

		[Test]
		public void ShouldNotIncludePersonalActivityOutsideProjectionInPeriodTextWeekView()
		{
			var dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 5, 0, 0), new DateTime(2001, 1, 1, 13, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			var dateTimePeriodPersonal = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 3, 0, 0), new DateTime(2001, 1, 1, 4, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			_param = new ScheduleParameters(_scenario, _agent, new DateTimePeriod(2000, 1, 1, 2001, 1, 5));
			_scheduleRange = new ScheduleRange(_dic, _param, _permissionChecker, new FullPermission());

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_agent, _scenario, ActivityFactory.CreateActivity("sdfsdf"), dateTimePeriod, ShiftCategoryFactory.CreateShiftCategory("SC"));
			ass.AddPersonalActivity(ActivityFactory.CreateActivity("personal"), dateTimePeriodPersonal);
			_scheduleRange.Add(ass);
			_underlyingDictionary.Clear();
			_underlyingDictionary.Add(_scheduleRange.Person, _scheduleRange);


			var infoText = ViewBaseHelper.GetInfoTextWeekView(_scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1)), SchedulePartView.MainShift);
			var periodText = infoText[1];
			var expectedStart = dateTimePeriod.StartDateTime.ToShortTimeString() + " - " + dateTimePeriod.EndDateTime.ToShortTimeString();

			Assert.IsTrue(periodText.StartsWith(expectedStart));
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
            var absCollection = new [] { personAbsence };
            var part = _mockRep.StrictMock<IScheduleDay>();
			string expectedStart = "Tjänsteresa: " + TimeZoneHelper.ConvertFromUtc(startTime, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()).ToShortTimeString() + 
                                            " - " + partStartperiod.EndDateTimeLocal(TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()).ToShortTimeString();

			string expectedMiddle = "Tjänsteresa: " + partStartperiod.StartDateTimeLocal(TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()).ToShortTimeString() +
											" - " + partStartperiod.EndDateTimeLocal(TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()).ToShortTimeString();

			string expectedEnd = "Tjänsteresa: " + partStartperiod.StartDateTimeLocal(TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()).ToShortTimeString() +
											" - " + TimeZoneHelper.ConvertFromUtc(endTime, TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()).ToShortTimeString();

            using (_mockRep.Record())
            {
                Expect.Call(part.PersonAbsenceCollection()).Return(absCollection).Repeat.AtLeastOnce();
                Expect.Call(part.Period).Return(partStartperiod);
                Expect.Call(part.Period).Return(partMiddlePeriod);
                Expect.Call(part.Period).Return(partEndPeriod);
            }

            using (_mockRep.Playback())
            {
                Assert.AreEqual(expectedStart, ViewBaseHelper.GetToolTipAbsences(part));  
                Assert.AreEqual(expectedMiddle, ViewBaseHelper.GetToolTipAbsences(part));
                Assert.AreEqual(expectedEnd, ViewBaseHelper.GetToolTipAbsences(part));
            }
        }

        [Test]
        public void VerifyConfidentialAbsence()
        {
            IAbsence confidentialAbsence = AbsenceFactory.CreateAbsence("Confidential", "CF", Color.DimGray);
            confidentialAbsence.Confidential = true;

            IPersonAbsence confidentialPersonAbsence = new PersonAbsence(_agent, _scenario,
                                    new AbsenceLayer(confidentialAbsence, new DateTimePeriod(2006, 1, 2, 2006, 1, 3)));

            _agent.SetId(Guid.NewGuid());
            _param = new ScheduleParameters(_scenario, _agent,
                               new DateTimePeriod(2006, 1, 1, 2006, 1, 10));

			var authorization = _mockRep.StrictMock<IAuthorization>();
			
            using(_mockRep.Record())
            {
                Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewConfidential,
                                                      DateOnly.Today, _agent)).Return(false);
                Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)).
                    Return(true);
				Expect.Call(authorization.PermittedPeriods(DefinedRaptorApplicationFunctionPaths.ViewSchedules,new DateOnlyPeriod(2006,1,1,2006,1,9),_agent)).
					Return(new []{new DateOnlyPeriod(2006, 1, 1, 2006, 1, 9)});
			}

			using (_mockRep.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					_scheduleRange =
						new ScheduleRange(_dic, _param, _permissionChecker, new ThisAuthorization(authorization));
					_scheduleRange.Add(confidentialPersonAbsence);

					//rk ändrat här. ska det visas agentens tid eller betraktarens tid?
					string absencePeriod = confidentialPersonAbsence.Layer.Period.StartDateTime.ToShortTimeString() +
										   " - " +
										   confidentialPersonAbsence.Layer.Period.EndDateTime.ToShortTimeString();


					string expected = ConfidentialPayloadValues.Description.Name + ": " + absencePeriod;

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
			IList<IPersonPeriod> personPeriods = _agent.PersonPeriods(period.ToDateOnlyPeriod(TimeZoneGuardForDesktop.Instance_DONTUSE.CurrentTimeZone()));
            IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
                MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("Paid Overtime",
                                                                                   MultiplicatorType.Overtime);
            personPeriods[0].PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);

						var personAssignment = new PersonAssignment(_agent, _scheduleRange.Scenario, new DateOnly(2008, 1, 1));
            IActivity activity = ActivityFactory.CreateActivity("Overtime activity");
						personAssignment.AddOvertimeActivity(activity, period, multiplicatorDefinitionSet);

            var part = _scheduleRange.ScheduledDay(new DateOnly(2008, 1, 1));
            part.Add(personAssignment);

            //rk ändrat här. ska det visas agentens tid eller betraktarens tid?
            string expected = string.Concat(multiplicatorDefinitionSet.Name, ": ", activity.Name, ": ", "17:00 - 18:00");

            Assert.AreEqual(expected, ViewBaseHelper.GetToolTipOvertime(part));
        }

        [Test]
        public void VerifyGetToolTip()
        {
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_agent,
				  _scenario, ActivityFactory.CreateActivity("sdfsdf"), TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 2, 0, 0), new DateTime(2001, 1, 1, 3, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), ShiftCategoryFactory.CreateShiftCategory("Morgon"));

            _scheduleRange.Add(ass);

            _underlyingDictionary.Clear();
            _underlyingDictionary.Add(_scheduleRange.Person,_scheduleRange);

            StringAssert.Contains("Morgon", ViewBaseHelper.GetToolTip(_scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1))));
        }

        [Test]
        public void VerifyEmptyToolTip()
        {
            _param = new ScheduleParameters(_scenario, _agent,
                                           new DateTimePeriod(2000, 12, 31, 2001, 1, 1));

            _scheduleRange = new ScheduleRange(_dic, _param, _permissionChecker, new FullPermission());

            string expected = string.Empty;

            Assert.AreEqual(expected, ViewBaseHelper.GetToolTip(_scheduleRange.ScheduledDay(new DateOnly(2000, 1, 1))));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyRuleConflictToolTip()
		{
				var schedulePeriod = new DateTimePeriod(2000, 11, 1, 2001, 5, 1);
				_param = new ScheduleParameters(_scenario, _agent,
					schedulePeriod);

				_scheduleRange = new ScheduleRange(_dic, _param, _permissionChecker, new FullPermission());
				_underlyingDictionary[_agent] = _scheduleRange;
				ITeam team = TeamFactory.CreateSimpleTeam();

				_agent.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1),
					new PersonContract(_contract, new PartTimePercentage("sdf"), new ContractSchedule("sdf")),
					team));


				var noRest = CreateNoRestAss();
				_scheduleRange.Add(noRest);

				_scheduleRange.Add(_ass1);


				var newRules = NewBusinessRuleCollection.Minimum();
				newRules.Add(new NewNightlyRestRule(new WorkTimeStartEndExtractor()));
				_dic.ValidateBusinessRulesOnPersons(new List<IPerson> {_agent}, newRules);

				var parts = _scheduleRange.ScheduledDay(new DateOnly(2001, 1, 1));

				string tt = ViewBaseHelper.GetToolTip(parts);
				int pos = tt.IndexOf("There must be a nightly rest of at least", StringComparison.OrdinalIgnoreCase);
				Assert.AreNotEqual(-1, pos);
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
            var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(start, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), TimeZoneHelper.ConvertToUtc(end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone));

            _destRect = new Rectangle(0, 0, 400, 40);
            
            Rectangle layerRect = ViewBaseHelper.GetLayerRectangle(_periodBounds, _destRect, period, false);

            Assert.IsTrue(layerRect.IsEmpty);
        }

        [Test]
        public void VerifyGetLayerRectangleRightToLeft()
        {
            var start = new DateTime(2007, 10, 1, 7, 0, 0);
            var end = new DateTime(2007, 10, 1, 10, 0, 0);
            var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(start, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), TimeZoneHelper.ConvertToUtc(end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone));

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
            IPersonAssignment noRest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_agent,
                  _scenario, act, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 4, 0, 0), new DateTime(2001, 1, 1, 21, 0, 0), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), ShiftCategoryFactory.CreateShiftCategory("Morgon"));

            return noRest;
        }

		[Test]
		public void ShouldSetCellValueTargetTimeToNaWhenSchedulePeriodAndOpenPeriodDoNotMatch()
		{
			var person = PersonFactory.CreatePerson();
			var personWithSchedulePeriod = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2001, 1, 2));

			using (var style = new GridStyleInfo())
			{
				var baseDateTime = new DateOnly(2001, 1, 1);
				var baseDateTimePeriod = new DateOnlyPeriod(baseDateTime, baseDateTime.AddDays(10));
				
				var range = _mockRep.StrictMock<IScheduleRange>();
				var schedulingResultStateHolder = _mockRep.StrictMock<ISchedulingResultStateHolder>();

				ViewBaseHelper.StyleTargetScheduleContractTimeCell(style, personWithSchedulePeriod, baseDateTimePeriod, schedulingResultStateHolder, range);
		
				Assert.AreEqual(UserTexts.Resources.NA, style.CellValue);
			}

		}

		[Test]
		public void ShouldSetCellValueTargetDayOffToNaWhenSchedulePeriodAndOpenPeriodDoNotMatch()
		{
			var person = PersonFactory.CreatePerson();
			var personWithSchedulePeriod = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(person, new DateOnly(2001, 1, 2));

			using (var style = new GridStyleInfo())
			{
				var baseDateTime = new DateOnly(2001, 1, 1);
				var baseDateTimePeriod = new DateOnlyPeriod(baseDateTime, baseDateTime.AddDays(10));

				var range = _mockRep.StrictMock<IScheduleRange>();
				
				ViewBaseHelper.StyleTargetScheduleDaysOffCell(style, personWithSchedulePeriod, baseDateTimePeriod, range);

				Assert.AreEqual(UserTexts.Resources.NA, style.CellValue);
			}	
		}

		[Test]
		public void ShouldSetCellValueTargetTimeToNaWhenHourlyEmployee()
		{
			var scenario = new Scenario();
			var date = new DateOnly(2018, 04, 04);
			var period = new DateOnlyPeriod(date, date);
			var contract = new Contract("_") {EmploymentType = EmploymentType.HourlyStaff};
			var person = new Person().WithPersonPeriod(contract).WithSchedulePeriodOneDay(date);
			var authorization = new FullPermission();
			var checker = new PersistableScheduleDataPermissionChecker(authorization);
			var scheduleDictionary = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(period.ToDateTimePeriod(TimeZoneInfo.Utc), new[] {person},new SchedulerRangeToLoadCalculator(period.ToDateTimePeriod(TimeZoneInfo.Utc))), checker, authorization);
			var scheduleRange = new ScheduleRange(scheduleDictionary, new ScheduleParameters(scenario, person, period.ToDateTimePeriod(TimeZoneInfo.Utc)), checker, authorization);
			using (var style = new GridStyleInfo())
			{
				ViewBaseHelper.StyleTargetScheduleContractTimeCell(style, person, period, new SchedulingResultStateHolder(), scheduleRange);
				Assert.AreEqual(UserTexts.Resources.NA, style.CellValue);
			}
		}

		[Test]
		public void ShouldSetCellValueTargetDayOffToNaWhenHourlyEmployee()
		{
			var scenario = new Scenario();
			var date = new DateOnly(2018, 04, 04);
			var period = new DateOnlyPeriod(date, date);
			var contract = new Contract("_") { EmploymentType = EmploymentType.HourlyStaff };
			var person = new Person().WithPersonPeriod(contract).WithSchedulePeriodOneDay(date);
			var authorization = new FullPermission();
			var checker = new PersistableScheduleDataPermissionChecker(authorization);
			var scheduleDictionary = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(period.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { person }, new SchedulerRangeToLoadCalculator(period.ToDateTimePeriod(TimeZoneInfo.Utc))), checker, authorization);
			var scheduleRange = new ScheduleRange(scheduleDictionary, new ScheduleParameters(scenario, person, period.ToDateTimePeriod(TimeZoneInfo.Utc)), checker, authorization);
			using (var style = new GridStyleInfo())
			{
				ViewBaseHelper.StyleTargetScheduleDaysOffCell(style, person, period, scheduleRange);
				Assert.AreEqual(UserTexts.Resources.NA, style.CellValue);
			}
		}
		       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldThrowExceptionOnNullStyle()
        {
            Assert.Throws<ArgumentNullException>(() => ViewBaseHelper.StyleTargetScheduleDaysOffCell(null, null, new DateOnlyPeriod(), null));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldThrowExceptionOnNullPerson()
        {
            var style = new GridStyleInfo();
            Assert.Throws<ArgumentNullException>(() => ViewBaseHelper.StyleTargetScheduleDaysOffCell(style, null, new DateOnlyPeriod(), null));
        }

        [Test]
        public void VerifyCheckLoadedAndScheduledPeriodDayOff()
        {
            _baseDateTime = new DateOnly(2009, 6, 1);
            var dateOnlyPeriod = new DateOnlyPeriod(_baseDateTime, _baseDateTime.AddDays(6));
	        var personContract = _mockRep.DynamicMock<IPersonContract>();
            var person = PersonFactory.CreatePersonWithPersonPeriod(_baseDateTime,Enumerable.Empty<ISkill>());
			var personPeriod = person.Period(_baseDateTime);
            var info = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            person.PermissionInformation.SetDefaultTimeZone(info);
            var schedulePeriod = new SchedulePeriod(new DateOnly(2009, 6, 1), SchedulePeriodType.Week, 1);
            schedulePeriod.SetDaysOff(2);
            person.AddSchedulePeriod(schedulePeriod);
			
			personPeriod.PersonContract = personContract;
			    
            using (_mockRep.Record())
            {
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
            _baseDateTime = new DateOnly(2009, 6, 1);
            var dateOnlyPeriod = new DateOnlyPeriod(_baseDateTime, _baseDateTime.AddDays(6));
            var person = PersonFactory.CreatePersonWithPersonPeriod(_baseDateTime,Enumerable.Empty<ISkill>());
	        var personPeriod = person.Period(_baseDateTime);
            var info = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            person.PermissionInformation.SetDefaultTimeZone(info);
            var schedulePeriod = new SchedulePeriod(new DateOnly(2009, 6, 1), SchedulePeriodType.Week, 1)
                                     {
										 AverageWorkTimePerDayOverride = new TimeSpan(0, 7, 0)
                                     };

            person.AddSchedulePeriod(schedulePeriod);
	        personPeriod.PersonContract = new PersonContract(_contract,
		        PartTimePercentageFactory.CreatePartTimePercentage("Full time"),
		        ContractScheduleFactory.CreateWorkingWeekContractSchedule());

	        using (_mockRep.Record())
	        {
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

        [Test]
        public void VerifyAssignmentDisplayMode()
        {
            var period = new DateTimePeriod(2000, 1, 2, 2000, 1, 3);

            IPersonAssignment personAssEndsToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(), _scenario, _periodEndsToday);

            IPersonAssignment personAssBeginsToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(), _scenario, _periodBeginsToday);

            IPersonAssignment personAssBeginsAndEndsToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(), _scenario, _periodBeginsAndEndsToday);

            IPersonAssignment personAssWholeToday = PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(), _scenario, _periodWholeDay);

            IScheduleDictionary scheduleDictionary = new ScheduleDictionary(_scenario, new ScheduleDateTimePeriod(period), _permissionChecker, new FullPermission());
            var schedulePart = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, PersonFactory.CreatePerson(), new DateOnly(2000,1,2), new FullPermission());

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
	        var dayOffTemplate = DayOffFactory.CreateDayOff(new Description("Tjillevippen", "xy"));
			_personAssignment.SetDayOff(dayOffTemplate);
            var range = _mockRep.StrictMock<IScheduleRange>();
            using (_mockRep.Record())
            {
                Expect.Call(_schedulePart1.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart1.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart1.PersonAbsenceCollection()).Return(_personAbsenceCollection).Repeat.AtLeastOnce();

                Expect.Call(_schedulePart2.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart2.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart2.PersonAbsenceCollection()).Return(_personAbsenceCollection).Repeat.AtLeastOnce();

                Expect.Call(_schedulePart3.PersonAssignment()).Return(_personAssignment).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart3.ProjectionService()).Return(_projectionService).Repeat.AtLeastOnce();
                Expect.Call(_schedulePart3.PersonAbsenceCollection()).Return(_personAbsenceCollection).Repeat.AtLeastOnce();

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
            Assert.AreEqual("Tjillevippen", infoText);
            Assert.AreEqual("-", periodText);
            Assert.AreEqual("00:00", timeText);
        }

	    [Test]
		public void ShouldMatchSchedulePeriodWithOpenPeriodWhenPreviousPeriodIntersectOpenPeriod()
	    {
		    var schedulePeriodDateFrom1 = new DateOnly(2015, 11, 23);
			var schedulePeriodDateFrom2 = new DateOnly(2015, 11, 30);
			var openPeriodDateFrom = new DateOnly(2015, 11, 30);
			var openPeriodDateTo = new DateOnly(2015, 12, 27);
			var openPeriod = new DateOnlyPeriod(openPeriodDateFrom, openPeriodDateTo);
			var schedulePeriod1 = new SchedulePeriod(schedulePeriodDateFrom1, SchedulePeriodType.Week, 4);
			var schedulePeriod2 = new SchedulePeriod(schedulePeriodDateFrom2, SchedulePeriodType.Week, 4);

			_agent.AddSchedulePeriod(schedulePeriod1);
			_agent.AddSchedulePeriod(schedulePeriod2);

			var match = ViewBaseHelper.CheckOpenPeriodMatchSchedulePeriod(_agent, openPeriod);

			Assert.IsTrue(match);  
	    }

	    private IPersonAssignment CreatePersonAssignment()
	    {
		    IPersonAssignment personAssignment = new PersonAssignment(PersonFactory.CreatePerson(), _scenario,
			    new DateOnly(2008, 11, 1));
		    var mainShift = new EditableShift(ShiftCategoryFactory.CreateShiftCategory("shiftcategory"));
		    var start = new DateTime(2008, 11, 1, 10, 0, 0, DateTimeKind.Utc);
		    var end = new DateTime(2008, 11, 1, 12, 0, 0, 0, DateTimeKind.Utc);
		    var period = new DateTimePeriod(start, end);
		    mainShift.LayerCollection.Add(new EditableShiftLayer(ActivityFactory.CreateActivity("activity"), period));
		    new EditableShiftMapper().SetMainShiftLayers(personAssignment, mainShift);

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
