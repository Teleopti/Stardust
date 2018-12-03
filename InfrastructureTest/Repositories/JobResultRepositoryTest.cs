using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests JobResultRepository
    ///</summary>
    [TestFixture]
    [Category("BucketB")]
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
            jobResult.AddDetail(new JobResultDetail(DetailLevel.Warning,RandomString.Make(10001),timestamp.AddMinutes(1),new PermissionException("Action not allowed.",new PermissionException("Not authorized to buy beer."))));
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

			IJobResultRepository repository = new JobResultRepository(CurrUnitOfWork);
			var pagingDetail = new PagingDetail {Skip = 0, Take = 10};
			var result = repository.LoadHistoryWithPaging(pagingDetail, JobCategory.QuickForecast);
			result.Count.Should().Be.EqualTo(1);
			pagingDetail.TotalNumberOfResults.Should().Be.EqualTo(1);
		}

	    [Test]
	    public void ShouldBeAbleToPersistArtifact()
	    {
			var item = CreateAggregateWithCorrectBusinessUnit();
			var artifact = new JobResultArtifact(JobResultArtifactCategory.Input, "test.xls", Encoding.ASCII.GetBytes("test"));
			item.AddArtifact(artifact);
			PersistAndRemoveFromUnitOfWork(item);
			PersistAndRemoveFromUnitOfWork(artifact);

			var repository = new JobResultRepository(CurrUnitOfWork);
		    var result = repository.LoadAll().First();

			LazyLoadingManager.IsInitialized(result.Artifacts).Should().Be.False();
		    var savedArtifact = result.Artifacts.Single();
			LazyLoadingManager.IsInitialized(result.Artifacts).Should().Be.True();
		    savedArtifact.Name.Should().Be.EqualTo("test.xls");
		    savedArtifact.Content.Should().Have.SameSequenceAs(Encoding.ASCII.GetBytes("test"));
		    savedArtifact.Category.Should().Be(JobResultArtifactCategory.Input);
	    }
		
        protected override Repository<IJobResult> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new JobResultRepository(currentUnitOfWork);
        }
    }
}
