using System;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class PersonRequestDtoTest
    {
        private PersonRequestDto _target;
        private PersonDto _person;
        private Guid _id;
        private AbsenceRequestDto _absenceRequest;
        private DateOnlyDto _date;
        private string _subject;
        private string _message;

        [SetUp]
        public void Setup()
        {
            _person = new PersonDto();
            _id = Guid.NewGuid();
			_date = new DateOnlyDto { DateTime = new DateTime(2008, 01, 01) };
            _absenceRequest = new AbsenceRequestDto();
            _message = "message";
            _subject = "subject";

            _target = new PersonRequestDto
                          {
                              CreatedDate = _date.DateTime,
                              Id = _id,
                              Person = _person,
                              Message = _message,
                              Request = _absenceRequest,
                              RequestedDate = _date.DateTime,
                              RequestedDateLocal = _date.DateTime,
                              RequestStatus = RequestStatusDto.Denied,
                              Subject = _subject,
                              IsDeleted = false
                          };
        }


        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_id, _target.Id);
            Assert.AreEqual(_person,_target.Person);
            Assert.AreEqual(_subject,_target.Subject);
            Assert.AreEqual(_message,_target.Message);
            Assert.AreEqual(RequestStatusDto.Denied,_target.RequestStatus);
            Assert.AreEqual(_date.DateTime, _target.RequestedDate);
            Assert.AreEqual(_date.DateTime, _target.CreatedDate);
            Assert.AreEqual(_date.DateTime,_target.RequestedDateLocal);
            Assert.AreEqual(_absenceRequest,_target.Request);
            Assert.IsFalse(_target.IsDeleted);
        }
    }
}