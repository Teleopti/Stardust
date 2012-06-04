using System;
using System.Collections.Generic;
using NUnit.Framework;
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
    public class LoadOrganizationCommandTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IRepositoryFactory _repositoryFactory;
        private IPersonSelectorView _personSelectorView;
        private LoadOrganizationCommand _target;
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
            _target = new LoadOrganizationCommand(_unitOfWorkFactory, _repositoryFactory, _personSelectorView, _commonNameSetting, _myApplicationFunction, true, true);

        }

        [Test]
        public void ShouldCallRepositoryToGetOrganization()
        {
            var buId = Guid.NewGuid();
            var teamId = Guid.NewGuid();
			var olaPersonId = Guid.NewGuid();
			var mickePersonId = Guid.NewGuid();
			var robinPersonId = Guid.NewGuid();
            var date = new DateOnly(2012, 1, 19);
            var dateOnlyPeriod = new DateOnlyPeriod(date, date);
            var lightPerson = _mocks.StrictMock<ILightPerson>();
            var rep = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonSelectorReadOnlyRepository(_unitOfWork)).Return(rep);
            Expect.Call(_personSelectorView.SelectedPeriod).Return(dateOnlyPeriod);
            Expect.Call(rep.GetOrganization(dateOnlyPeriod, true)).Return(new List<IPersonSelectorOrganization>
                                                                    {
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Site = "STO",Team = "Blue", TeamId = teamId, PersonId = olaPersonId},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Site = "STO",Team = "Blue", TeamId = teamId, PersonId = mickePersonId},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Site = "STO",Team = "Blue", TeamId = teamId, PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Site = "Str",Team = "Red", TeamId = Guid.NewGuid(), PersonId = robinPersonId},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Site = "Str",Team = "Yellow", TeamId = Guid.NewGuid(), PersonId = Guid.NewGuid()}
                                                                    });
            
			Expect.Call(_personSelectorView.VisiblePersonIds).Return(new List<Guid> { olaPersonId, mickePersonId, robinPersonId }).Repeat.AtLeastOnce();
            Expect.Call(_personSelectorView.PreselectedPersonIds).Return(new List<Guid> { olaPersonId }).Repeat.Times(3);
            Expect.Call(() => _unitOfWork.Dispose());
            Expect.Call(_commonNameSetting.BuildCommonNameDescription(lightPerson)).Repeat.Times(3).IgnoreArguments().Return("");
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
                var dateOnlyPeriod = new DateOnlyPeriod(date, date);
                var rep = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
                Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_repositoryFactory.CreatePersonSelectorReadOnlyRepository(_unitOfWork)).Return(rep);
                Expect.Call(_personSelectorView.SelectedPeriod).Return(dateOnlyPeriod);
                Expect.Call(rep.GetOrganization(dateOnlyPeriod, false)).Return(new List<IPersonSelectorOrganization>
                                                                    {
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Site = "STO",Team = "Blue", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Site = "STO",Team = "Blue", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Site = "STO",Team = "Blue", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Site = "Str",Team = "Red", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Site = "Str",Team = "Yellow", PersonId = Guid.NewGuid()}
                                                                    });
                Expect.Call(() => _unitOfWork.Dispose());
				Expect.Call(_personSelectorView.VisiblePersonIds).Return(null).Repeat.AtLeastOnce();
                Expect.Call(() => _personSelectorView.ResetTreeView(new TreeNodeAdv[0])).IgnoreArguments();
                _mocks.ReplayAll();
                _target.Execute();
                _mocks.VerifyAll();
            }
        }
    }

}