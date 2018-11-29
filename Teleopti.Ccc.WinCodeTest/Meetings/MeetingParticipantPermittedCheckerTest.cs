using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingParticipantPermittedCheckerTest
    {
        private MockRepository _mocks;
        private MeetingParticipantPermittedChecker _target;
        private IList<IPerson> _persons;
        private DateOnly _dateOnly;
        private IViewBase _viewBase;
        private IAuthorization _authorization;
        private Person _person;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person = new Person().WithName(new Name("ola", "håkansson"));
            _persons = new List<IPerson>{_person};
            _dateOnly = new DateOnly(2011, 4, 3);
            _viewBase = _mocks.StrictMock<IViewBase>();
            _authorization = _mocks.StrictMock<IAuthorization>();
            _target = new MeetingParticipantPermittedChecker();
        }

        [Test]
        public void ShouldCheckPermissionOnEveryPerson()
        {
            Expect.Call(_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings, _dateOnly,_person)).Return(false);
            Expect.Call(() => _viewBase.ShowErrorMessage("", "")).IgnoreArguments();
            _mocks.ReplayAll();
            Assert.That(_target.ValidatePermittedPersons(_persons,_dateOnly, _viewBase,_authorization), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfPermitted()
        {
            Expect.Call(_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings, _dateOnly, _person)).Return(true);
            _mocks.ReplayAll();
            Assert.That(_target.ValidatePermittedPersons(_persons, _dateOnly, _viewBase, _authorization), Is.True);
            _mocks.VerifyAll();
        }
    }

}