using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture, SetUICulture("en-US")]
    public class RequestPresenterTest
    {
        private IRequestPresenter _requestPresenter;
        private IHandleBusinessRuleResponse _handleBusinessRuleResponse;
        private IList<PersonRequestViewModel> _requestViewAdapters;
        private PersonRequestViewModel _request1;
        private PersonRequestViewModel _request2;
        private PersonRequestViewModel _request3;
        private MockRepository _mocks;
        private IScheduleDictionary _schedules;
        private IScheduleDateTimePeriod _scheduleDateTimePeriod;
        private DateTimePeriod _dateTimePeriod;
        private IScenario _scenario;
        private IViewBase _view;

        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
        private IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private TimeZoneInfo _TimeZoneInfo;
        private IDictionary<Guid, IPerson> filteredPersons;
	    private IGlobalSettingDataRepository _globalSettingRepo;

	    [SetUp]
        public void Setup()
        {
            _TimeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _requestViewAdapters = new List<PersonRequestViewModel>();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
		    DateTime startDateTime = DateTime.SpecifyKind(DateTime.Today.AddDays(7), DateTimeKind.Utc);
            DateTime endDateTime = startDateTime.AddHours(2);
            _dateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
            _scheduleDateTimePeriod = new ScheduleDateTimePeriod(_dateTimePeriod);
            _schedules = new ScheduleDictionary(_scenario, _scheduleDateTimePeriod);
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IViewBase>();
            _handleBusinessRuleResponse = _mocks.DynamicMock<IHandleBusinessRuleResponse>();
            _requestPresenter = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
            _shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
	        _globalSettingRepo = _mocks.StrictMock<IGlobalSettingDataRepository>();

            _person1 = PersonFactory.CreatePerson("A", "B");
            _person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            _person2 = PersonFactory.CreatePerson("A", "A");
            _person2.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            _person3 = PersonFactory.CreatePerson("A", "B");
            _person3.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            IAbsence absence = AbsenceFactory.CreateAbsence("abs");

            DateTimePeriod period1 = new DateTimePeriod(startDateTime, endDateTime);
            PersonRequest personRequest1 = new PersonRequest(_person1);
            personRequest1.TrySetMessage("b");
            personRequest1.Subject = "Absence Request1";
            personRequest1.Request = new AbsenceRequest(absence, period1);
            personRequest1.Pending();

            _request1 = new PersonRequestViewModel(personRequest1, _shiftTradeRequestStatusChecker, null, null, _TimeZoneInfo);
            
            

            DateTimePeriod period2 = new DateTimePeriod(startDateTime,
                                                endDateTime);
            PersonRequest personRequest2 = new PersonRequest(_person2);
            personRequest2.TrySetMessage("b");
            personRequest2.Subject = "Absence Request2";
            personRequest2.Request = new AbsenceRequest(absence, period2);
            personRequest2.Pending();
            _request2 = new PersonRequestViewModel(personRequest2, _shiftTradeRequestStatusChecker, null, null, _TimeZoneInfo);

            DateTimePeriod period3 = new DateTimePeriod(startDateTime,
                                                endDateTime);
            PersonRequest personRequest3 = new PersonRequest(_person3);
            personRequest3.TrySetMessage("a");
            personRequest3.Subject = "Absence Request3";
            personRequest3.Request = new AbsenceRequest(absence, period3);
            personRequest3.Pending();
            _request3 = new PersonRequestViewModel(personRequest3, _shiftTradeRequestStatusChecker, null, null, _TimeZoneInfo);

            _requestViewAdapters.Add(_request1);
            _requestViewAdapters.Add(_request2);
            _requestViewAdapters.Add(_request3);


            filteredPersons = new Dictionary<Guid, IPerson>();
            filteredPersons.Add(_request1.PersonRequest.Person.Id.GetValueOrDefault(), _request1.PersonRequest.Person);
        }

        [Test]
        public void VerifyFilterAdapters()
        {
	        var filterExpression = new List<string>();
            
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filterExpression);
            Assert.AreEqual(3, list.Count);

            list = RequestPresenter.FilterAdapters(_requestViewAdapters, filterExpression);
            Assert.AreEqual(3, list.Count);

        }

        [Test]
        public void VerifyThatPersonNameFound()
        {
            var filterExpression = new List<string>();
            filterExpression.Add("A");
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filterExpression);
            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        public void ShouldNotReturnAnyRequestIfNoTextFound()
        {
            var filerExpression = new List<string>();
            filerExpression.Add("NOT FOUND");
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filerExpression);
            Assert.AreEqual(list.Count, 0);
        }

		[Ignore("Temporary disalbed, maybe depends on current time")]
        [Test]
        public void ShouldReturnThoseRequestThatRequestedInCurrentYear()
        {
            var filerExpression = new List<string>();
            var year = DateTime.Now.Year.ToString();
            filerExpression.Add(year);
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filerExpression);
            Assert.AreEqual(list.Count, 3);
        }

        [Test]
		public void ShouldOnlyShowFilteredPersons()
		{
			foreach (var request in _requestViewAdapters)
			{
				request.PersonRequest.Person.SetId(Guid.NewGuid());
			}
			var result = RequestPresenter.FilterAdapters(_requestViewAdapters,
			                                new List<Guid> {_request1.PersonRequest.Person.Id.GetValueOrDefault()});
			result.Count.Should().Be.EqualTo(1);
			result.First().Should().Be.EqualTo(_request1);
		}

        
        [Test]
        public void ShouldReturnRequestsWhichContainTextToBeSearched()
        {
            var filerExpression = new List<string>();
            filerExpression.Add("Request2");
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filerExpression);
            Assert.AreEqual(list.Count, 1);
        }

        [Test]
        public void ShouldReturnRequestsFromtheCurrentMonth()
        {
            var filerExpression = new List<string>();
            var month = DateTime.Now.AddDays(7).Month;
            filerExpression.Add(month.ToString());
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filerExpression);
            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        public void ShouldReturnRequestsOfTypeAbsence()
        {
            var filerExpression = new List<string>();
            filerExpression.Add("Absence");
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filerExpression);
            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        public void ShouldReturnRequestWhichContainsSearchedAbsenceName()
        {
            var filerExpression = new List<string>();
            filerExpression.Add("abs");
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            var list = RequestPresenter.FilterAdapters(_requestViewAdapters, filerExpression);
            Assert.AreEqual(list.Count, 3);
        }

        [Test]
        public void VerifyCanApproveStatusOnRequest()
        {
            
            var newBusinessRuleCollection = _mocks.DynamicMock<INewBusinessRuleCollection>();

            Expect.Call(newBusinessRuleCollection.CheckRules(null, null)).IgnoreArguments().Return(
                new List<IBusinessRuleResponse>());
            
            _mocks.ReplayAll();

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }

            //Change status
            _requestPresenter.ApproveOrDeny(_requestViewAdapters,
                                            new ApprovePersonRequestCommand(_view, _schedules, _scenario,
                                                                            _requestPresenter,
                                                                            _handleBusinessRuleResponse,
                                                                            new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingRepo), string.Empty);

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.IsApproved);
                Assert.IsFalse(adapter.IsDenied);
                Assert.IsFalse(adapter.IsPending);
                Assert.IsFalse(adapter.IsNew);
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanDenyStatusOnRequest()
        {
            _mocks.ReplayAll();

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }

            //Change status
            _requestPresenter.ApproveOrDeny(_requestViewAdapters, new DenyPersonRequestCommand(_requestPresenter, new PersonRequestAuthorizationCheckerForTest()), string.Empty);

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsDenied);
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyReplyOnRequest()
        {
            _mocks.ReplayAll();
            var undo = new UndoRedoContainer(100);
            _requestPresenter.SetUndoRedoContainer(undo);

            _requestPresenter.Reply(_requestViewAdapters, "Testin'");

			Assert.AreEqual("b\r\nTestin'", _requestViewAdapters[0].GetMessage(new NoFormatting()));
			Assert.AreEqual("b\r\nTestin'", _requestViewAdapters[1].GetMessage(new NoFormatting()));
			Assert.AreEqual("a\r\nTestin'", _requestViewAdapters[2].GetMessage(new NoFormatting()));

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyBrokenRulesOverridden()
        {
            _requestViewAdapters.RemoveAt(2);
            _requestViewAdapters.RemoveAt(1);

            IScheduleRange totalScheduleRange = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay daySchedule = _mocks.StrictMock<IScheduleDay>();
            
            var newBusinessRuleCollection = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
            
            _schedules = _mocks.StrictMock<IScheduleDictionary>();
			expects(totalScheduleRange, daySchedule, daySchedule, false);

            _mocks.ReplayAll();

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }

            //Change status
            _requestPresenter.ApproveOrDeny(_requestViewAdapters,
                                            new ApprovePersonRequestCommand(_view, _schedules, _scenario,
                                                                            _requestPresenter,
                                                                            _handleBusinessRuleResponse,
                                                                            new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingRepo), string.Empty);

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsApproved);
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyMandatoryBrokenRules()
        {
            _requestViewAdapters.RemoveAt(2);
            _requestViewAdapters.RemoveAt(1);

            IScheduleRange totalScheduleRange = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay daySchedule = _mocks.StrictMock<IScheduleDay>();

            _schedules = _mocks.StrictMock<IScheduleDictionary>();
			expectsForMandatoryRuleViolation(totalScheduleRange, daySchedule, daySchedule, true);
            var newBusinessRuleCollection = _mocks.StrictMock<INewBusinessRuleCollection>();

            _mocks.ReplayAll();

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }

            //Change status
            _requestPresenter.ApproveOrDeny(_requestViewAdapters,
                                            new ApprovePersonRequestCommand(_view, _schedules, _scenario,
                                                                            _requestPresenter,
                                                                            _handleBusinessRuleResponse,
                                                                            new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingRepo), string.Empty);

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }
            _mocks.VerifyAll();
        }
        [Test]
        public void VerifyBrokenRulesAndCancel()
        {
            _requestViewAdapters.RemoveAt(2);
            _requestViewAdapters.RemoveAt(1);

            IScheduleRange totalScheduleRange = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay daySchedule = _mocks.StrictMock<IScheduleDay>();

            _schedules = _mocks.StrictMock<IScheduleDictionary>();
			expectsForRuleViolationAndCancel(totalScheduleRange, daySchedule, daySchedule, false);
            var newBusinessRuleCollection = _mocks.StrictMock<INewBusinessRuleCollection>();
           
            _mocks.ReplayAll();

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }

            //Change status
            _requestPresenter.ApproveOrDeny(_requestViewAdapters,
                                            new ApprovePersonRequestCommand(_view, _schedules, _scenario,
                                                                            _requestPresenter,
                                                                            _handleBusinessRuleResponse,
                                                                            new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingRepo), string.Empty);

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }
            _mocks.VerifyAll();
        }
        [Test]
        public void VerifyCanOverrideRulesFromEarlier()
        {
            _requestPresenter = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());

            _requestViewAdapters.RemoveAt(2);
            _requestViewAdapters.RemoveAt(1);

            IScheduleRange totalScheduleRange = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay daySchedule = _mocks.StrictMock<IScheduleDay>();

            _schedules = _mocks.StrictMock<IScheduleDictionary>();
			expects(totalScheduleRange, daySchedule, daySchedule, false);

            var newBusinessRuleCollection = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
            
            _mocks.ReplayAll();

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsPending);
            }

            //Change status
            _requestPresenter.ApproveOrDeny(_requestViewAdapters,
                                            new ApprovePersonRequestCommand(_view, _schedules, _scenario,
                                                                            _requestPresenter,
                                                                            _handleBusinessRuleResponse,
                                                                            new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingRepo), string.Empty);

            foreach (PersonRequestViewModel adapter in _requestViewAdapters)
            {
                Assert.IsTrue(adapter.PersonRequest.IsApproved);
            }
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldNotCrashWhen_NullSubject()
		{
			_requestViewAdapters.First().PersonRequest.Subject = null;
            RequestPresenter.InitializeFileteredPersonDictionary(filteredPersons);
            RequestPresenter.FilterAdapters(_requestViewAdapters, new List<string> {"StringThatDoesNotExistInLists"});
		}

        #region Undo/redo tests
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyUndoRedo()
        {
            var newBusinessRuleCollection = _mocks.StrictMock<INewBusinessRuleCollection>();
            
            ITeam team = TeamFactory.CreateSimpleTeam();
            _person1.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1900, 1, 1), PersonContractFactory.CreatePersonContract(), team));
            
            UndoRedoContainer undo = new UndoRedoContainer(100);
            IScheduleDictionary sched = new ScheduleWithBusinessRuleDictionary(_scenario, new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2100, 1, 1)));
            sched.SetUndoRedoContainer(undo);
            IRequest part = new AbsenceRequest(AbsenceFactory.CreateAbsence("asdf"),
                                                   new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            IPersonRequest req = new PersonRequest(_person1);
            req.Request = part;
            req.Pending();
            PersonRequestViewModel adp = new PersonRequestViewModel(req, _shiftTradeRequestStatusChecker, null, null, _TimeZoneInfo);

            _requestPresenter.SetUndoRedoContainer(undo);
            _requestPresenter.ApproveOrDeny(new List<PersonRequestViewModel> {adp},
                                            new ApprovePersonRequestCommand(_view, sched, _scenario, _requestPresenter,
                                                                            _handleBusinessRuleResponse,
                                                                            new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingRepo), string.Empty);
            Assert.IsTrue(req.IsApproved);
            Assert.AreEqual(1, sched[_person1].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Count);

            undo.Undo();
            Assert.IsTrue(req.IsPending);
            Assert.AreEqual(0, sched[_person1].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Count);

            undo.Redo();
            Assert.IsTrue(req.IsApproved);
            Assert.AreEqual(1, sched[_person1].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyNoUndoRedoEntryIfNotApproved()
        {
            var newBusinessRuleCollection = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
            Expect.Call(()=>_view.ShowErrorMessage("","")).IgnoreArguments();

            var absence = _mocks.StrictMock<IAbsence>();

            _mocks.ReplayAll();
            IPerson person = PersonFactory.CreatePerson();
            person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1900, 1, 1)));

            UndoRedoContainer undo = new UndoRedoContainer(100);
            ScheduleWithBusinessRuleDictionary sched = new ScheduleWithBusinessRuleDictionary(_scenario, new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2100, 1, 1)));
            sched.SetUndoRedoContainer(undo);
            sched.AddBusinessRule(new BusinessRuleResponse(typeof(string), string.Empty, false, true, new DateTimePeriod(2000, 1, 1, 2001, 1, 1), person, new DateOnlyPeriod(new DateOnly(),new DateOnly() )));
            IRequest part = new AbsenceRequest(absence, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
            IPersonRequest req = new PersonRequest(person);
            req.Request = part;
            req.Pending();
            PersonRequestViewModel adp = new PersonRequestViewModel(req, _shiftTradeRequestStatusChecker, null, null, _TimeZoneInfo);

            _requestPresenter.SetUndoRedoContainer(undo);
            _requestPresenter.ApproveOrDeny(new List<PersonRequestViewModel> {adp},
                                            new ApprovePersonRequestCommand(_view, sched, _scenario, _requestPresenter,
                                                                            _handleBusinessRuleResponse,
                                                                            new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new ResourceCalculationOnlyScheduleDayChangeCallback(), _globalSettingRepo), string.Empty);

            Assert.IsTrue(req.IsPending);
            Assert.AreEqual(0, sched[person].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Count);
            Assert.IsFalse(undo.CanUndo());
            _mocks.VerifyAll();
        }
        #endregion


		private void expects(IScheduleRange totalScheduleRange, IScheduleDay dayScheduleForAbsenceReqStart, IScheduleDay dayScheduleForAbsenceReqEnd, bool mandatory)
        {
            Expect.Call(_schedules[_person1]).Return(totalScheduleRange);

			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqStart);
			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqEnd);
			Expect.Call(dayScheduleForAbsenceReqStart.FullAccess).Return(true);
			dayScheduleForAbsenceReqStart.Add(null);
            LastCall.IgnoreArguments();

            IList<IBusinessRuleResponse> brokenRules = new List<IBusinessRuleResponse>();
            DateTime start = DateTime.UtcNow.AddDays(2).Date;
            brokenRules.Add(new BusinessRuleResponse(typeof(DayOffRule), "Error", true, mandatory, new DateTimePeriod(start, start), _person1, new DateOnlyPeriod()));

            Expect.Call(_schedules.Modify(ScheduleModifier.Request, (IScheduleDay) null, null, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance))).Return(
                brokenRules).IgnoreArguments().Repeat.Twice();
            _handleBusinessRuleResponse.SetResponse(brokenRules);
            Expect.Call(_handleBusinessRuleResponse.DialogResult).Return(DialogResult.OK);

            // second call to personRequest.Approve
            Expect.Call(_schedules[_person1]).Return(totalScheduleRange);
			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqStart);
			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqEnd);
			Expect.Call(dayScheduleForAbsenceReqStart.FullAccess).Return(true);
			dayScheduleForAbsenceReqStart.Add(null);
            LastCall.IgnoreArguments();
            Expect.Call(_schedules.Modify(ScheduleModifier.Request, (IScheduleDay)null, null, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance))).Return(
                new List<IBusinessRuleResponse>()).IgnoreArguments();
            //Expect.Call(_handleBusinessRuleResponse.ApplyToAll).Return(true);
        }

		private void expectsForMandatoryRuleViolation(IScheduleRange totalScheduleRange, IScheduleDay dayScheduleForAbsenceReqStart, IScheduleDay dayScheduleForAbsenceReqEnd, bool mandatory)
        {
            Expect.Call(_schedules[_person1]).Return(totalScheduleRange);

			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqStart);
			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqEnd);
			Expect.Call(dayScheduleForAbsenceReqStart.FullAccess).Return(true);
			dayScheduleForAbsenceReqStart.Add(null);
            LastCall.IgnoreArguments();

            IList<IBusinessRuleResponse> brokenRules = new List<IBusinessRuleResponse>();
            DateTime start = DateTime.UtcNow.AddDays(2).Date;
            brokenRules.Add(new BusinessRuleResponse(typeof(DayOffRule), "Error", true, mandatory, new DateTimePeriod(start, start), _person1, new DateOnlyPeriod()));

            Expect.Call(_schedules.Modify(ScheduleModifier.Request, (IScheduleDay)null, null, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance))).Return(
                brokenRules).IgnoreArguments().Repeat.Twice();

            _view.ShowErrorMessage("MandatoryError", "Dont break this rule");
            LastCall.IgnoreArguments();
        }

        private void expectsForRuleViolationAndCancel(IScheduleRange totalScheduleRange, IScheduleDay dayScheduleForAbsenceReqStart, IScheduleDay dayScheduleForAbsenceReqEnd, bool mandatory)
        {
            Expect.Call(_schedules[_person1]).Return(totalScheduleRange);

			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqStart);
			Expect.Call(dayScheduleForAbsenceReqStart.FullAccess).Return(true);
			dayScheduleForAbsenceReqStart.Add(null);
			LastCall.IgnoreArguments();
			Expect.Call(totalScheduleRange.ScheduledDay(DateOnly.Today)).IgnoreArguments().Return(dayScheduleForAbsenceReqEnd);

            IList<IBusinessRuleResponse> brokenRules = new List<IBusinessRuleResponse>();
            DateTime start = DateTime.UtcNow.AddDays(2).Date;
            brokenRules.Add(new BusinessRuleResponse(typeof(DayOffRule), "Error", true, mandatory, new DateTimePeriod(start, start), _person1, new DateOnlyPeriod()));

            Expect.Call(_schedules.Modify(ScheduleModifier.Request, (IScheduleDay)null, null, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance))).Return(
                brokenRules).IgnoreArguments();
            Expect.Call(_schedules.Modify(ScheduleModifier.Request, (IScheduleDay)null, null, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance))).Return(
                brokenRules).IgnoreArguments();

            _handleBusinessRuleResponse.SetResponse(brokenRules);
            Expect.Call(_handleBusinessRuleResponse.DialogResult).Return(DialogResult.Cancel);
        }
    }
}
