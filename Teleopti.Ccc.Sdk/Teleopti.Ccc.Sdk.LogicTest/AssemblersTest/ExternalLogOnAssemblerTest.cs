using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[SetUpFixture]
	public class ExternalLogOnAssemblerTest
	{
		private ExternalLogOnAssembler _externalLogOnAssembler;
		private IExternalLogOn _externalLogOn;
		private MockRepository _mockRepository;
		
		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_externalLogOnAssembler = new ExternalLogOnAssembler();
			_externalLogOn = _mockRepository.StrictMock<IExternalLogOn>();
		}

		[Test]
		public void ShouldCreateDtoFromEntity()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_externalLogOn.AcdLogOnOriginalId).Return("OriginalId");
				Expect.Call(_externalLogOn.AcdLogOnName).Return("LogOnName");
			}

			using (_mockRepository.Playback())
			{
				var externalLogonDto = _externalLogOnAssembler.DomainEntityToDto(_externalLogOn);

				Assert.IsNotNull(externalLogonDto.AcdLogOnOriginalId);
				Assert.IsNotNull(externalLogonDto.AcdLogOnName);
			}
		}
	}
}
