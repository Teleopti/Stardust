using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests JobResultRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class JobResultRepositoryTest : RepositoryTest<IJobResult>
    {
        private IPerson person;
        private DateOnlyPeriod period;
        private DateTime timestamp;

        protected override void ConcreteSetup()
        {
            period = new DateOnlyPeriod(2011, 8, 1, 2011, 8, 31);
            person = PersonFactory.CreatePerson();
            timestamp = DateTime.UtcNow;
            PersistAndRemoveFromUnitOfWork(person);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IJobResult CreateAggregateWithCorrectBusinessUnit()
        {
            var jobResult = new JobResult(JobCategory.QuickForecast,period,person,timestamp);
            jobResult.AddDetail(new JobResultDetail(DetailLevel.Warning,"No beer in fridge",timestamp.AddMinutes(1),new PermissionException("Action not allowed.",new PermissionException("Not authorized to buy beer."))));
            return jobResult;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IJobResult loadedAggregateFromDatabase)
        {
            IJobResult org = CreateAggregateWithCorrectBusinessUnit();
            org.Details.Single().InnerExceptionMessage.Should().Be.EqualTo(
                loadedAggregateFromDatabase.Details.Single().InnerExceptionMessage);
            org.JobCategory.Should().Be.EqualTo(loadedAggregateFromDatabase.JobCategory);
            org.Owner.Should().Be.EqualTo(loadedAggregateFromDatabase.Owner);
            org.Period.Should().Be.EqualTo(loadedAggregateFromDatabase.Period);
        }

		[Test]
		public void ShouldLoadWithPaging()
		{
			var item = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(item);

			IJobResultRepository repository = new JobResultRepository(UnitOfWork);
			var pagingDetail = new PagingDetail {Skip = 0, Take = 10};
			var result = repository.LoadHistoryWithPaging(pagingDetail, JobCategory.QuickForecast);
			result.Count.Should().Be.EqualTo(1);
			pagingDetail.TotalNumberOfResults.Should().Be.EqualTo(1);
		}

        protected override Repository<IJobResult> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new JobResultRepository(currentUnitOfWork);
        }
    }
}
