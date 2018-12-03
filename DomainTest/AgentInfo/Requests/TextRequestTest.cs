using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	public class TextRequestTest
    {
        private DateTimePeriod _period;
        private TextRequest target;
        private string _description;

        [SetUp]
        public void Setup()
        {
            _period = new DateTimePeriod(new DateTime(2008, 7, 16, 0, 0, 0, DateTimeKind.Utc),
                                         new DateTime(2008, 7, 19, 0, 0, 0, DateTimeKind.Utc));

	        var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			target = (TextRequest) new PersonRequest(person, new TextRequest(_period)).Request;
            _description = "Text";
            target.RequestTypeDescription = _description;
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyGetDetails()
        {
            string text = target.GetDetails(new CultureInfo("en-US"));
            Assert.AreEqual(UserTexts.Resources.TextRequest + ", 2:00 AM - 2:00 AM", text);
        }

        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_period, target.Period);
            Assert.AreEqual(_description, target.RequestTypeDescription);

            Assert.AreEqual(new Description().Name, target.RequestPayloadDescription.Name); 
        }

        [Test]
        public void VerifyDenySetsTextForNotification()
        {
            target.Deny(null);
            Assert.IsNotNull(target);
            Assert.AreEqual("TextRequestHasBeenDeniedDot", target.TextForNotification);
        }

        [Test]
        public void VerifyApproveWorks()
        {
            MockRepository mocks = new MockRepository();
            IPerson person = PersonFactory.CreatePerson();
            IRequestApprovalService approvalService = mocks.StrictMock<IRequestApprovalService>();
            IPersonRequestCheckAuthorization authorization = mocks.StrictMock<IPersonRequestCheckAuthorization>();

            Expect.Call(()=>authorization.VerifyEditRequestPermission(null)).IgnoreArguments();

            mocks.ReplayAll();
            PersonRequest personRequest = new PersonRequest(person, target);
            personRequest.Pending();
            personRequest.Approve(approvalService,authorization);
            Assert.AreEqual("TextRequestHasBeenApprovedDot", target.TextForNotification);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyNotifyMessageDefaults()
        {
            Assert.AreEqual(target.TextForNotification, string.Empty);
            Assert.IsFalse(target.ShouldNotifyWithMessage);
            Assert.AreEqual(1, target.ReceiversForNotification.Count);
            Assert.IsTrue(target.ReceiversForNotification.Contains(target.Person));

        }

        [Test]
        public void GetRequestType()
        {
            Assert.IsNotNull(target.RequestType);
        }
    }
}
