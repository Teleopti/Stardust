using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class PersonRequestAuthorizationTest
    {
        private PersonRequestAuthorization _target;
        private MockRepository _mocks;
        private string _absenceRequest;
        private string _textRequest;
        private string _shiftTradeRequest;

        private IAuthorization _authorizationService;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _authorizationService = _mocks.StrictMock<IAuthorization>();
            _target = new PersonRequestAuthorization(_authorizationService);
            _absenceRequest = Resources.RequestTypeAbsence;
            _textRequest = Resources.RequestTypeText;
            _shiftTradeRequest = Resources.RequestTypeShiftTrade;

        }

        [Test]
        public void VerifyCanOpenWithViewRequestWithCorrectRights()
        {
            using(_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
                    .Return(true);
            }
            using(_mocks.Playback())
            {
                Assert.IsTrue(_target.IsPermittedRequestView());
            }
        }

        [Test]
        public void VerifyCannotOpenWithViewRequestWithoutRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
                    .Return(false);
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsPermittedRequestView());
            }
        }

        [Test]
        public void VerifyCanModifyAbsenceRequestWithCorrectRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(true);
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule))
                    .Return(true);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.IsPermittedRequestApprove(_absenceRequest));
            }
        }

        [Test]
        public void VerifyCannotModifyAbsenceRequestWithNoModifyScheduleRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(true).Repeat.Any();
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule))
                    .Return(false).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsPermittedRequestApprove(_absenceRequest));
            }
        }

        [Test]
        public void VerifyCannotModifyAbsenceRequestWithNoApproveRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(false).Repeat.Any();
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule))
                    .Return(true).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsPermittedRequestApprove(_absenceRequest));
            }
        }

        [Test]
        public void VerifyCanModifyShiftTradeRequestWithCorrectRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(true);
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule))
                    .Return(true);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.IsPermittedRequestApprove(_shiftTradeRequest));
            }
        }

        [Test]
        public void VerifyCannotModifyShiftTradeRequestWithNoModifyScheduleRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(true).Repeat.Any();
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule))
                    .Return(false).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsPermittedRequestApprove(_shiftTradeRequest));
            }
        }

        [Test]
        public void VerifyCannotModifyShiftTradeRequestWithNoApproveRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(false).Repeat.Any();
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule))
                    .Return(true).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsPermittedRequestApprove(_shiftTradeRequest));
            }
        }

        [Test]
        public void VerifyCanModifyTextRequestWithCorrectRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(true);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.IsPermittedRequestApprove(_textRequest));
            }
        }

        [Test]
        public void VerifyCannotModifyTextRequestWithNoApproveRights()
        {
            using (_mocks.Record())
            {
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                    .Return(false).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsPermittedRequestApprove(_textRequest));
            }
        }
    }
}
