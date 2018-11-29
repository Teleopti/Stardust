using System;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
    [TestFixture]
    public class LoadUserDefinedTabsCommandTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
        private IPersonSelectorView _personSelectorView;
        private LoadUserDefinedTabsCommand _target;
        private ICommonNameDescriptionSetting _commonNameSetting;
        private Guid _guid;
        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
                                           DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();
			_personSelectorReadOnlyRepository = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
            _personSelectorView = _mocks.StrictMock<IPersonSelectorView>();
            _commonNameSetting = _mocks.StrictMock<ICommonNameDescriptionSetting>();
            _guid = Guid.NewGuid();
            _target = new LoadUserDefinedTabsCommand(_unitOfWorkFactory, _personSelectorReadOnlyRepository, _personSelectorView, _guid, _commonNameSetting, _myApplicationFunction);
        }

        [Test]
        public void ShouldCallRepositoryUserTabs()
        {
            var buId = Guid.NewGuid();
            var stoGuid = Guid.NewGuid();
            var strId = Guid.NewGuid();
			var olaPersonId = Guid.NewGuid();
			var mickePersonId = Guid.NewGuid();
			var robinPersonId = Guid.NewGuid();
            var lightPerson = _mocks.StrictMock<ILightPerson>();
            var date = new DateOnly(2012, 1, 19);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            Expect.Call(_personSelectorView.SelectedDate).Return(date);
            Expect.Call(_personSelectorReadOnlyRepository.GetUserDefinedTab(date, _guid)).Return(new List<IPersonSelectorUserDefined>
                                                                    {
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "", LastName = "", Node = "Root", ParentId = null, NodeId = _guid},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", ParentId = _guid,NodeId = stoGuid, PersonId = olaPersonId, Show = true},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO",ParentId = _guid,NodeId = stoGuid, PersonId = mickePersonId, Show = true},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO",ParentId = _guid,NodeId = stoGuid, PersonId = Guid.NewGuid(), Show = true},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str",ParentId = _guid,NodeId = strId, PersonId = robinPersonId, Show = true},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str",ParentId = _guid,NodeId = strId, PersonId = Guid.NewGuid(), Show = true}
                                                                    });
			Expect.Call(_personSelectorView.VisiblePersonIds).Return(new List<Guid> { olaPersonId, mickePersonId, robinPersonId }).Repeat.AtLeastOnce();
            Expect.Call(_personSelectorView.PreselectedPersonIds).Return(new HashSet<Guid> { olaPersonId }).Repeat.Times(3);
        	Expect.Call(_personSelectorView.ExpandSelected).Return(true).Repeat.AtLeastOnce();
            Expect.Call(() => _unitOfWork.Dispose());
            Expect.Call(_commonNameSetting.BuildFor(lightPerson)).Repeat.Times(3).IgnoreArguments().Return("");
            Expect.Call(() => _personSelectorView.ResetTreeView(new TreeNodeAdv[0])).IgnoreArguments();
            _mocks.ReplayAll();

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Execute();
			}

			_mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveFromListIfNoPermission()
        {
            using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
                var buId = Guid.NewGuid();
                var stoGuid = Guid.NewGuid();
                var strId = Guid.NewGuid();
                var date = new DateOnly(2012, 1, 19);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_personSelectorView.SelectedDate).Return(date);
                Expect.Call(_personSelectorReadOnlyRepository.GetUserDefinedTab(date, _guid)).Return(new List<IPersonSelectorUserDefined>
                                                                    {
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "", LastName = "", Node = "Root", ParentId = null, NodeId = _guid},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", ParentId = _guid,NodeId = stoGuid, PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO",ParentId = _guid,NodeId = stoGuid, PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO",ParentId = _guid,NodeId = stoGuid, PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str",ParentId = _guid,NodeId = strId, PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorUserDefined { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str",ParentId = _guid,NodeId = strId, PersonId = Guid.NewGuid()}
                                                                    });
                Expect.Call(() => _unitOfWork.Dispose());
				Expect.Call(_personSelectorView.VisiblePersonIds).Return(null).Repeat.AtLeastOnce();
                Expect.Call(() => _personSelectorView.ResetTreeView(new TreeNodeAdv[0])).IgnoreArguments();
                _mocks.ReplayAll();
                _target.Execute();
                _mocks.VerifyAll();
            }
        }

        [Test]
        public void ShouldContainTheGuid()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				Assert.That(_target.Id, Is.EqualTo(_guid));
				Assert.That(_target.Key, Is.EqualTo(_guid.ToString()));
			}
		}
    }

}