using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
    [TestFixture]
    public class TeleoptiCccDeveloperLicenseOptionTest
    {
        private TeleoptiCccDeveloperLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new TeleoptiCccDeveloperLicenseOption();
        }

        [Test]
        public void VerifyEnable()
        {
            var factory = new DefinedRaptorApplicationFunctionFactory();
            IList<IApplicationFunction> instantInputList = factory.ApplicationFunctionList.ToList();
            IList<IApplicationFunction> storedInputList = factory.ApplicationFunctionList.ToList();
            foreach (IApplicationFunction function in storedInputList)
            {
                function.IsPreliminary = false;
            }
            instantInputList[0].IsPreliminary = true;
            storedInputList[0].IsPreliminary = true;
            Assert.IsTrue(instantInputList[0].IsPreliminary);
            Assert.IsTrue(storedInputList[0].IsPreliminary);
            _target.EnableApplicationFunctions(instantInputList);
            IList<IApplicationFunction> resultList = _target.EnabledApplicationFunctions;
			Assert.AreEqual(1, resultList.Count); // we do not add any function to the enabled list
            Assert.IsFalse(instantInputList[0].IsPreliminary); // instead set all IsPreliminary to false
            Assert.IsFalse(storedInputList[0].IsPreliminary);
        }
    }
}