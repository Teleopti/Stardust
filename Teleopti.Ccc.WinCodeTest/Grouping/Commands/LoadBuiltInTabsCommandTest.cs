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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
	[TestFixture]
	public class LoadBuiltInTabsCommandTest
	{
		private MockRepository _mocks;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
		private IPersonSelectorView _personSelectorView;
		private LoadBuiltInTabsCommand _target;
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
			_unitOfWork = _mocks.StrictMock<IUnitOfWork>();
			_personSelectorReadOnlyRepository = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
			_personSelectorView = _mocks.StrictMock<IPersonSelectorView>();
			_commonNameSetting = _mocks.StrictMock<ICommonNameDescriptionSetting>();
			_target = new LoadBuiltInTabsCommand(PersonSelectorField.Contract, _unitOfWorkFactory, _personSelectorReadOnlyRepository, _personSelectorView, "Contract", _commonNameSetting, _myApplicationFunction, Guid.Empty);
		}

		[Test]
		public void ShouldCallRepositoryUserTabs()
		{
			var buId = Guid.NewGuid();
			var olaPersonId = Guid.NewGuid();
			var mickePersonId = Guid.NewGuid();
			var robinPersonId = Guid.NewGuid();
			var date = new DateOnly(2012, 1, 19);
			var datePeriod = new DateOnlyPeriod(date, date);
			var lightPerson = _mocks.StrictMock<ILightPerson>();
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(_personSelectorView.SelectedPeriod).Return(datePeriod);
			Expect.Call(_personSelectorReadOnlyRepository.GetBuiltIn(datePeriod, PersonSelectorField.Contract, Guid.Empty)).Return(new List<IPersonSelectorBuiltIn>
																						  {
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", PersonId = olaPersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO" , PersonId = mickePersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str", PersonId = robinPersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str", PersonId = Guid.NewGuid()}
																						  });
			Expect.Call(_personSelectorView.VisiblePersonIds).Return(new List<Guid> { olaPersonId, mickePersonId, robinPersonId }).Repeat.AtLeastOnce();
			Expect.Call(_personSelectorView.PreselectedPersonIds).Return(new HashSet<Guid> { olaPersonId }).Repeat.Times(3);
			Expect.Call(_personSelectorView.ExpandSelected).Return(true).Repeat.AtLeastOnce();
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
			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				var buId = Guid.NewGuid();
				var date = new DateOnly(2012, 1, 19);
				var datePeriod = new DateOnlyPeriod(date, date);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(_personSelectorView.SelectedPeriod).Return(datePeriod);
				Expect.Call(_personSelectorReadOnlyRepository.GetBuiltIn(datePeriod, PersonSelectorField.Contract, Guid.Empty)).Return(new List<IPersonSelectorBuiltIn>
																						  {
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str", PersonId = Guid.NewGuid()}
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
			Assert.That(_target.Key, Is.EqualTo("Contract"));
		}
	}

}