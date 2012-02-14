using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationSteps
{
    [TestFixture]
    public class LicensedOptionProviderTest
    {
        private LicensedOptionProvider _target;

        [SetUp]
        public void Setup()
        {
            _target = new LicensedOptionProvider();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySetInputEntityList()
        {
            IApplicationFunction applicationFunction1 =
                ApplicationFunctionFactory.CreateApplicationFunction("1");
            IApplicationFunction applicationFunction2 =
                ApplicationFunctionFactory.CreateApplicationFunction("2");
            IApplicationFunction applicationFunction3 =
                ApplicationFunctionFactory.CreateApplicationFunction("3");

            IList<IApplicationFunction> expectedList = new List<IApplicationFunction>();
            expectedList.Add(applicationFunction1);
            expectedList.Add(applicationFunction2);
            expectedList.Add(applicationFunction3);

            IList<IAuthorizationEntity> inputEntityList = AuthorizationEntityExtender.ConvertToBaseList(expectedList);
            _target.InputEntityList = inputEntityList;
            // read back
            IList<IAuthorizationEntity> resultList = _target.InputEntityList;
            Assert.IsNotNull(resultList);
            Assert.AreNotEqual(0, resultList.Count);
            for (int counter = 0; counter < inputEntityList.Count; counter++)
            {
                Assert.AreSame(inputEntityList[counter], resultList[counter]);
            }

        }

        [Test]
        public void VerifyResultEntityList()
        {
            _target.InputEntityList = new List<IAuthorizationEntity>();
            LicenseSchema schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
            // changes a bit the default schema
            schema.LicenseOptions[0].Enabled = true;
            schema.LicenseOptions[1].Enabled = false;
            schema.EnabledLicenseSchema = DefinedLicenseSchemaCodes.TeleoptiCccSchema;
            LicenseSchema.ActiveLicenseSchema = schema;

            int expectedCount = 1;

            IList<LicenseOption> result = _target.ResultEntityList;
            Assert.AreEqual(expectedCount, result.Count);

            LicenseSchema.ActiveLicenseSchema = null;
        }
    }
}
