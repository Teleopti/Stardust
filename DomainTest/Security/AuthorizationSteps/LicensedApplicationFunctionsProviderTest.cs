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
    public class LicensedApplicationFunctionsProviderTest
    {
        private LicensedApplicationFunctionsProvider _target;
        private IList<LicenseOption> _licenceOptions;

        [SetUp]
        public void Setup()
        {
            _target = new LicensedApplicationFunctionsProvider();
            _licenceOptions = new List<LicenseOption>();
            _licenceOptions.Add(new LicenseOption(DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/OPT1", "Option 1 Name"));
            _licenceOptions.Add(new LicenseOption(DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/OPT2", "Option 2 Name"));
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifySetInputEntityList()
        {
            IList<IAuthorizationEntity> inputEntityList = AuthorizationEntityExtender.ConvertToBaseList(_licenceOptions);
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
            IApplicationFunction sharedApplicationFunction =
                ApplicationFunctionFactory.CreateApplicationFunction("Shared");
                        IApplicationFunction onlyOption1ApplicationFunction =
                ApplicationFunctionFactory.CreateApplicationFunction("Option 1 Only");
                        IApplicationFunction onlyOption2ApplicationFunction =
                ApplicationFunctionFactory.CreateApplicationFunction("Option 2 Only");

            _licenceOptions[0].EnabledApplicationFunctions.Add(sharedApplicationFunction);
            _licenceOptions[0].EnabledApplicationFunctions.Add(onlyOption1ApplicationFunction);
            
            _licenceOptions[1].EnabledApplicationFunctions.Add(onlyOption2ApplicationFunction);
            _licenceOptions[1].EnabledApplicationFunctions.Add(sharedApplicationFunction);

            IList<IApplicationFunction> expectedList = new List<IApplicationFunction>();
            expectedList.Add(sharedApplicationFunction);
            expectedList.Add(onlyOption1ApplicationFunction);
            expectedList.Add(onlyOption2ApplicationFunction);

            _target.InputEntityList = AuthorizationEntityExtender.ConvertToBaseList(_licenceOptions);

            IList<IApplicationFunction> resultList = _target.ResultEntityList;
            for (int counter = 0; counter < expectedList.Count; counter++)
            {
                Assert.AreSame(expectedList[counter], resultList[counter]);
            }
        }

    }
}
