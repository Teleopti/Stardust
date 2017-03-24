using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	public class CheckPasswordStrengthTest
	{
		[Test]
		public void ShouldThrowIfNotEnoughPasswordStrength()
		{
			var password = RandomName.Make();
			var passwordPolicy = MockRepository.GenerateMock<IPasswordPolicy>();
			passwordPolicy.Expect(x => x.CheckPasswordStrength(password)).Return(false);

			var target = new CheckPasswordStrength(() => passwordPolicy);
			Assert.Throws<PasswordStrengthException>(() => target.Validate(password));
		}

		[Test]
		public void ShouldNotThrowIfEnoughPasswordStrength()
		{
			var password = RandomName.Make();
			var passwordPolicy = MockRepository.GenerateMock<IPasswordPolicy>();
			passwordPolicy.Expect(x => x.CheckPasswordStrength(password)).Return(true);

			var target = new CheckPasswordStrength(() => passwordPolicy);
			Assert.DoesNotThrow(() => target.Validate(password));
		}
	}
}