using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.WinCode.Security;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Security
{
    [TestFixture]
    public class PasswordPolicyServiceViewModelTest
    {
        private PasswordPolicyServiceViewModel _target;
        private TesterForCommandModels _testerForCommandModels;
        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _target = new PasswordPolicyServiceViewModel();
            _testerForCommandModels = new TesterForCommandModels();
            _mockRepository = new MockRepository();
        }

        [Test]
        public void VerifyCreatesALoadServiceWithLoadString()
        {
           
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);

            _target.Path = "filethatDoesNotExist";
            Assert.IsFalse(_target.FilePathIsOk);

            Assert.IsTrue(listener.HasFired("Path"));
        }

        [Test]
        public void VerifyLoadFileCommand()
        {
            ILoadPasswordPolicyService service = _mockRepository.StrictMock<ILoadPasswordPolicyService>();
            _target=new PasswordPolicyServiceViewModel(service);
            using(_mockRepository.Record())
            {
                Expect.Call(() => service.Path = "test");
            }
            using (_mockRepository.Playback())
            {
                PropertyChangedListener listener = new PropertyChangedListener();
                listener.ListenTo(_target);
                _target.Path = "test";
                _testerForCommandModels.ExecuteCommandModel(_target.LoadFileCommand);
                Assert.IsTrue(listener.HasFired("InvalidAttemptWindow"));
                Assert.IsTrue(listener.HasFired("MaxAttemptCount"));
                Assert.IsTrue(listener.HasFired("PasswordValidForDayCount"));
                Assert.IsTrue(listener.HasFired("PasswordExpireWarningDayCount"));   
            }
            
        }

        [Test]
        public void VerifyExposedPropertiesFromModel()
        {
            TimeSpan invalidAttemptWindow = TimeSpan.FromMinutes(12);
            int maxAttemptCount = 3;
            int passwordValidForDayCount = 14;
            int passwordExpireWarningDayCount = 23;


            IPasswordPolicy policy = _mockRepository.StrictMock<IPasswordPolicy>();
            _target = new PasswordPolicyServiceViewModel(policy);
            using (_mockRepository.Record())
            {
                Expect.Call(policy.InvalidAttemptWindow).Return(invalidAttemptWindow);
                Expect.Call(policy.MaxAttemptCount).Return(maxAttemptCount);
                Expect.Call(policy.PasswordValidForDayCount).Return(passwordValidForDayCount);
                Expect.Call(policy.PasswordExpireWarningDayCount).Return(passwordExpireWarningDayCount);
            }
            using (_mockRepository.Playback())
            {
                Assert.AreEqual(invalidAttemptWindow,_target.InvalidAttemptWindow);
                Assert.AreEqual(maxAttemptCount, _target.MaxAttemptCount);
                Assert.AreEqual(passwordValidForDayCount, _target.PasswordValidForDayCount);
                Assert.AreEqual(passwordExpireWarningDayCount, _target.PasswordExpireWarningDayCount);
               
            }
        }

        [Test]
        public void VerifyCanLoadFile()
        {
            //If the path is a valid file, the CanLoadFile should be true.
            PropertyChangedListener listener = new PropertyChangedListener();

            _target.Path = "notValid";
            Assert.IsFalse(_target.FilePathIsOk);
            listener.ListenTo(_target);

            _target.Path = @"..\..\Security\ValidFile.xml";
            Assert.IsTrue(_target.FilePathIsOk);
            Assert.IsTrue(listener.HasFired("FilePathIsOk"));
        }

        [Test]
        public void ShouldChangePassword()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);

            _target.Password = "new password";

            Assert.AreEqual("new password",_target.Password);
            Assert.IsTrue(listener.HasFired("Password"));
        }


        [Test]
        public void VerifyNewPasswordCanSetAndValidatedByModel()
        {
            IPasswordPolicy policy = _mockRepository.StrictMock<IPasswordPolicy>();
            _target = new PasswordPolicyServiceViewModel(policy);

            using(_mockRepository.Record())
            {
                Expect.Call(policy.CheckPasswordStrength("password")).Return(true);
            }
            using(_mockRepository.Playback())
            {
                Assert.IsNull(_target.Password);
                _target.Password = "password";
                Assert.IsNotNullOrEmpty(_target.Password);
                Assert.IsTrue(_target.PasswordIsStrongEnough);
            }
        }

        [Test]
        public void VerifyNewPasswordCanSetButNotStrongIfNotValidatedByModel()
        {
            IPasswordPolicy policy = _mockRepository.StrictMock<IPasswordPolicy>();
            _target = new PasswordPolicyServiceViewModel(policy);

            using (_mockRepository.Record())
            {
                Expect.Call(policy.CheckPasswordStrength("password")).Return(false);
            }
            using (_mockRepository.Playback())
            {
                Assert.IsNull(_target.Password);
                _target.Password = "password";
                Assert.IsNotNullOrEmpty(_target.Password);
                Assert.IsFalse(_target.PasswordIsStrongEnough);
            }
        }
    }
}
