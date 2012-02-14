using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
    [TestFixture]
    public class TeleoptiCccAgentScheduleMessengerLicenseOptionTest
    {
        private TeleoptiCccAgentScheduleMessengerLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new TeleoptiCccAgentScheduleMessengerLicenseOption();
        }

        [Test]
        public void VerifyEnable()
        {
            IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

            _target.EnableApplicationFunctions(inputList);
            IList<IApplicationFunction> resultList = _target.EnabledApplicationFunctions;
            Assert.AreEqual(2, resultList.Count);
        }
    }
}