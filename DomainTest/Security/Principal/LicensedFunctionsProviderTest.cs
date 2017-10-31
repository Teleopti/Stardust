using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class LicensedFunctionsProviderTest
    {
        private IDefinedRaptorApplicationFunctionFactory functionFactory;
        private ILicensedFunctionsProvider target;
		private LicenseSchema schema;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private const string tenantName = "for test";

	    [SetUp]
        public void Setup()
        {
            functionFactory = new DefinedRaptorApplicationFunctionFactory();
			_currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(new FakeStorage()).WithCurrent(new FakeUnitOfWorkFactory(new FakeStorage()) { Name = tenantName });
			target = new LicensedFunctionsProvider(functionFactory);
        }

		[TearDown]
		public void TearDown()
		{
		    LicenseSchema.SetActiveLicenseSchema(_currentUnitOfWorkFactory.Current().Name, null);
		}

        [Test]
        public void ShouldReturnAllLicensedFunctions()
		{
			schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(_currentUnitOfWorkFactory.Current().Name, schema);

        	// changes a bit the default schema
            schema.LicenseOptions.ElementAt(0).Enabled = false;
            schema.LicenseOptions.ElementAt(1).Enabled = false;
            schema.EnabledLicenseSchema = DefinedLicenseSchemaCodes.TeleoptiWFMSchema;
            
            var result = target.LicensedFunctions(tenantName);
            result.IsEmpty().Should().Be.False();
        }

		[Test]
		public void ShouldDoLicenseFunctionWorkOnceOnly()
		{
			schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(_currentUnitOfWorkFactory.Current().Name, schema);

			var res1 = target.LicensedFunctions(tenantName);
			var res2 = target.LicensedFunctions(tenantName);
			res1.Should().Be.SameInstanceAs(res2);
		}

		[Test]
		public void ShouldOnlyReturnLicensedBaseFunctions()
		{
			schema = LicenseDataFactory.CreateBaseLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(_currentUnitOfWorkFactory.Current().Name, schema);

			var result = target.LicensedFunctions(tenantName);

			var baseLicensedApplicationFunctions = 0;
			var applicationFunctions = functionFactory.ApplicationFunctions.ToList();

			foreach (var enabledLicenseOption in LicenseSchema.GetActiveLicenseSchema(tenantName).EnabledLicenseOptions)
			{
				enabledLicenseOption.EnableApplicationFunctions(applicationFunctions);
				baseLicensedApplicationFunctions += enabledLicenseOption.EnabledApplicationFunctions.Length;
			}

			result.Count().Should().Be.EqualTo(baseLicensedApplicationFunctions);
		}
    }
}
