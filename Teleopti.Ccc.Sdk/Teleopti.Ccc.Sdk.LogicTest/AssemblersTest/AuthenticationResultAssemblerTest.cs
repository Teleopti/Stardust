using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class AuthenticationResultAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var target = new AuthenticationResultAssembler();

			var authenticationResultDomain = new AuthenticationQuerierResult
			{
				FailReason = "Change password soon!",
				Person = null,
				Success = true
			};
			
			AuthenticationResultDto authenticationResultDto = target.DomainEntityToDto(authenticationResultDomain);

            Assert.AreNotEqual(authenticationResultDomain.Success, authenticationResultDto.HasMessage);
            Assert.AreEqual(authenticationResultDomain.Success, authenticationResultDto.Successful);
            Assert.AreEqual(authenticationResultDomain.FailReason, authenticationResultDto.Message);
        }

        [Test]
        public void VerifyCanAddBusinessUnitToDto()
        {
			var target = new AuthenticationResultAssembler();

			var authenticationResultDomain = new AuthenticationQuerierResult
			{
				FailReason = "Change password soon!",
				Person = null,
				Success = true
			};
			
			var authenticationResultDto = target.DomainEntityToDto(authenticationResultDomain);
            var businessUnitDto = new BusinessUnitDto();
            authenticationResultDto.BusinessUnitCollection.Add(businessUnitDto);
            Assert.AreEqual(1,authenticationResultDto.BusinessUnitCollection.Count);
        }

        [Test,ExpectedException(typeof(NotImplementedException))]
        public void VerifyDtoToDomainEntityNotImplemented()
        {
			var target = new AuthenticationResultAssembler();
			
			target.DtoToDomainEntity(new AuthenticationResultDto());
        }
    }
}
