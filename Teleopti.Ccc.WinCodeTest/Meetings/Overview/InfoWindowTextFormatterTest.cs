using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture]
    public class InfoWindowTextFormatterTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISettingDataRepository _settingDataRepository;
        private InfoWindowTextFormatter _target;
        private CommonNameDescriptionSetting _commonNameDescription;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _settingDataRepository = _mocks.StrictMock<ISettingDataRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _commonNameDescription = new CommonNameDescriptionSetting();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork());
            Expect.Call(_settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting())).Return(_commonNameDescription).IgnoreArguments();
            _mocks.ReplayAll();
            _target = new InfoWindowTextFormatter(_settingDataRepository, _unitOfWorkFactory);
            _mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfRepositoryIsNull()
        {
            _target = new InfoWindowTextFormatter(null, _unitOfWorkFactory);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfFactoryIsNull()
        {
            _target = new InfoWindowTextFormatter(_settingDataRepository, null);
        }

        [Test]
        public void ShouldReturnEmptyStringIfMeetingIsNullL()
        {
            Assert.That(_target.GetInfoText(null),Is.EqualTo(string.Empty));
        }

        [Test]
        public void ShouldReassociateEveryPersonToUnitOfWork()
        {
            _mocks.BackToRecordAll();
            var meeting = _mocks.StrictMock<IMeeting>();
            var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
            var person = new Person {Name = new Name("goran", "person")};

            var uow = _mocks.StrictMock<IUnitOfWork>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(() => uow.Reassociate(person)).Repeat.Twice();
            Expect.Call(meeting.Organizer).Return(person);
            Expect.Call(meeting.Subject).Return("subject");
            Expect.Call(meeting.Location).Return("lokalen");
            Expect.Call(meeting.MeetingPersons).Return(
                new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> {meetingPerson}));
            Expect.Call(meetingPerson.Person).Return(person);
            Expect.Call(meeting.Description).Return("we'll meet again, don't know where, don't know when");
            Expect.Call( meeting.UpdatedOn).Return(new DateTime(2011, 3, 25, 14, 45, 0)).Repeat.Twice();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.GetInfoText(meeting);
            _mocks.VerifyAll();
        }   
    }

    
}