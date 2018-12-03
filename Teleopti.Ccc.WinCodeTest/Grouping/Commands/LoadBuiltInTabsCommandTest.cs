using System;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

using Is = NUnit.Framework.Is;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
	[TestFixture]
	public class LoadBuiltInTabsCommandTest
	{
		private readonly IApplicationFunction _myApplicationFunction =
			 ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
													  DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);
		
		[Test]
		public void ShouldCallRepositoryUserTabs()
		{
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(new FakeStorage());
			var personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var commonNameSetting = new CommonNameDescriptionSetting();
			var target = new LoadBuiltInTabsCommand(PersonSelectorField.Contract, unitOfWorkFactory, personSelectorReadOnlyRepository, personSelectorView, "Contract", commonNameSetting, _myApplicationFunction, Guid.Empty);

			var buId = Guid.NewGuid();
			var olaPersonId = Guid.NewGuid();
			var mickePersonId = Guid.NewGuid();
			var robinPersonId = Guid.NewGuid();
			var date = new DateOnly(2012, 1, 19);
			var datePeriod = date.ToDateOnlyPeriod();

			personSelectorView.Stub(x => x.SelectedPeriod).Return(datePeriod);
			personSelectorReadOnlyRepository.Stub(x => x.GetBuiltIn(datePeriod, PersonSelectorField.Contract, Guid.Empty)).Return(new List<IPersonSelectorBuiltIn>
																						  {
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", PersonId = olaPersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO" , PersonId = mickePersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str", PersonId = robinPersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str", PersonId = Guid.NewGuid()}
																						  });
			personSelectorView.Stub(x => x.VisiblePersonIds).Return(new List<Guid> { olaPersonId, mickePersonId, robinPersonId });
			personSelectorView.Stub(x => x.PreselectedPersonIds).Return(new HashSet<Guid> { olaPersonId });
			personSelectorView.Stub(x => x.ExpandSelected).Return(true);

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				target.Execute();
			}

			personSelectorView.AssertWasCalled(x => x.ResetTreeView(new TreeNodeAdv[0]), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldHandleCaseInsensitiveSortingForNamesOfGroups()
		{
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(new FakeStorage());
			var personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var commonNameSetting = new CommonNameDescriptionSetting();
			var target = new LoadBuiltInTabsCommand(PersonSelectorField.OptionalColumn, unitOfWorkFactory, personSelectorReadOnlyRepository, personSelectorView, nameof(PersonSelectorField.OptionalColumn), commonNameSetting, _myApplicationFunction, Guid.Empty);

			var buId = Guid.NewGuid();
			var olaPersonId = Guid.NewGuid();
			var mickePersonId = Guid.NewGuid();
			var robinPersonId = Guid.NewGuid();
			var date = new DateOnly(2012, 1, 19);
			var datePeriod = date.ToDateOnlyPeriod();

			personSelectorView.Stub(x => x.SelectedPeriod).Return(datePeriod);
			personSelectorReadOnlyRepository.Stub(x => x.GetBuiltIn(datePeriod, PersonSelectorField.OptionalColumn, Guid.Empty)).Return(new List<IPersonSelectorBuiltIn>
																						  {
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "Yes", PersonId = olaPersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "yes" , PersonId = mickePersonId},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Yes", PersonId = robinPersonId}
																						  });
			personSelectorView.Stub(x => x.VisiblePersonIds).Return(new List<Guid> { olaPersonId, mickePersonId, robinPersonId });
			personSelectorView.Stub(x => x.PreselectedPersonIds).Return(new HashSet<Guid> { olaPersonId });
			personSelectorView.Stub(x => x.ExpandSelected).Return(true);

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				target.Execute();
			}

			personSelectorView.AssertWasCalled(x => x.ResetTreeView(new TreeNodeAdv[0]), o => o.Constraints(new PredicateConstraint<TreeNodeAdv[]>(t => t[0].Nodes.Count == 2)));
		}

		[Test]
		public void ShouldRemoveFromListIfNoPermission()
		{
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(new FakeStorage());
			var personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var commonNameSetting = new CommonNameDescriptionSetting();
			var target = new LoadBuiltInTabsCommand(PersonSelectorField.Contract, unitOfWorkFactory, personSelectorReadOnlyRepository, personSelectorView, "Contract", commonNameSetting, _myApplicationFunction, Guid.Empty);

			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				var buId = Guid.NewGuid();
				var date = new DateOnly(2012, 1, 19);
				var datePeriod = new DateOnlyPeriod(date, date);
				personSelectorView.Stub(x => x.SelectedPeriod).Return(datePeriod);
				personSelectorReadOnlyRepository.Stub(x => x.GetBuiltIn(datePeriod, PersonSelectorField.Contract, Guid.Empty)).Return(new List<IPersonSelectorBuiltIn>
																						  {
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Ola", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Micke", LastName = "D", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Claes", LastName = "H", Node = "STO", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Robin", LastName = "K", Node = "Str", PersonId = Guid.NewGuid()},
																								new PersonSelectorBuiltIn { BusinessUnitId = buId ,FirstName = "Jonas", LastName = "N", Node = "Str", PersonId = Guid.NewGuid()}
																						  });
				personSelectorView.Stub(x => x.VisiblePersonIds).Return(null);

				target.Execute();

				personSelectorView.AssertWasCalled(x => x.ResetTreeView(new TreeNodeAdv[0]), o => o.IgnoreArguments());
			}
		}

		[Test]
		public void ShouldContainTheGuid()
		{
			var unitOfWorkFactory = new FakeUnitOfWorkFactory(new FakeStorage());
			var personSelectorReadOnlyRepository = MockRepository.GenerateMock<IPersonSelectorReadOnlyRepository>();
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var commonNameSetting = new CommonNameDescriptionSetting();

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var target = new LoadBuiltInTabsCommand(PersonSelectorField.Contract, unitOfWorkFactory,
					personSelectorReadOnlyRepository, personSelectorView, "Contract", commonNameSetting,
					_myApplicationFunction, Guid.Empty);
				Assert.That(target.Key, Is.EqualTo("Contract"));
			}
		}
	}

}