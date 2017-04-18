using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.TestCommon;
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

        [Test]
        public void ShouldThrowIfRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target = new InfoWindowTextFormatter(null, _unitOfWorkFactory));
        }

        [Test]
        public void ShouldThrowIfFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target = new InfoWindowTextFormatter(_settingDataRepository, null));
        }

        [Test]
        public void ShouldReturnEmptyStringIfMeetingIsNullL()
        {
            Assert.That(_target.GetInfoText(null),Is.EqualTo(string.Empty));
        }

        [Test]
        public void ShouldReassociateEveryPersonToUnitOfWork()
        {
        	var meeting = MockRepository.GenerateMock<IMeeting>();
        	var meetingPerson = MockRepository.GenerateMock<IMeetingPerson>();
			var person = new Person().WithName(new Name("goran", "person"));
        	var uow = MockRepository.GenerateMock<IUnitOfWork>();
        	var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var commonNameDescription = new CommonNameDescriptionSetting();
        	var settingDataRepository = MockRepository.GenerateMock<ISettingDataRepository>();

        	settingDataRepository.Stub(x => x.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting())).
        		Return(commonNameDescription).IgnoreArguments();
			meetingPerson.Stub(x => x.Person).Return(person);
        	meeting.Stub(x => x.Organizer).Return(person);
        	meeting.Stub(x => x.MeetingPersons).Return(
        		new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> {meetingPerson}));
        	uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);

			var target = new InfoWindowTextFormatter(settingDataRepository, uowFactory);
			
			target.GetInfoText(meeting);
			uow.AssertWasCalled(x => x.Reassociate(person), options => options.Repeat.Times(2));


			/*_mocks.BackToRecordAll();
            var meeting = _mocks.StrictMock<IMeeting>();
            var meetingPerson = _mocks.StrictMock<IMeetingPerson>();
            var person = new Person {Name = new Name("goran", "person")};

            var uow = _mocks.StrictMock<IUnitOfWork>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(() => uow.Reassociate(person)).Repeat.Twice();
            Expect.Call(meeting.Organizer).Return(person);
			Expect.Call(meeting.GetSubject(new NoFormatting())).Return("subject");
			Expect.Call(meeting.GetLocation(new NoFormatting())).Return("lokalen");
            Expect.Call(meeting.MeetingPersons).Return(
                new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> {meetingPerson}));
            Expect.Call(meetingPerson.Person).Return(person);
			Expect.Call(meeting.GetDescription(new NoFormatting())).Return("we'll meet again, don't know where, don't know when");
            Expect.Call( meeting.UpdatedOn).Return(new DateTime(2011, 3, 25, 14, 45, 0)).Repeat.Twice();
            Expect.Call(uow.Dispose);
            _mocks.ReplayAll();
            _target.GetInfoText(meeting);
            _mocks.VerifyAll();*/
        }   
    }

    
}