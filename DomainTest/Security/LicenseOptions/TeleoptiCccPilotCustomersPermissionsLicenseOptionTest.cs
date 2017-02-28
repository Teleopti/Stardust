﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
    [TestFixture]
    public class TeleoptiCccPilotCustomersPermissionsLicenseOptionTest
    {
        private TeleoptiCccPilotCustomersPermissionsLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new TeleoptiCccPilotCustomersPermissionsLicenseOption();
        }

        [Test]
        public void VerifyEnable()
        {
            IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

            _target.EnableApplicationFunctions(inputList);
            IList<IApplicationFunction> resultList = _target.EnabledApplicationFunctions;
            Assert.AreEqual(1, resultList.Count);
        }
    }
}