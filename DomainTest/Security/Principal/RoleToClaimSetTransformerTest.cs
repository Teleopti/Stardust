using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
    [TestFixture]
    public class RoleToClaimSetTransformerTest
    {
        private MockRepository mocks;
        private IFunctionsForRoleProvider functionsForRoleProvider;
        private RoleToClaimSetTransformer target;
        private IUnitOfWorkFactory unitOfWorkFactory;
        private const string Function = "test";

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            functionsForRoleProvider = mocks.StrictMock<IFunctionsForRoleProvider>();
            unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            target = new RoleToClaimSetTransformer(functionsForRoleProvider);
        }

        [Test]
        public void ShouldTransformRoleWithNoneAuthorizationOptionIntoClaimSet()
        {
            var availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.None};
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = new ApplicationFunction { FunctionCode = Function };
            using (mocks.Record())
            {
                Expect.Call(applicationRole.AvailableData).Return(availableData).Repeat.AtLeastOnce();
                Expect.Call(functionsForRoleProvider.AvailableFunctions(applicationRole,unitOfWorkFactory)).Return(new[] {applicationFunction}).Repeat.AtLeastOnce();
            }
            using(mocks.Playback())
            {
                var claimSet = target.Transform(applicationRole,unitOfWorkFactory);
                claimSet.Count.Should().Be.EqualTo(2);
            }
        }

        [Test]
        public void ShouldTransformRoleWithEveryoneAuthorizationOptionIntoClaimSet()
        {
            var availableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone};
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = new ApplicationFunction {FunctionCode = Function};
            using (mocks.Record())
            {
                Expect.Call(applicationRole.AvailableData).Return(availableData).Repeat.AtLeastOnce();
                Expect.Call(functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWorkFactory)).Return(new[] { applicationFunction }).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var claimSet = target.Transform(applicationRole,unitOfWorkFactory);
                claimSet.Count.Should().Be.EqualTo(3);
            }
        }

        [Test]
        public void ShouldTransformRoleWithBusinessUnitAuthorizationOptionIntoClaimSet()
        {
            var availableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.MyBusinessUnit };
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = new ApplicationFunction { FunctionCode = Function };
            using (mocks.Record())
            {
                Expect.Call(applicationRole.AvailableData).Return(availableData).Repeat.AtLeastOnce();
                Expect.Call(functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWorkFactory)).Return(new[] { applicationFunction }).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var claimSet = target.Transform(applicationRole,unitOfWorkFactory);
                claimSet.Count.Should().Be.EqualTo(3);
            }
        }

        [Test]
        public void ShouldTransformRoleWithSiteAuthorizationOptionIntoClaimSet()
        {
            var availableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.MySite };
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = new ApplicationFunction { FunctionCode = Function };
            using (mocks.Record())
            {
                Expect.Call(applicationRole.AvailableData).Return(availableData).Repeat.AtLeastOnce();
                Expect.Call(functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWorkFactory)).Return(new[] { applicationFunction }).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var claimSet = target.Transform(applicationRole,unitOfWorkFactory);
                claimSet.Count.Should().Be.EqualTo(3);
            }
        }

        [Test]
        public void ShouldTransformRoleWithTeamAuthorizationOptionIntoClaimSet()
        {
            var availableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.MyTeam };
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = new ApplicationFunction { FunctionCode = Function };
            using (mocks.Record())
            {
                Expect.Call(applicationRole.AvailableData).Return(availableData).Repeat.AtLeastOnce();
                Expect.Call(functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWorkFactory)).Return(new[] { applicationFunction }).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var claimSet = target.Transform(applicationRole,unitOfWorkFactory);
                claimSet.Count.Should().Be.EqualTo(3);
            }
        }

        [Test]
        public void ShouldTransformRoleWithOwnAuthorizationOptionIntoClaimSet()
        {
            var availableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.MyOwn };
            var applicationRole = mocks.StrictMock<IApplicationRole>();
            var applicationFunction = new ApplicationFunction { FunctionCode = Function };
            using (mocks.Record())
            {
                Expect.Call(applicationRole.AvailableData).Return(availableData).Repeat.AtLeastOnce();
                Expect.Call(functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWorkFactory)).Return(new[] { applicationFunction }).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                var claimSet = target.Transform(applicationRole,unitOfWorkFactory);
                claimSet.Count.Should().Be.EqualTo(3);
            }
        }

		[Test]
		public void ShouldTransformRoleWithNoAvailableDataIntoClaimSet()
		{
			var applicationRole = mocks.StrictMock<IApplicationRole>();
			var applicationFunction = new ApplicationFunction { FunctionCode = Function };
			using (mocks.Record())
			{
				Expect.Call(applicationRole.AvailableData).Return(null).Repeat.AtLeastOnce();
				Expect.Call(functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWorkFactory)).Return(new[] { applicationFunction }).Repeat.AtLeastOnce();
			}
			using (mocks.Playback())
			{
				var claimSet = target.Transform(applicationRole, unitOfWorkFactory);
				claimSet.Count.Should().Be.EqualTo(1);
			}
		}
    }
}
