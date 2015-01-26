using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class LicenseOptionTest
    {
        
        private LicenseOption _target;

        [SetUp]
        public void TestInit()
        {
            _target = new LicenseOption();
        }

        [TearDown]
        public void TestDispose()
        {
            _target = null;
        }

        [Test]
        public void VerifyConstructorOverload1()
        {
            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target.EnabledApplicationFunctions);
        }

        [Test]
        public void VerifyConstructorOverload2()
        {
            const string optionPath = "SchemaCode/OptionCode";
            const string optionName = "Option Name";
            _target = new LicenseOption(optionPath, optionName);
            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target.EnabledApplicationFunctions);
            Assert.AreEqual("OptionCode", _target.LicenseOptionCode);
            Assert.AreEqual("SchemaCode", _target.LicenseSchemaCode);
            Assert.AreEqual(optionPath, _target.LicenseOptionPath);
            Assert.AreEqual(optionName, _target.OptionName);

        }

        [Test]
        public void VerifyLicenseOptionCode()
        {
            // Declare variable to hold property set method
            System.String setValue = "OptionCode";

            // Test set method
            _target.LicenseOptionCode = setValue;

            // Declare return variable to hold property get method
            System.String getValue;

            // Test get method
            getValue = _target.LicenseOptionCode;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyLicenseSchemaCode()
        {
            // Declare variable to hold property set method
            System.String setValue = "LicenseSchemaCode";

            // Test set method
            _target.LicenseSchemaCode = setValue;

            // Declare return variable to hold property get method
            System.String getValue;

            // Test get method
            getValue = _target.LicenseSchemaCode;

            // Perform Assert Tests
            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyEnabledApplicationFunctions()
        {
            // Test set method
            _target.EnabledApplicationFunctions.Add(new ApplicationFunction("APP1"));
            _target.EnabledApplicationFunctions.Add(new ApplicationFunction("APP2"));

            // Perform Assert Tests
            Assert.AreEqual(2, _target.EnabledApplicationFunctions.Count);
        }

        [Test]
        public void VerifyEnabled()
        {
            bool setValue = !_target.Enabled;
            _target.Enabled = setValue;
            bool getValue = _target.Enabled;
            Assert.AreEqual(setValue, getValue);

        }

        [Test]
        public void VerifyEnableApplicationFunctions()
        {

            Assert.AreEqual(0, _target.EnabledApplicationFunctions.Count);

            IList<IApplicationFunction> applicationFunctions = new List<IApplicationFunction>();
            applicationFunctions.Add(new ApplicationFunction("APP1"));
            applicationFunctions.Add(new ApplicationFunction("APP2"));

            _target.EnableApplicationFunctions(applicationFunctions);

            Assert.AreEqual(2, _target.EnabledApplicationFunctions.Count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ByPath")]
        [Test]
        public void VerifyFindLicenseOptionByPath()
        {
            const string path1 = "root/name1";
            const string path2 = "root/name1";
            const string pathNotExist = "root/notexist";
            const string expectedNotExistenceOptionName = "root notexist";
            const string name1 = "name1";
            const string name2 = "name2";
            LicenseOption option1 = new LicenseOption(path1, name1);
            LicenseOption option2 = new LicenseOption(path2, name2);

            LicenseOption result; 
            result = LicenseOption.FindLicenseOptionByPath(new List<LicenseOption> {option1, option2}, path1);
            Assert.AreEqual(option1, result);

            result = LicenseOption.FindLicenseOptionByPath(new List<LicenseOption> { option1, option2 }, pathNotExist);
            Assert.IsNotNull(result);
            Assert.AreEqual(pathNotExist, result.LicenseOptionPath);
            Assert.AreEqual(expectedNotExistenceOptionName, result.OptionName);

        }

    }

}
