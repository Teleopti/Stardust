using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Main
{
	[TestFixture]
	public class LoginInitializerTest
	{
		private ILoginInitializer _target;
		private ILogonLicenseChecker _licenseChecker;
		private IRoleToPrincipalCommand _roleToPrincipalCommand;



		[SetUp]
		public void Setup()
		{
			_licenseChecker = MockRepository.GenerateMock<ILogonLicenseChecker>();
			_roleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();
			_target = new LoginInitializer(_roleToPrincipalCommand, _licenseChecker);
		}

		[Test]
		public void InitializeApplication_ReturnTrue()
		{
			var datasourceContainer = new DataSourceContainer(null, null, null, AuthenticationTypeOption.Application);

			_licenseChecker.Expect(l => l.HasValidLicense(datasourceContainer)).Return(true);
			_roleToPrincipalCommand.Expect(r => r.Execute(null, null, null)).IgnoreArguments();

			var result = _target.InitializeApplication(datasourceContainer);
			result.Should().Be.True();
		}
	}
}
