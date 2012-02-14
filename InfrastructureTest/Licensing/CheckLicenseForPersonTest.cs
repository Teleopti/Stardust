using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    [Category("LongRunning")]
    public class CheckLicenseForPersonTest
    {
        readonly MockRepository _mockRepository = new MockRepository();
        private IPersonRepository _personRepository;
        private ILicenseActivator _licenseActivator;

        [SetUp]
        public void Setup()
        {

            _personRepository = _mockRepository.StrictMock<IPersonRepository>();
            _licenseActivator = _mockRepository.StrictMock<ILicenseActivator>();

        }

        [Test]
        public void VerifyLicenseOk()
        {
            const int numberOfActiveAgents = 100;
            Expect.Call(_personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
            Expect.Call(_licenseActivator.IsThisTooManyActiveAgents(numberOfActiveAgents)).Return(false);
            
            _mockRepository.ReplayAll();
            CheckLicenseForPerson.Verify(_personRepository, _licenseActivator);
            _mockRepository.VerifyAll();
        }

        [Test(Description = "A null license is ok, the reason that we allow it is so that conversion will work")]
        public void VerifyMissingLicenseOk()
        {
            _mockRepository.ReplayAll();
            CheckLicenseForPerson.Verify(_personRepository, null);
            _mockRepository.VerifyAll();
        }
        
        [Test, ExpectedException(typeof(TooManyActiveAgentsException))]
        public void VerifyLicenseFails()
        {
            const int numberOfActiveAgents = 100;
            Expect.Call(_personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
            Expect.Call(_licenseActivator.IsThisTooManyActiveAgents(numberOfActiveAgents)).Return(true);
        	Expect.Call(_licenseActivator.LicenseType).Return(LicenseType.Agent).Repeat.AtLeastOnce();
            Expect.Call(_licenseActivator.MaxActiveAgents).Return(0);

            _mockRepository.ReplayAll();
            CheckLicenseForPerson.Verify(_personRepository, _licenseActivator);
        }
    }
}
