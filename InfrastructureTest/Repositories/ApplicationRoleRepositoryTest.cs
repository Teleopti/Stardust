using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class ApplicationRoleRepositoryTest : RepositoryTest<IApplicationRole>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IApplicationRole CreateAggregateWithCorrectBusinessUnit()
        {
            IApplicationRole ret= new ApplicationRole();
            ret.DescriptionText = "hej och";
            ret.Name = "ha";
            ret.SetBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
            return ret;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IApplicationRole loadedAggregateFromDatabase)
        {
            Assert.AreEqual(CreateAggregateWithCorrectBusinessUnit().DescriptionText,
                            loadedAggregateFromDatabase.DescriptionText);
        }

        protected override Repository<IApplicationRole> TestRepository(IUnitOfWork unitOfWork)
        {
            return new ApplicationRoleRepository(unitOfWork);
        }

        /// <summary>
        /// Verifies Get all application roles works.
        /// </summary>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 08-04-2008
        /// </remarks>
        [Test]
        public void VerifyGetAllApplicationRoles()
        {
            const int roleCount = 5;
            for (var i = 0; i < roleCount; i++)
            {
                var appRole = ApplicationRoleFactory.CreateRole("Role " + i, "Role Description " + i);
                var applicationFunction = new ApplicationFunction("Test");
                var applicationFunction1 = new ApplicationFunction("Test2");
              
                PersistAndRemoveFromUnitOfWork(applicationFunction);
                PersistAndRemoveFromUnitOfWork(applicationFunction1);
               
                appRole.AddApplicationFunction(applicationFunction);
                appRole.AddApplicationFunction(applicationFunction1);

                PersistAndRemoveFromUnitOfWork(appRole);
             }

            var rolesList = new ApplicationRoleRepository(UnitOfWork).LoadAllApplicationRolesSortedByName();
            Assert.AreEqual(roleCount, rolesList.Count);

            Assert.IsTrue(LazyLoadingManager.IsInitialized(rolesList[0].BusinessUnit));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(rolesList[0].ApplicationFunctionCollection));
        }

    }
}
