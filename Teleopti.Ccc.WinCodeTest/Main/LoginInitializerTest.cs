using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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

		[SetUp]
		public void Setup()
		{
			_licenseChecker = MockRepository.GenerateMock<ILogonLicenseChecker>();
			_roleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();
			_raptorSynchronizer = MockRepository.GenerateMock<IRaptorApplicationFunctionsSynchronizer>();
			_target = new LoginInitializer(_roleToPrincipalCommand, _licenseChecker, _raptorSynchronizer);
		}

		[Test]
		public void InitializeApplication_ReturnTrue()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var loggedOnUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var datasourceContainer = new DataSourceContainer(new DataSource(loggedOnUnitOfWorkFactory, null, null), null, null, AuthenticationTypeOption.Application);
			var raptorApplicationResult = new CheckRaptorApplicationFunctionsResult(new List<IApplicationFunction>(), new List<IApplicationFunction>());
			
			_licenseChecker.Expect(l => l.HasValidLicense(datasourceContainer)).Return(true);
			_raptorSynchronizer.Expect(r => r.CheckRaptorApplicationFunctions()).Return(raptorApplicationResult);
			loggedOnUnitOfWorkFactory.Expect(l => l.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null)).IgnoreArguments();
			
			var result = _target.InitializeApplication(datasourceContainer);
			result.Should().Be.True();
		}
	}
}