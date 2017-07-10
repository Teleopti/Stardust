using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	public class AbsenceRequestTest
    {
        private DateTimePeriod _period;
        private Absence _absence;
        private AbsenceRequest _target;
        private RequestPartForTest _obj;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
                                         new DateTime(2008, 7, 19, 0, 0, 0, DateTimeKind.Utc));
            _absence = new Absence();
            _absence.Description = new Description("Holiday","861");

            _target = new AbsenceRequest(_absence,_period);
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_period,_target.Period);
            Assert.AreEqual(_absence,_target.Absence);

            _target.RequestTypeDescription = "Absence";
            Assert.AreEqual("Absence", _target.RequestTypeDescription);

            Assert.AreEqual(_absence.Description, _target.RequestPayloadDescription);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "personRequest"), Test]
        public void VerifyDenySetsTextForNotificationIfMultipleDaysAbsence()
        {
            IPerson person = PersonFactory.CreatePerson();
            PersonRequest personRequest = new PersonRequest(person, _target);
            _target.Deny(null);
            //var datePattern = person.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;

            //var culture = person.PermissionInformation.UICulture();
            //var timeZone = person.PermissionInformation.DefaultTimeZone();

            //var notificationMessage = string.Format(culture, UserTexts.Resources.ResourceManager.GetString("AbsenceRequestHasBeenDeniedDot", culture),
            //           personRequest.Request.Period.StartDateTimeLocal(timeZone).Date.ToString(
            //                                            culture.DateTimeFormat.ShortDatePattern, culture),
            //                                        personRequest.Request.Period.EndDateTimeLocal(timeZone).Date.ToString(
            //                                            culture.DateTimeFormat.ShortDatePattern, culture));

            //var notificationMessage = string.Format(person.PermissionInformation.UICulture(),
            //                                        UserTexts.Resources.AbsenceRequestHasBeenDeniedDot,
            //                                        personRequest.Request.Period.StartDateTimeLocal(
            //                                            person.PermissionInformation.DefaultTimeZone()).ToString(
            //                                                datePattern),
            //                                        personRequest.Request.Period.EndDateTimeLocal(
            //                                            person.PermissionInformation.DefaultTimeZone()).ToString(
            //                                                datePattern));
            Assert.IsNotEmpty(_target.TextForNotification);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "personRequest"), Test]
        public void VerifyDenySetsTextForNotificationIfOneDayAbsence()
        {
           var period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
                                         new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc));
            var absence = new Absence();
            absence.Description = new Description("Holiday", "861");

            var target = new AbsenceRequest(absence, period);
            
            IPerson person = PersonFactory.CreatePerson();
            person.SetId(Guid.Empty);
            var personRequest = new PersonRequest(person, target);
            target.Deny(null);
            Assert.IsNotEmpty(target.TextForNotification);
        }



        [Test]
        public void VerifyApproveAbsenceCallWorks()
        {
            MockRepository mocks = new MockRepository();
	        var service = createAbsenceRequestApproveService();
			IPersonRequestCheckAuthorization authorization = mocks.StrictMock<IPersonRequestCheckAuthorization>();

			Expect.Call(() => authorization.VerifyEditRequestPermission(null)).IgnoreArguments();

            mocks.ReplayAll();

			var person = PersonFactory.CreatePerson();
			PersonRequest personRequest = new PersonRequest(person, _target);
			personRequest.Pending();

			IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(service, authorization);
            Assert.AreEqual(0, brokenRules.Count);
           
            Assert.IsNotEmpty(_target.TextForNotification);
            
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyApproveOneDayAbsenceCallWorks()
        {
            var absence = new Absence();
            absence.Description = new Description("Holiday", "861");

            var mocks = new MockRepository();
	        var requestApprovalService = createAbsenceRequestApproveService();
			var authorization = mocks.StrictMock<IPersonRequestCheckAuthorization>();
            var period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
                                        new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc));
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.Empty);
            var target = new AbsenceRequest(absence, period);
			PersonRequest personRequest = new PersonRequest(person, target);

			Expect.Call(() => authorization.VerifyEditRequestPermission(null)).IgnoreArguments();

            mocks.ReplayAll();
            
            personRequest.Pending();

            IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(requestApprovalService, authorization);
            Assert.AreEqual(0, brokenRules.Count);
			Assert.That(target.TextForNotification, Is.Not.Null.Or.Empty);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyApproveAbsenceCallDoesNotSendNotificationWhenFailing()
        {
			var mocks = new MockRepository();
            var requestApprovalService = mocks.StrictMock<IRequestApprovalService>();
            var authorization = mocks.StrictMock<IPersonRequestCheckAuthorization>();
            var person = PersonFactory.CreatePerson();

            Expect.Call(requestApprovalService.Approve(null)).Return(new List<IBusinessRuleResponse>{null}).IgnoreArguments();
            Expect.Call(() => authorization.VerifyEditRequestPermission(null)).IgnoreArguments();

            mocks.ReplayAll();

            PersonRequest personRequest = new PersonRequest(person, _target);
            personRequest.Pending();

            IList<IBusinessRuleResponse> brokenRules = personRequest.Approve(requestApprovalService, authorization);
            Assert.AreEqual(1, brokenRules.Count);
            Assert.IsTrue(string.IsNullOrEmpty(_target.TextForNotification));

            mocks.VerifyAll();
        }

        [Test]
        public void VerifyClone()
        {
            IAbsenceRequest entityClone = (IAbsenceRequest)_target.EntityClone();

            Assert.AreEqual(_target.Person, entityClone.Person);
            Assert.AreEqual(_target.Id, entityClone.Id);
            Assert.AreEqual(_target.Absence, entityClone.Absence);
            Assert.AreEqual(_target.Period, entityClone.Period);
            Assert.AreEqual(_target.RequestTypeDescription, entityClone.RequestTypeDescription);

            IAbsenceRequest clone = (IAbsenceRequest)_target.Clone();
            Assert.AreNotEqual(_target, clone);

        }

        [Test]
        public void VerifyPersonReturnsNullWithoutParent()
        {
            _obj = new RequestPartForTest();
            Assert.IsNull(_obj.GetPerson());
        }

        [Test]
        public void VerifyCanGetDetails()
        {
	        var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
	        var absenceRequest = new PersonRequest(person, new AbsenceRequest(new Absence {Description = new Description("The Absence")}, _period)).Request;
            string text = absenceRequest.GetDetails(new CultureInfo("en-US"));

            Assert.AreEqual("The Absence, 2:00 AM - 2:00 AM", text);
            text = absenceRequest.GetDetails(new CultureInfo("ko-KR"));
            Assert.AreEqual("The Absence, 오전 2:00 - 오전 2:00", text);
            text = absenceRequest.GetDetails(new CultureInfo("zh-TW"));
            Assert.AreEqual("The Absence, 上午 02:00 - 上午 02:00", text);

	        var obj2 =
		        new PersonRequest(person,
			        new AbsenceRequest(new Absence {Description = new Description("The other Absence")},
				        new DateTimePeriod(new DateTime(2009, 12, 10, 0, 0, 0, DateTimeKind.Utc),
					        new DateTime(2009, 12, 10, 23, 59, 59, DateTimeKind.Utc)))).Request;
            string otertext = obj2.GetDetails(new CultureInfo("es-ES"));
            Assert.AreEqual("The other Absence", otertext);
        }

        [Test]
        public void VerifyTheOnlyOneThatShouldGetNotifiedIsThePersonInvolvedInTheRequest()
        {
            Assert.IsTrue(_target.ReceiversForNotification.Contains(_target.Person));
            Assert.AreEqual(1, _target.ReceiversForNotification.Count);

        }

	    private AbsenceRequestApprovalService createAbsenceRequestApproveService()
	    {
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var dateTimePeriod = new DateTimePeriod(2010, 1, 1, 2010, 1, 2);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var businessRules = new FakeNewBusinessRuleCollection();
			businessRules.SetRuleResponse(new List<IBusinessRuleResponse> { new BusinessRuleResponse(typeof(BusinessRuleResponse), "warning", true, false, dateTimePeriod, person, new DateOnlyPeriod(2010, 1, 1, 2010, 1, 2), "test warning") });
			var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			return new AbsenceRequestApprovalService(scenario, scheduleDictionary, businessRules, scheduleDayChangeCallback, globalSettingDataRepository, new CheckingPersonalAccountDaysProvider(personAbsenceAccountRepository));
		}

		private class RequestPartForTest : Request
        {
            public IPerson GetPerson()
            {
                return Person;
            }


            public override void Deny(IPerson denyPerson)
            {
                throw new NotImplementedException();
            }

	        public override void Cancel()
	        {
		        throw new NotImplementedException();
	        }

            public override string GetDetails(CultureInfo cultureInfo)
            {
                throw new NotImplementedException();
            }

            protected override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
            {
                throw new NotImplementedException();
            }

            public override string RequestTypeDescription
            {
                get { return RequestTypeDescription; }
                set { throw new NotImplementedException(); }
            }

        	public override RequestType RequestType
        	{
        		get { throw new NotImplementedException(); }
        	}

        	public override Description RequestPayloadDescription
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
