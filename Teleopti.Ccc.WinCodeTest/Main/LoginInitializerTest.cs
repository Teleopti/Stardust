using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WinCodeTest.Main
{
	[TestFixture]
	public class LoginInitializerTest
	{
		private ILoginInitializer _target;
		private ILogonLicenseChecker _licenseChecker;
		private IRoleToPrincipalCommand _roleToPrincipalCommand;
		private IRaptorApplicationFunctionsSynchronizer _raptorSynchronizer;
		private ILogonMatrix _logonMatrix;
		private ILogonView _view;

		[SetUp]
		public void Setup()
		{
			_licenseChecker = MockRepository.GenerateMock<ILogonLicenseChecker>();
			_roleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();
			_raptorSynchronizer = MockRepository.GenerateMock<IRaptorApplicationFunctionsSynchronizer>();
			_logonMatrix = MockRepository.GenerateMock<ILogonMatrix>();
			_view = MockRepository.GenerateMock<ILogonView>();
			_target = new LoginInitializer(_roleToPrincipalCommand, _licenseChecker, _view, _raptorSynchronizer, _logonMatrix);
		}

		[Test]
		public void InitializeApplication_ReturnTrue()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var datasourceContainer = new DataSourceContainer(new DataSource(loggedOnUnitOfWorkFactory, null, null), null);
			var raptorApplicationResult = new CheckRaptorApplicationFunctionsResult(new List<IApplicationFunction>(), new List<IApplicationFunction>());

			_licenseChecker.Expect(l => l.HasValidLicense(datasourceContainer)).Return(true);
			_raptorSynchronizer.Expect(r => r.CheckRaptorApplicationFunctions()).Return(raptorApplicationResult);
			loggedOnUnitOfWorkFactory.Expect(l => l.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null, null)).IgnoreArguments();
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null)).IgnoreArguments();

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var result = _target.InitializeApplication(datasourceContainer);
				result.Should().Be.True();
			}
		}

		[Test]
		public void InitializeApplication_ReturnFalseIfNoOnDeletedFunctions()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var datasourceContainer = new DataSourceContainer(new DataSource(loggedOnUnitOfWorkFactory, null, null), null);
			var raptorApplicationResult = new CheckRaptorApplicationFunctionsResult(new List<IApplicationFunction>(), new List<IApplicationFunction> { new ApplicationFunction("code") });

			_licenseChecker.Expect(l => l.HasValidLicense(datasourceContainer)).Return(true);
			_raptorSynchronizer.Expect(r => r.CheckRaptorApplicationFunctions()).Return(raptorApplicationResult);
			_view.Expect(v => v.ShowYesNoMessage("", "", MessageBoxDefaultButton.Button1))
				  .IgnoreArguments()
				  .Return(DialogResult.No);
			loggedOnUnitOfWorkFactory.Expect(l => l.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null, null)).IgnoreArguments();
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null)).IgnoreArguments();

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var result = _target.InitializeApplication(datasourceContainer);
				result.Should().Be.False();
			}
		}

		[Test]
		public void InitializeApplication_ReturnFalseIfNoOnAddedFunctions()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var datasourceContainer = new DataSourceContainer(new DataSource(loggedOnUnitOfWorkFactory, null, null), null);
			var raptorApplicationResult = new CheckRaptorApplicationFunctionsResult(new List<IApplicationFunction> { new ApplicationFunction("code") }, new List<IApplicationFunction>());

			_licenseChecker.Expect(l => l.HasValidLicense(datasourceContainer)).Return(true);
			_raptorSynchronizer.Expect(r => r.CheckRaptorApplicationFunctions()).Return(raptorApplicationResult);
			_view.Expect(v => v.ShowYesNoMessage("", "", MessageBoxDefaultButton.Button1))
				  .IgnoreArguments()
				  .Return(DialogResult.No);
			loggedOnUnitOfWorkFactory.Expect(l => l.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null, null)).IgnoreArguments();
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null)).IgnoreArguments();

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var result = _target.InitializeApplication(datasourceContainer);
				result.Should().Be.False();
			}
		}


		[Test]
		public void InitializeApplication_ReturnFalseIfNoPermissions()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var datasourceContainer = new DataSourceContainer(new DataSource(loggedOnUnitOfWorkFactory, null, null), null);
			var raptorApplicationResult = new CheckRaptorApplicationFunctionsResult(new List<IApplicationFunction>(), new List<IApplicationFunction>());

			_licenseChecker.Expect(l => l.HasValidLicense(datasourceContainer)).Return(true);
			_raptorSynchronizer.Expect(r => r.CheckRaptorApplicationFunctions()).Return(raptorApplicationResult);
			_view.Expect(
				 v =>
				 v.ShowErrorMessage(string.Concat(Resources.YouAreNotAuthorizedToRunTheApplication, "  "),
										  Resources.AuthenticationFailed));

			loggedOnUnitOfWorkFactory.Expect(l => l.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null, null)).IgnoreArguments();
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null)).IgnoreArguments();

			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				var result = _target.InitializeApplication(datasourceContainer);
				result.Should().Be.False();
			}

		}
	}
}