using System;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
    [TestFixture]
    public class LoadBuiltInTabsCommandTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
        private IPersonSelectorView _personSelectorView;
        private LoadBuiltInTabsCommand _target;
        private ICommonNameDescriptionSetting _commonNameSetting;
        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                                           DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);
        private IStatelessUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.StrictMock<IStatelessUnitOfWork>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _personSelectorView = _mocks.StrictMock<IPersonSelectorView>();
            _commonNameSetting = _mocks.StrictMock<ICommonNameDescriptionSetting>();
            _target = new LoadBuiltInTabsCommand(PersonSelectorField.Contract, _unitOfWorkFactory, _repositoryFactory, _personSelectorView, "Contract", _commonNameSetting, _myApplicationFunction, true);
        }

        [Test]
        public void ShouldCallRepositoryUserTabs()
        {
            var buId = Guid.NewGuid();
            var onePersonId = Guid.NewGuid();
            var date = new DateOnly(2012, 1, 19);
            var datePeriod = new DateOnlyPeriod(date, date);
            var lightPerson = _mocks.StrictMock<ILightPerson>();
            var rep = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonSelectorReadOnlyRepository(_unitOfWork)).Return(rep);
            Expect.Call(_personSelectorView.SelectedPeriod).Return(datePeriod);
            Expect.Call(rep.GetBuiltIn(datePeriod, PersonSelectorField.Contract)).Return(new List<IPersonSelectorBuiltIn>
                                                                    {
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", PersonId = onePersonId},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO" , PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str", PersonId = Guid.NewGuid()}
                                                                    });
            Expect.Call(_personSelectorView.PreselectedPersonIds).Return(new List<Guid> {onePersonId}).Repeat.Times(5);
            Expect.Call(() => _unitOfWork.Dispose());
            Expect.Call(_commonNameSetting.BuildCommonNameDescription(lightPerson)).Repeat.Times(5).IgnoreArguments().Return("");
            Expect.Call(() => _personSelectorView.ResetTreeView(new TreeNodeAdv[0])).IgnoreArguments();
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveFromListIfNoPermission()
        {
            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                var buId = Guid.NewGuid();
                var date = new DateOnly(2012, 1, 19);
                var datePeriod = new DateOnlyPeriod(date, date);
                var rep = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
                Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_repositoryFactory.CreatePersonSelectorReadOnlyRepository(_unitOfWork)).Return(rep);
                Expect.Call(_personSelectorView.SelectedPeriod).Return(datePeriod);
                Expect.Call(rep.GetBuiltIn(datePeriod, PersonSelectorField.Contract)).Return(new List<IPersonSelectorBuiltIn>
                                                                    {
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str", PersonId = Guid.NewGuid()}
                                                                    });
                Expect.Call(() => _unitOfWork.Dispose());
                Expect.Call(() => _personSelectorView.ResetTreeView(new TreeNodeAdv[0])).IgnoreArguments();
                _mocks.ReplayAll();
                _target.Execute();
                _mocks.VerifyAll();
            }
        }
    }

}