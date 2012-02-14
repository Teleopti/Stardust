using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class AuthenticationResultAssemblerTest
    {
        private AuthenticationResultAssembler _target;
        private AuthenticationResult _authenticationResultDomain;
        private AuthenticationResultDto _authenticationResultDto;

        [SetUp]
        public void Setup()
        {
            _target = new AuthenticationResultAssembler();

            // Create domain object
            _authenticationResultDomain = new AuthenticationResult
                                              {
                                                  HasMessage = true,
                                                  Message = "Change password soon!",
                                                  Person = null,
                                                  Successful = true
                                              };

            // Create Dto object
            _authenticationResultDto = new AuthenticationResultDto();
        }

        [Test]
        public void VerifyDomainEntityToDto()
        {
            AuthenticationResultDto authenticationResultDto = _target.DomainEntityToDto(_authenticationResultDomain);

            Assert.AreEqual(_authenticationResultDomain.HasMessage, authenticationResultDto.HasMessage);
            Assert.AreEqual(_authenticationResultDomain.Successful, authenticationResultDto.Successful);
            Assert.AreEqual(_authenticationResultDomain.Message, authenticationResultDto.Message);
        }

        [Test]
        public void VerifyCanAddBusinessUnitToDto()
        {
            AuthenticationResultDto authenticationResultDto = _target.DomainEntityToDto(_authenticationResultDomain);
            BusinessUnitDto businessUnitDto = new BusinessUnitDto();
            authenticationResultDto.BusinessUnitCollection.Add(businessUnitDto);
            Assert.AreEqual(1,authenticationResultDto.BusinessUnitCollection.Count);
        }

        [Test,ExpectedException(typeof(NotImplementedException))]
        public void VerifyDtoToDomainEntityNotImplemented()
        {
            _target.DtoToDomainEntity(_authenticationResultDto);
        }
    }
}
