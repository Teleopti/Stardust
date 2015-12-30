using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class LicensedFunctionsProviderTest
    {
        private IDefinedRaptorApplicationFunctionFactory functionFactory;
        private ILicensedFunctionsProvider target;
		private LicenseSchema schema;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            functionFactory = new DefinedRaptorApplicationFunctionFactory();
            _currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
            var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory.Stub(x => x.Current())
                .Return(unitOfWorkFactory);
            unitOfWorkFactory.Stub(x => x.Name).Return("for test");
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
            
            var result = target.LicensedFunctions("for test");
            result.IsEmpty().Should().Be.False();

        }

		[Test]
		public void ShouldDoLicenseFunctionWorkOnceOnly()
		{
			schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(_currentUnitOfWorkFactory.Current().Name, schema);
			var mocks = new MockRepository();
			var defRaptorAppFactory = mocks.DynamicMock<IDefinedRaptorApplicationFunctionFactory>();
			target = new LicensedFunctionsProvider(defRaptorAppFactory);

			using(mocks.Record())
			{
				Expect.Call(defRaptorAppFactory.ApplicationFunctionList).Return(new List<IApplicationFunction>());
			}
			using(mocks.Playback())
			{
				var res1 = target.LicensedFunctions("for test");
				var res2 = target.LicensedFunctions("for test");
				res1.Should().Be.SameInstanceAs(res2);
			}
		}

		[Test]
		public void ShouldOnlyReturnLicensedBaseFunctions()
		{
			schema = LicenseDataFactory.CreateBaseLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(_currentUnitOfWorkFactory.Current().Name, schema);

			var result = target.LicensedFunctions("for test");

			var baseLicensedApplicationFunctions = 0;
			var applicationFunctions = functionFactory.ApplicationFunctionList.ToList();

			foreach (var enabledLicenseOption in LicenseSchema.GetActiveLicenseSchema("for test").EnabledLicenseOptions)
			{
				enabledLicenseOption.EnableApplicationFunctions(applicationFunctions);
				baseLicensedApplicationFunctions += enabledLicenseOption.EnabledApplicationFunctions.Count();
			}

			result.Count().Should().Be.EqualTo(baseLicensedApplicationFunctions);
		}
    }
}
