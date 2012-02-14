using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class LicensedFunctionsProviderTest
    {
        private IDefinedRaptorApplicationFunctionFactory functionFactory;
        private ILicensedFunctionsProvider target;
		private LicenseSchema schema;

        [SetUp]
        public void Setup()
        {
            functionFactory = new DefinedRaptorApplicationFunctionFactory();
            target = new LicensedFunctionsProvider(functionFactory);
			schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.ActiveLicenseSchema = schema;
		}

		[TearDown]
		public void TearDown()
		{
			LicenseSchema.ActiveLicenseSchema = null;
		}

        [Test]
        public void ShouldReturnAllLicensedFunctions()
        {
        	// changes a bit the default schema
            schema.LicenseOptions[0].Enabled = true;
            schema.LicenseOptions[1].Enabled = false;
            schema.EnabledLicenseSchema = DefinedLicenseSchemaCodes.TeleoptiCccSchema;
            
            var result = target.LicensedFunctions();
            result.IsEmpty().Should().Be.False();

        }

		[Test]
		public void ShouldDoLicenseFunctionWorkOnceOnly()
		{
			var mocks = new MockRepository();
			var defRaptorAppFactory = mocks.DynamicMock<IDefinedRaptorApplicationFunctionFactory>();
			target = new LicensedFunctionsProvider(defRaptorAppFactory);

			using(mocks.Record())
			{
				Expect.Call(defRaptorAppFactory.ApplicationFunctionList).Return(new List<IApplicationFunction>());
			}
			using(mocks.Playback())
			{
				var res1 = target.LicensedFunctions();
				var res2 = target.LicensedFunctions();
				res1.Should().Be.SameInstanceAs(res2);
			}
		}
    }
}
