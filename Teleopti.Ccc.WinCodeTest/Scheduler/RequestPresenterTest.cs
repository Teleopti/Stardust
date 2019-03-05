using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture, SetUICulture("en-US")]
	public class RequestPresenterTest
	{
		private IRequestPresenter _requestPresenter;
		private FakeBusinessRulesResponseHandler _handleBusinessRuleResponse;
		private IList<PersonRequestViewModel> _requestViewAdapters;
		private PersonRequestViewModel _request1;
		private PersonRequestViewModel _request2;
		private PersonRequestViewModel _request3;
		private IScheduleDictionary _schedules;
		private IScheduleDateTimePeriod _scheduleDateTimePeriod;
		private DateTimePeriod _dateTimePeriod;
		private IScenario _scenario;
		private IViewBase _view;

		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
		private ITimeZoneGuard _timeZoneGuard;
		private IDictionary<Guid, IPerson> filteredPersons;
		private IGlobalSettingDataRepository _globalSettingRepo;
		private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private FullPermission _permission;

		[SetUp]
		public void Setup()
		{
			_timeZoneGuard = new FakeTimeZoneGuard();
			_requestViewAdapters = new List<PersonRequestViewModel>();
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_permission = new FullPermission();
			
			DateTime startDateTime = DateTime.SpecifyKind(new DateTime(2010, 1, 1), DateTimeKind.Utc);
			DateTime endDateTime = startDateTime.AddHours(2);

			_dateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			_scheduleDateTimePeriod = new ScheduleDateTimePeriod(_dateTimePeriod);
			_schedules = new ScheduleDictionary(_scenario, _scheduleDateTimePeriod,
				new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
				CurrentAuthorization.Make());
			_view = MockRepository.GenerateMock<IViewBase>();
			_handleBusinessRuleResponse = new FakeBusinessRulesResponseHandler();
			_requestPresenter = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			_shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			_globalSettingRepo = new FakeGlobalSettingDataRepository();
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();

			_person1 = PersonFactory.CreatePerson("A", "B");
			_person1.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
			_person2 = PersonFactory.CreatePerson("A", "A");
			_person2.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
			_person3 = PersonFactory.CreatePerson("A", "B");
			_person3.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);

			IAbsence absence = AbsenceFactory.CreateAbsence("abs");
			
			PersonRequest personRequest1 = new PersonRequest(_person1);
			personRequest1.TrySetMessage("b");
			personRequest1.Subject = "Absence Request1";
			personRequest1.Request = new AbsenceRequest(absence, _dateTimePeriod);
			personRequest1.Pending();

			_request1 = new PersonRequestViewModel(personRequest1, _shiftTradeRequestStatusChecker, null, null, _timeZoneInfo);
			
			PersonRequest personRequest2 = new PersonRequest(_person2);
			personRequest2.TrySetMessage("b");
			personRequest2.Subject = "Absence Request2";
			personRequest2.Request = new AbsenceRequest(absence, _dateTimePeriod);
			personRequest2.Pending();
			_request2 = new PersonRequestViewModel(personRequest2, _shiftTradeRequestStatusChecker, null, null, _timeZoneInfo);
			
			PersonRequest personRequest3 = new PersonRequest(_person3);
			personRequest3.TrySetMessage("a");
			personRequest3.Subject = "Absence Request3";
			personRequest3.Request = new AbsenceRequest(absence, _dateTimePeriod);
			personRequest3.Pending();
			_request3 = new PersonRequestViewModel(personRequest3, _shiftTradeRequestStatusChecker, null, null, _timeZoneInfo);

			_requestViewAdapters.Add(_request1);
			_requestViewAdapters.Add(_request2);
			_requestViewAdapters.Add(_request3);


			filteredPersons = new Dictionary<Guid, IPerson>
			{
				{_request1.PersonRequest.Person.Id.GetValueOrDefault(), _request1.PersonRequest.Person}
			};
		}

		[Test]
		public void VerifyFilterAdapters()
		{
			var filterExpression = new List<string>();
			
			var target=  new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			var list = target.FilterAdapters(_requestViewAdapters, filterExpression);
			Assert.AreEqual(3, list.Count);

			list = target.FilterAdapters(_requestViewAdapters, filterExpression);
			Assert.AreEqual(3, list.Count);

		}

		[Test]
		public void VerifyThatPersonNameFound()
		{
			var filterExpression = new List<string>();
			filterExpression.Add("A");

			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			var list = target.FilterAdapters(_requestViewAdapters, filterExpression);
			Assert.AreEqual(list.Count, 3);
		}

		[Test]
		public void ShouldNotReturnAnyRequestIfNoTextFound()
		{
			var filerExpression = new List<string>();
			filerExpression.Add("NOT FOUND");

			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			var list = target.FilterAdapters(_requestViewAdapters, filerExpression);
			Assert.AreEqual(list.Count, 0);
		}
		
		[Test]
		public void ShouldReturnThoseRequestThatRequestedInCurrentYear()
		{
			var filerExpression = new List<string>();
			const string year = "2010";
			filerExpression.Add(year);

			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			var list = target.FilterAdapters(_requestViewAdapters, filerExpression);
			Assert.AreEqual(list.Count, 3);
		}

		[Test]
		public void ShouldOnlyShowFilteredPersons()
		{
			foreach (var request in _requestViewAdapters)
			{
				request.PersonRequest.Person.SetId(Guid.NewGuid());
			}

			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			var result = target.FilterAdapters(_requestViewAdapters,
											new List<Guid> {_request1.PersonRequest.Person.Id.GetValueOrDefault()});
			result.Count.Should().Be.EqualTo(1);
			result.First().Should().Be.EqualTo(_request1);
		}

		
		[Test]
		public void ShouldReturnRequestsWhichContainTextToBeSearched()
		{
			var filerExpression = new List<string>();
			filerExpression.Add("Request2");

			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			var list = target.FilterAdapters(_requestViewAdapters, filerExpression);
			Assert.AreEqual(list.Count, 1);
		}

		[Test]
		public void ShouldReturnRequestsFromtheCurrentMonth()
		{
			var filerExpression = new List<string>();
			const string month = "1";
			filerExpression.Add(month);

			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			var list = target.FilterAdapters(_requestViewAdapters, filerExpression);
			Assert.AreEqual(list.Count, 3);
		}

		[Test]
		public void ShouldReturnRequestsOfTypeAbsence()
		{
			var filerExpression = new List<string>();
			filerExpression.Add("Absence");
			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			var list = target.FilterAdapters(_requestViewAdapters, filerExpression);
			Assert.AreEqual(list.Count, 3);
		}

		[Test]
		public void ShouldReturnRequestWhichContainsSearchedAbsenceName()
		{
			var filerExpression = new List<string>();
			filerExpression.Add("abs");
			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			var list = target.FilterAdapters(_requestViewAdapters, filerExpression);
			Assert.AreEqual(list.Count, 3);
		}

		[Test]
		public void VerifyCanApproveStatusOnRequest()
		{
			var newBusinessRuleCollection = new FakeNewBusinessRuleCollection();
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
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack()
																				, _globalSettingRepo, _personAbsenceAccountRepository, new FakePersonRequestRepository(), _timeZoneGuard), string.Empty);

			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsTrue(adapter.IsApproved);
				Assert.IsFalse(adapter.IsDenied);
				Assert.IsFalse(adapter.IsPending);
				Assert.IsFalse(adapter.IsNew);
			}
		}

		[Test]
		public void VerifyCanDenyStatusOnRequest()
		{
			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsTrue(adapter.PersonRequest.IsPending);
			}

			//Change status
			_requestPresenter.ApproveOrDeny(_requestViewAdapters, new DenyPersonRequestCommand(_requestPresenter, new PersonRequestAuthorizationCheckerForTest(), _scenario, null), string.Empty);

			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsTrue(adapter.PersonRequest.IsDenied);
			}
		}

		[Test]
		public void VerifyCanNotDenyStatusOnRequestWhenHasNoPermissionToModifyRestrictedScenario()
		{
			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsTrue(adapter.PersonRequest.IsPending);
			}

			var scenario = ScenarioFactory.CreateScenarioWithId("test scenario", true);
			scenario.Restricted = true;

			_permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);
			CurrentAuthorization.ThreadlyUse(_permission);

			_requestPresenter.ApproveOrDeny(_requestViewAdapters, new DenyPersonRequestCommand(_requestPresenter, new PersonRequestAuthorizationCheckerForTest(), scenario, null), string.Empty);

			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsFalse(adapter.PersonRequest.IsDenied);
			}
		}

		[Test]
		public void VerifyReplyOnRequest()
		{
			var undo = new UndoRedoContainer();
			_requestPresenter.SetUndoRedoContainer(undo);

			_requestPresenter.Reply(_requestViewAdapters, "Testin'");

			Assert.AreEqual("b\r\nTestin'", _requestViewAdapters[0].GetMessage(new NoFormatting()));
			Assert.AreEqual("b\r\nTestin'", _requestViewAdapters[1].GetMessage(new NoFormatting()));
			Assert.AreEqual("a\r\nTestin'", _requestViewAdapters[2].GetMessage(new NoFormatting()));
		}

		[Test]
		public void VerifyBrokenRulesOverridden()
		{
			_requestViewAdapters.RemoveAt(2);
			_requestViewAdapters.RemoveAt(1);
			
			_schedules = new ScheduleDictionaryForTest(_scenario, _dateTimePeriod);
			_handleBusinessRuleResponse.WithDialogResult(DialogResult.OK);
			
			var newBusinessRuleCollection = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
			
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
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), _globalSettingRepo, _personAbsenceAccountRepository,new FakePersonRequestRepository(), _timeZoneGuard), string.Empty);

			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsTrue(adapter.PersonRequest.IsApproved);
			}
		}

		[Test]
		public void VerifyMandatoryBrokenRules()
		{
			_requestViewAdapters.RemoveAt(2);
			_requestViewAdapters.RemoveAt(1);


			_schedules = new ScheduleDictionaryForTest(_scenario, _dateTimePeriod);
			var newBusinessRuleCollection = new FakeNewBusinessRuleCollection();
			newBusinessRuleCollection.SetRuleResponse(new[]
			{
				new BusinessRuleResponse(typeof(DayOffRule), "Error", true, true, _dateTimePeriod,
					_person1, new DateOnlyPeriod(), "tjillevippen")
			});
			
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
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), _globalSettingRepo, _personAbsenceAccountRepository, new FakePersonRequestRepository(), _timeZoneGuard), string.Empty);

			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsTrue(adapter.PersonRequest.IsPending);
			}

			_view.AssertWasCalled(x => x.ShowErrorMessage("MandatoryError", "Dont break this rule"), o => o.IgnoreArguments());
		}

		[Test]
		public void VerifyBrokenRulesAndCancel()
		{
				_requestViewAdapters.RemoveAt(2);
				_requestViewAdapters.RemoveAt(1);

				_schedules = new ScheduleDictionaryForTest(_scenario, _dateTimePeriod);
				var newBusinessRuleCollection = new FakeNewBusinessRuleCollection();
				newBusinessRuleCollection.SetRuleResponse(new[]
				{
					new BusinessRuleResponse(typeof(DayOffRule), "Error", true, false, _dateTimePeriod,
						_person1, new DateOnlyPeriod(), "tjillevippen")
				});
				_handleBusinessRuleResponse.WithDialogResult(DialogResult.Cancel);

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
							(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(),
						new DoNothingScheduleDayChangeCallBack(), _globalSettingRepo, _personAbsenceAccountRepository,
						new FakePersonRequestRepository(), _timeZoneGuard), string.Empty);

				foreach (PersonRequestViewModel adapter in _requestViewAdapters)
				{
					Assert.IsTrue(adapter.PersonRequest.IsPending);
				}
		}

		[Test]
		public void VerifyCanOverrideRulesFromEarlier()
		{
			_requestPresenter = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());

			_requestViewAdapters.RemoveAt(2);
			_requestViewAdapters.RemoveAt(1);

			_schedules = new ScheduleDictionaryForTest(_scenario, _dateTimePeriod);
			
			_handleBusinessRuleResponse.WithDialogResult(DialogResult.OK);
			var newBusinessRuleCollection = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
			
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
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), _globalSettingRepo, _personAbsenceAccountRepository, new FakePersonRequestRepository(), _timeZoneGuard), string.Empty);

			foreach (PersonRequestViewModel adapter in _requestViewAdapters)
			{
				Assert.IsTrue(adapter.PersonRequest.IsApproved);
			}
		}

		[Test]
		public void ShouldNotCrashWhen_NullSubject()
		{
			_requestViewAdapters.First().PersonRequest.Subject = null;
			var target = new RequestPresenter(new PersonRequestAuthorizationCheckerForTest());
			target.InitializeFileteredPersonDictionary(filteredPersons);
			target.FilterAdapters(_requestViewAdapters, new List<string> {"StringThatDoesNotExistInLists"});
		}
		
		[Test]
		public void VerifyUndoRedo()
		{
			var newBusinessRuleCollection = new FakeNewBusinessRuleCollection();

			ITeam team = TeamFactory.CreateSimpleTeam();
			_person1.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1900, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			
			UndoRedoContainer undo = new UndoRedoContainer();
			IScheduleDictionary sched = new ScheduleWithBusinessRuleDictionary(_scenario, new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2100, 1, 1)));
			sched.SetUndoRedoContainer(undo);
			IRequest part = new AbsenceRequest(AbsenceFactory.CreateAbsence("asdf"),
												   new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			IPersonRequest req = new PersonRequest(_person1);
			req.Request = part;
			req.Pending();
			PersonRequestViewModel adp = new PersonRequestViewModel(req, _shiftTradeRequestStatusChecker, null, null, _timeZoneInfo);

			_requestPresenter.SetUndoRedoContainer(undo);
			_requestPresenter.ApproveOrDeny(new List<PersonRequestViewModel> {adp},
											new ApprovePersonRequestCommand(_view, sched, _scenario, _requestPresenter,
																			_handleBusinessRuleResponse,
																			new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), _globalSettingRepo, _personAbsenceAccountRepository, new FakePersonRequestRepository(), _timeZoneGuard), string.Empty);
			Assert.IsTrue(req.IsApproved);
			Assert.AreEqual(1, sched[_person1].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Length);

			undo.Undo();
			Assert.IsTrue(req.IsPending);
			Assert.AreEqual(0, sched[_person1].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Length);

			undo.Redo();
			Assert.IsTrue(req.IsApproved);
			Assert.AreEqual(1, sched[_person1].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Length);
		}

		[Test]
		public void VerifyNoUndoRedoEntryIfNotApproved()
		{
			var newBusinessRuleCollection = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
			var absence = new Absence();

			IPerson person = PersonFactory.CreatePerson();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1900, 1, 1)));

			UndoRedoContainer undo = new UndoRedoContainer();
			ScheduleWithBusinessRuleDictionary sched = new ScheduleWithBusinessRuleDictionary(_scenario, new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2100, 1, 1)));
			sched.SetUndoRedoContainer(undo);
			sched.AddBusinessRule(new BusinessRuleResponse(typeof(string), string.Empty, false, true, new DateTimePeriod(2000, 1, 1, 2001, 1, 1), person, new DateOnlyPeriod(new DateOnly(),new DateOnly() ), "tjillevippen"));
			IRequest part = new AbsenceRequest(absence, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			IPersonRequest req = new PersonRequest(person);
			req.Request = part;
			req.Pending();
			PersonRequestViewModel adp = new PersonRequestViewModel(req, _shiftTradeRequestStatusChecker, null, null, _timeZoneInfo);

			_requestPresenter.SetUndoRedoContainer(undo);
			_requestPresenter.ApproveOrDeny(new List<PersonRequestViewModel> {adp},
											new ApprovePersonRequestCommand(_view, sched, _scenario, _requestPresenter,
																			_handleBusinessRuleResponse,
																			new PersonRequestAuthorizationCheckerForTest
																				(), newBusinessRuleCollection, new OverriddenBusinessRulesHolder(), new DoNothingScheduleDayChangeCallBack(), _globalSettingRepo, _personAbsenceAccountRepository, new FakePersonRequestRepository(), _timeZoneGuard), string.Empty);

			Assert.IsTrue(req.IsPending);
			Assert.AreEqual(0, sched[person].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAbsenceCollection().Length);
			Assert.IsFalse(undo.CanUndo());

			_view.AssertWasCalled(x => x.ShowErrorMessage("MandatoryError", "Dont break this rule"),o => o.IgnoreArguments());
		}
	}
}
