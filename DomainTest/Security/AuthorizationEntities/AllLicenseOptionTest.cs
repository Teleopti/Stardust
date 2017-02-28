using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class AllLicenseOptionTest
    {
        private AllLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new AllLicenseOption();
        }

        [Test]
        public void VerifyEnable()
        {
            IList<IApplicationFunction> inputList = new List<IApplicationFunction>();

            _target.EnableApplicationFunctions(inputList);
            IList<IApplicationFunction> resultList = _target.EnabledApplicationFunctions;
            Assert.AreEqual(inputList.Count, resultList.Count);
        }
    }
}
