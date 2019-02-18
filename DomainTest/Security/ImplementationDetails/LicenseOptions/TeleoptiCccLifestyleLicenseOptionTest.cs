using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.LicenseOptions
{
    [TestFixture]
    public class TeleoptiCccLifestyleLicenseOptionTest
    {
        private TeleoptiCccLifestyleLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new TeleoptiCccLifestyleLicenseOption();
        }

        [Test]
        public void VerifyEnable()
        {
            IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

            _target.EnableApplicationFunctions(inputList);
            IList<IApplicationFunction> resultList = _target.EnabledApplicationFunctions;
            Assert.AreEqual(11, resultList.Count);
        }
    }
}