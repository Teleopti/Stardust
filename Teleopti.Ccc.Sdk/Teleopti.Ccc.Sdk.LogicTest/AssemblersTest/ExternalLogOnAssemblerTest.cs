using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class ExternalLogOnAssemblerTest
	{
		[Test]
		public void ShouldCreateDtoFromEntity()
		{
			var externalLogOnAssembler = new ExternalLogOnAssembler();
			var externalLogOn = new ExternalLogOn(1, 2, "OriginalId", "LogOnName", true);

			var externalLogonDto = externalLogOnAssembler.DomainEntityToDto(externalLogOn);

			Assert.IsNotNull(externalLogonDto.AcdLogOnOriginalId);
			Assert.IsNotNull(externalLogonDto.AcdLogOnName);
		}
	}
}
