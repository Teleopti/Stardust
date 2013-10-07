using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Main
{
	[TestFixture]
	public class LoginInitializerTest
	{
		private ILoginInitializer _target;
		private ILogonLicenseChecker _licenseChecker;
		private IRoleToPrincipalCommand _roleToPrincipalCommand;
		private IRaptorApplicationFunctionsSynchronizer _raptorSynchronizer;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_licenseChecker = MockRepository.GenerateMock<ILogonLicenseChecker>();
			_roleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();
			_raptorSynchronizer = MockRepository.GenerateMock<IRaptorApplicationFunctionsSynchronizer>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_target = new LoginInitializer(_roleToPrincipalCommand, _licenseChecker, _raptorSynchronizer, _currentUnitOfWorkFactory);
		}

		[Test]
		public void InitializeApplication_ReturnTrue()
		{
			var datasourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);
			var raptorApplicationResult = new CheckRaptorApplicationFunctionsResult(new List<IApplicationFunction>(), new List<IApplicationFunction>());
			var loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();

			_licenseChecker.Expect(l => l.HasValidLicense(datasourceContainer)).Return(true);
			_raptorSynchronizer.Expect(r => r.CheckRaptorApplicationFunctions()).Return(raptorApplicationResult);
			_currentUnitOfWorkFactory.Expect(c => c.LoggedOnUnitOfWorkFactory()).Return(loggedOnUnitOfWorkFactory);
			loggedOnUnitOfWorkFactory.Expect(l => l.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null)).IgnoreArguments();
			
			var result = _target.InitializeApplication(datasourceContainer);
			result.Should().Be.True();
		}
	}
}