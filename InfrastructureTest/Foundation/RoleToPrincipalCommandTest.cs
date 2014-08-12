using System;
using System.IdentityModel.Claims;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    /// <summary>
    /// Tests for RoleToPrincipalCommand
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class RoleToPrincipalCommandTest
    {
        private MockRepository mocks;
        private IRoleToPrincipalCommand target;
        private IRoleToClaimSetTransformer roleToClaimSetTransformer;
        private IPersonRepository personRepository;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            roleToClaimSetTransformer = mocks.StrictMock<IRoleToClaimSetTransformer>();
            personRepository = mocks.StrictMock<IPersonRepository>();
            target = new RoleToPrincipalCommand(roleToClaimSetTransformer);
        }

        [Test]
        public void ShouldExecuteCommand()
        {
			var currentPrincipal = TeleoptiPrincipal.Current as TeleoptiPrincipal;
            
            var newRole = mocks.StrictMock<IApplicationRole>();
            var unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            var claimSet = new DefaultClaimSet();
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.AddApplicationRole(newRole);
                
            using(mocks.Record())
            {
                Expect.Call(roleToClaimSetTransformer.Transform(newRole,unitOfWorkFactory)).Return(claimSet);
                Expect.Call(personRepository.Get(Guid.Empty)).IgnoreArguments().Return(person);
            }
            using (mocks.Playback())
            {
                target.Execute(currentPrincipal, unitOfWorkFactory, personRepository);
                
                TeleoptiPrincipal.Current.ClaimSets.Contains(claimSet).Should().Be.True();
            }

            ((TeleoptiPrincipal) TeleoptiPrincipal.Current).ChangePrincipal(currentPrincipal);
        }
    }
}