using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class FunctionsForRoleProviderTest
    {
        private MockRepository mocks;
        private ILicensedFunctionsProvider licensedFunctionsProvider;
        private FunctionsForRoleProvider target;
        private IExternalFunctionsProvider externalFunctionsProvider;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private const string Function = "test";

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            licensedFunctionsProvider = mocks.StrictMock<ILicensedFunctionsProvider>();
            externalFunctionsProvider = mocks.StrictMock<IExternalFunctionsProvider>();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            target = new FunctionsForRoleProvider(licensedFunctionsProvider, externalFunctionsProvider);
        }

        [Test]
        public void ShouldTransformRoleWithAllFunctionIntoClaimSet()
        {
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var unitOfWork = mocks.StrictMock<IUnitOfWork>();
            var applicationFunction = new ApplicationFunction { FunctionCode = Function };
            var allApplicationFunction = new ApplicationFunction { FunctionCode = DefinedRaptorApplicationFunctionPaths.All };
            using (mocks.Record())
            {
                Expect.Call(applicationRole.ApplicationFunctionCollection).Return(new[] { allApplicationFunction }).Repeat.AtLeastOnce();
	            Expect.Call(unitOfWorkFactory.Name).Return("datasource");
	            Expect.Call(unitOfWorkFactory.CurrentUnitOfWork()).Return(unitOfWork);
                Expect.Call(licensedFunctionsProvider.LicensedFunctions("datasource")).Return(new[] { applicationFunction });
                Expect.Call(externalFunctionsProvider.ExternalFunctions(unitOfWork)).Return(
                    new List<IApplicationFunction>());
            }
            using (mocks.Playback())
            {
                var availableFunctions = target.AvailableFunctions(applicationRole, unitOfWorkFactory);
                availableFunctions.Count().Should().Be.EqualTo(1);
                availableFunctions.Any(c => c.FunctionCode == Function).Should().Be.True();
            }
        }

        [Test]
        public void ShouldTransformRoleWithRoleFunctionIntoClaimSet()
        {
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = new ApplicationFunction { FunctionCode = Function };
            using (mocks.Record())
            {
	            Expect.Call(unitOfWorkFactory.Name).Return("datasource");
                Expect.Call(licensedFunctionsProvider.LicensedFunctions("datasource")).Return(new IApplicationFunction[] {});
                Expect.Call(applicationRole.ApplicationFunctionCollection).Return(new[] { applicationFunction }).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var availableFunctions = target.AvailableFunctions(applicationRole, unitOfWorkFactory);
                availableFunctions.Count().Should().Be.EqualTo(1);
                availableFunctions.Any(c => c.FunctionCode == Function).Should().Be.True();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily"), Test]
        public void ShouldIgnoreDeletedFunctionsWhenTransformRoleWithRoleFunctionIntoClaimSet()
        {
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = mocks.StrictMultiMock<IApplicationFunction>(typeof(IDeleteTag));
            using (mocks.Record())
            {
                Expect.Call(applicationFunction.FunctionPath).Return(Function).Repeat.AtLeastOnce();
                Expect.Call(applicationFunction.ForeignSource).Return(DefinedForeignSourceNames.SourceRaptor);

                Expect.Call(applicationFunction.IsPreliminary).Return(false);
                Expect.Call(applicationFunction.IsPreliminary = false);

                Expect.Call(applicationFunction.SortOrder).Return(10);
                Expect.Call(applicationFunction.SortOrder = 10);

                Expect.Call(applicationFunction.FunctionCode).Return("code1").Repeat.AtLeastOnce();
                Expect.Call(applicationFunction.FunctionCode = "code1");

                Expect.Call(applicationFunction.FunctionDescription).Return("desc1");
                Expect.Call(applicationFunction.FunctionDescription = "desc1");
	            Expect.Call(unitOfWorkFactory.Name).Return("datasource");
                Expect.Call(licensedFunctionsProvider.LicensedFunctions("datasource")).Return(new[] { applicationFunction});
                Expect.Call(((IDeleteTag)applicationFunction).IsDeleted).Return(true);
                Expect.Call(((IDeleteTag)applicationFunction).IsDeleted).Return(false);

                Expect.Call(applicationRole.ApplicationFunctionCollection).Return(new[] { applicationFunction, applicationFunction }).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var availableFunctions = target.AvailableFunctions(applicationRole, unitOfWorkFactory);
                availableFunctions.Count().Should().Be.EqualTo(1);
                availableFunctions.Any(c => c.FunctionCode == "code1").Should().Be.True();
            }
        }
    }
}