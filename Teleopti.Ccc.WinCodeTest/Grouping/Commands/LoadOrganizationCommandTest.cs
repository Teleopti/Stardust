using System;
using System.Collections.Generic;
using NUnit.Framework;
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
    public class LoadOrganizationCommandTest
    {
        private MockRepository _mocks;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
        private IPersonSelectorView _personSelectorView;
        private LoadOrganizationCommand _target;
        private ICommonNameDescriptionSetting _commonNameSetting;
        private readonly IApplicationFunction _myApplicationFunction =
            ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
                                           DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			_personSelectorReadOnlyRepository = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
            _personSelectorView = _mocks.StrictMock<IPersonSelectorView>();
            _commonNameSetting = _mocks.DynamicMock<ICommonNameDescriptionSetting>();
            _target = new LoadOrganizationCommand(_unitOfWorkFactory, _personSelectorReadOnlyRepository, _personSelectorView, _commonNameSetting, _myApplicationFunction, true, true);

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
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            Expect.Call(_personSelectorView.SelectedPeriod).Return(dateOnlyPeriod);
            Expect.Call(_personSelectorReadOnlyRepository.GetOrganization(dateOnlyPeriod, true)).Return(new List<IPersonSelectorOrganization>
                                                                    {
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Site = "STO",Team = "Blue", TeamId = teamId, PersonId = olaPersonId},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Site = "STO",Team = "Blue", TeamId = teamId, PersonId = mickePersonId},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Site = "STO",Team = "Blue", TeamId = teamId, PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Site = "Str",Team = "Red", TeamId = Guid.NewGuid(), PersonId = robinPersonId},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Site = "Str",Team = "Yellow", TeamId = Guid.NewGuid(), PersonId = Guid.NewGuid()}
                                                                    });
            
			Expect.Call(_personSelectorView.VisiblePersonIds).Return(new List<Guid> { olaPersonId, mickePersonId, robinPersonId }).Repeat.AtLeastOnce();
            Expect.Call(_personSelectorView.PreselectedPersonIds).Return(new HashSet<Guid> { olaPersonId }).Repeat.Times(3);
        	Expect.Call(_personSelectorView.ExpandSelected).Return(true).Repeat.AtLeastOnce();
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
                var date = new DateOnly(2012, 1, 19);
                var dateOnlyPeriod = new DateOnlyPeriod(date, date);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_personSelectorView.SelectedPeriod).Return(dateOnlyPeriod);
                Expect.Call(_personSelectorReadOnlyRepository.GetOrganization(dateOnlyPeriod, false)).Return(new List<IPersonSelectorOrganization>
                                                                    {
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Site = "STO",Team = "Blue", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Site = "STO",Team = "Blue", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Site = "STO",Team = "Blue", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Site = "Str",Team = "Red", PersonId = Guid.NewGuid()},
                                                                        new PersonSelectorOrganization { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Site = "Str",Team = "Yellow", PersonId = Guid.NewGuid()}
                                                                    });
				Expect.Call(_personSelectorView.VisiblePersonIds).Return(null).Repeat.AtLeastOnce();
                Expect.Call(() => _personSelectorView.ResetTreeView(new TreeNodeAdv[0])).IgnoreArguments();
                _mocks.ReplayAll();
                _target.Execute();
                _mocks.VerifyAll();
            }
        }

		[Test]
		public void ShouldRemoveDuplicateEntriesFromList()
		{
			var buId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var date = new DateOnly(2012, 1, 19);
			var dateOnlyPeriod = new DateOnlyPeriod(date.AddDays(-730), date);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_personSelectorView.SelectedPeriod).Return(dateOnlyPeriod);
			Expect.Call(_personSelectorReadOnlyRepository.GetOrganization(dateOnlyPeriod, true)).Return(new List<IPersonSelectorOrganization>
			                                                               	{
			                                                               		new PersonSelectorOrganization
			                                                               			{
			                                                               				BusinessUnitId = buId,
			                                                               				FirstName = "Robin",
			                                                               				LastName = "K",
			                                                               				Site = "Str",
			                                                               				Team = "Red",
			                                                               				PersonId = personId,
			                                                               				TeamId = teamId,
			                                                               				SiteId = siteId
			                                                               			},
			                                                               		new PersonSelectorOrganization
			                                                               			{
			                                                               				BusinessUnitId = buId,
			                                                               				FirstName = "Robin",
			                                                               				LastName = "K",
			                                                               				Site = "Str",
			                                                               				Team = "Red",
			                                                               				PersonId = personId,
			                                                               				TeamId = teamId,
			                                                               				SiteId = siteId
			                                                               			},
			                                                               		new PersonSelectorOrganization
			                                                               			{
			                                                               				BusinessUnitId = buId,
			                                                               				FirstName = "Robin",
			                                                               				LastName = "K",
			                                                               				Site = "Str",
			                                                               				Team = "Yellow",
			                                                               				PersonId = personId,
			                                                               				TeamId = Guid.NewGuid(),
			                                                               				SiteId = siteId
			                                                               			},
			                                                               		new PersonSelectorOrganization
			                                                               			{
			                                                               				BusinessUnitId = buId,
			                                                               				FirstName = "Robin",
			                                                               				LastName = "K",
			                                                               				Site = "STO",
			                                                               				Team = "Red",
			                                                               				PersonId = personId,
			                                                               				TeamId = teamId,
			                                                               				SiteId = Guid.NewGuid()
			                                                               			}
			                                                               	});

			Expect.Call(_personSelectorView.PreselectedPersonIds).Return(new HashSet<Guid> { personId }).Repeat.AtLeastOnce();
			Expect.Call(_personSelectorView.VisiblePersonIds).Return(null).Repeat.AtLeastOnce();
			Expect.Call(_personSelectorView.ExpandSelected).Return(true).Repeat.AtLeastOnce();
			Expect.Call(() => _personSelectorView.ResetTreeView(new TreeNodeAdv[0])).Constraints(
				Rhino.Mocks.Constraints.Is.Matching<TreeNodeAdv[]>(t =>
					t[0].GetNodeCount(true) == 8)); //2 Sites (STO+STr), 3 Teams (Str/Yellow+Str/Red+STO/Red), 3 Occurences of person (Str/Yellow+Str/Red+STO/Red)
			_mocks.ReplayAll();

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Execute();
			}

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldContainTheGuid()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				Assert.That(_target.Key, Is.EqualTo("Organization"));
			}
		}
    }
}