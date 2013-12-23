using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class ChangePasswordPresenterTest
    {
        private ChangePasswordPresenter target;
        private MockRepository mocks;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private IRepositoryFactory repositoryFactory;
        private IUnitOfWork unitOfWork;
        private IPerson person;
        private Guid personId;
        private IChangePasswordView view;
        private IOneWayEncryption oneWayEncryption;
        private IApplicationAuthenticationInfo applicationAuthenticationInfo;
        private IPasswordPolicy passwordPolicy;
        private IPersonRepository personRepository;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            unitOfWork = mocks.StrictMock<IUnitOfWork>();
            person = mocks.StrictMock<IPerson>();
            personId = Guid.Empty;
            view = mocks.StrictMock<IChangePasswordView>();
            passwordPolicy = mocks.StrictMock<IPasswordPolicy>();
            oneWayEncryption = mocks.StrictMock<IOneWayEncryption>();
            applicationAuthenticationInfo = mocks.StrictMock<IApplicationAuthenticationInfo>();
            personRepository = mocks.StrictMock<IPersonRepository>();
            target = new ChangePasswordPresenter(view, passwordPolicy, unitOfWorkFactory, repositoryFactory, oneWayEncryption);
        }

        private void initializeExpectation()
        {
            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
            Expect.Call(personRepository.Get(personId)).Return(person);
            Expect.Call(person.ApplicationAuthenticationInfo).Return(applicationAuthenticationInfo);
            Expect.Call(applicationAuthenticationInfo.Password).Return("currentEncryptedPassword");
            Expect.Call(unitOfWork.Dispose);
            Expect.Call(view.SetInputFocus);
        }

        [Test]
        public void VerifyOldPasswordMustMatch()
        {
            using (mocks.Record())
            {
                initializeExpectation();
                Expect.Call(()=>view.SetOldPasswordValid(false));
                Expect.Call(oneWayEncryption.EncryptString("currentNotEncryptedPassword2")).Return(
                    "currentEncryptedPassword2");
            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.SetOldPassword("currentNotEncryptedPassword2");
            }
        }

        [Test]
        public void VerifyNewPasswordMustPassPolicyCheck()
        {
            using (mocks.Record())
            {
                initializeExpectation();
                Expect.Call(() => view.SetNewPasswordValid(false));
                Expect.Call(passwordPolicy.CheckPasswordStrength("newNotEncryptedPassword2")).Return(false);
            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.SetNewPassword("newNotEncryptedPassword2");
            }
        }

        [Test]
        public void VerifyNewPasswordsWhenMatch()
        {
            using (mocks.Record())
            {
                initializeExpectation();
                Expect.Call(() => view.SetNewPasswordValid(true));
                Expect.Call(passwordPolicy.CheckPasswordStrength("newNotEncryptedPassword2")).Return(true);
                Expect.Call(() => view.SetConfirmPasswordValid(true));
            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.SetNewPassword("newNotEncryptedPassword2");
                target.SetConfirmNewPassword("newNotEncryptedPassword2");
            }
        }

        [Test]
        public void VerifySaveNewPasswordsWhenMatch()
        {
            var userDetailRepository = mocks.StrictMock<IUserDetailRepository>();
            var userDetail = mocks.StrictMock<IUserDetail>();
            using (mocks.Record())
            {
                initializeExpectation();
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
                Expect.Call(repositoryFactory.CreateUserDetailRepository(unitOfWork)).Return(userDetailRepository);
                Expect.Call(personRepository.Get(personId)).Return(person);
                Expect.Call(userDetailRepository.FindByUser(person)).Return(userDetail);
                Expect.Call(person.ChangePassword("newNotEncryptedPassword2",
                                                  StateHolderReader.Instance.StateReader.ApplicationScopeData.
                                                      LoadPasswordPolicyService, userDetail)).Return(true);
                Expect.Call(passwordPolicy.CheckPasswordStrength("newNotEncryptedPassword2")).Return(true);
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>(0));
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(view.Close);

            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.Model.OldEnteredEncryptedPassword = target.Model.OldEncryptedPassword;
                target.Model.NewPassword = "newNotEncryptedPassword2";
                target.Model.ConfirmPassword = "newNotEncryptedPassword2";
                target.Save();
                Assert.AreEqual("newNotEncryptedPassword2",
                                ((IUnsafePerson)TeleoptiPrincipal.Current).Person.ApplicationAuthenticationInfo.Password);
            }
        }

        [Test]
        public void VerifyWhenPasswordCouldNotBeChanged()
        {
            var userDetailRepository = mocks.StrictMock<IUserDetailRepository>();
            var userDetail = mocks.StrictMock<IUserDetail>();
            using (mocks.Record())
            {
                initializeExpectation();
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
                Expect.Call(repositoryFactory.CreateUserDetailRepository(unitOfWork)).Return(userDetailRepository);
                Expect.Call(personRepository.Get(personId)).Return(person);
                Expect.Call(userDetailRepository.FindByUser(person)).Return(userDetail);
                Expect.Call(person.ChangePassword("newNotEncryptedPassword2",
                                                  StateHolderReader.Instance.StateReader.ApplicationScopeData.
                                                      LoadPasswordPolicyService, userDetail)).Return(false);
                Expect.Call(passwordPolicy.CheckPasswordStrength("newNotEncryptedPassword2")).Return(true);
                Expect.Call(unitOfWork.Dispose);
                Expect.Call(view.ShowValidationError);

            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.Model.OldEnteredEncryptedPassword = target.Model.OldEncryptedPassword;
                target.Model.NewPassword = "newNotEncryptedPassword2";
                target.Model.ConfirmPassword = "newNotEncryptedPassword2";
                target.Save();
            }
        }

        [Test]
        public void VerifySaveWithErrorInModel()
        {
            using (mocks.Record())
            {
                initializeExpectation();
                Expect.Call(view.ShowValidationError);
            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.Model.NewPassword = "newNotEncryptedPassword2";
                target.Model.ConfirmPassword = "newNotEncryptedPassword3";
                target.Save();
            }
        }

        [Test]
        public void VerifySaveWithErrorWhenNoNewPassword()
        {
            using (mocks.Record())
            {
                initializeExpectation();
                Expect.Call(view.ShowValidationError);
            }
            using (mocks.Playback())
            {
                target.Initialize();
                target.Model.OldEncryptedPassword = "encrypted!";
                target.Model.OldEnteredEncryptedPassword = "encrypted!";
                target.Model.NewPassword = "";
                target.Model.ConfirmPassword = "";
                target.Save();
            }
        }
    }
}
