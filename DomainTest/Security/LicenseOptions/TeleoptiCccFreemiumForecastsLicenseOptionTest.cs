using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
    [TestFixture]
    public class TeleoptiCccFreemiumForecastsLicenseOptionTest
    {
        private TeleoptiCccFreemiumForecastsLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new TeleoptiCccFreemiumForecastsLicenseOption();
        }

        [Test]
        public void VerifyEnable()
        {
            IList<IApplicationFunction> inputList = ApplicationFunctionFactory.CreateApplicationFunctionStructure();

            _target.EnableApplicationFunctions(inputList);
            IList<IApplicationFunction> resultList = _target.EnabledApplicationFunctions;
            Assert.AreEqual(7, resultList.Count);
        }
    }
}