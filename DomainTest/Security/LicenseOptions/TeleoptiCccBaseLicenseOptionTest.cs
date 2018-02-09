using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
    [TestFixture]
    public class TeleoptiCccBaseLicenseOptionTest
    {
        private TeleoptiCccBaseLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new TeleoptiCccBaseLicenseOption();
        }

        [Test]
        public void VerifyEnable()
        {
            var inputList = new List<IApplicationFunction>();
            _target.EnableApplicationFunctions(inputList);
            var enabledFunctions = _target.EnabledApplicationFunctions;
            Assert.That(enabledFunctions, Is.Not.Null);
            Assert.That(enabledFunctions.Any());
        }

        [Test]
        public void ShouldIncludeRemoveShiftInBaseCollection()
		{
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
            _target.EnableApplicationFunctions(inputList);
            var enabledFunctions = _target.EnabledApplicationFunctions;
            Assert.That(enabledFunctions, Is.Not.Null);
            Assert.That(enabledFunctions.Any(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.RemoveShift));
        }
    }
}