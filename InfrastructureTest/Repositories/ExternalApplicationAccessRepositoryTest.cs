using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class ExternalApplicationAccessRepositoryTest : RepositoryTest<IExternalApplicationAccess>
    {
        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IExternalApplicationAccess CreateAggregateWithCorrectBusinessUnit()
        {
			return new ExternalApplicationAccess{CreatedOn = DateTime.UtcNow,Hash = Guid.NewGuid().ToString(), Name = "HR system", PersonId = LoggedOnPerson.Id.GetValueOrDefault()};
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IExternalApplicationAccess loadedAggregateFromDatabase)
        {
        }

        protected override Repository<IExternalApplicationAccess> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new ExternalApplicationAccessRepository(currentUnitOfWork);
        }
    }
}
